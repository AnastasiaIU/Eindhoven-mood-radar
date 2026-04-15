using MoodRadar.API.Data;
using MoodRadar.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace MoodRadar.API.Services;
/// <summary>
/// Mock mood prediction service for Phase 1.
/// Generates deterministic mood labels based on aggregated data.
/// Will be replaced with real ML model in Phase 2.
/// 
/// Mood rules (simple heuristics):
/// - Energetic: Many events (>5) + warm weather (>18°C) + evening (18:00-23:59)
/// - Intense: PSV match + large events + crowd indicators
/// - Busy: Many events (3-5) + daytime (08:00-17:59)
/// - Relaxed: Few events (1-2) + mild weather + evening
/// - Calm: No events + quiet hours (23:00-08:00) or cold (<10°C)
/// </summary>
public class MoodPredictionService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MoodPredictionService> _logger;

    public MoodPredictionService(IServiceProvider serviceProvider, ILogger<MoodPredictionService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Generate 24-hour hourly mood forecasts for all neighborhoods and store in database.
    /// Called by cron job after fetching all data sources.
    /// </summary>
    public async Task PredictAndStoreAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var neighborhoods = await dbContext.Neighborhoods.ToListAsync(cancellationToken);
            var (forecastStartUtc, forecastEndUtcExclusive) = GetForecastWindowUtc();

            _logger.LogInformation(
                "Generating hourly mood forecasts for {Count} neighborhoods ({Start} to {End})",
                neighborhoods.Count,
                forecastStartUtc,
                forecastEndUtcExclusive);

            // Preload existing snapshots (IMPORTANT for UPSERT)
            var existingSnapshots = await dbContext.NeighborhoodSnapshots
                .Where(s => s.Timestamp >= forecastStartUtc && s.Timestamp < forecastEndUtcExclusive)
                .Select(s => new NeighborhoodSnapshot
                {
                    Id = s.Id,
                    NeighborhoodId = s.NeighborhoodId,
                    Timestamp = DateTime.SpecifyKind(
                        new DateTime(s.Timestamp.Year, s.Timestamp.Month, s.Timestamp.Day, s.Timestamp.Hour, 0, 0),
                        DateTimeKind.Utc),
                    MoodLabel = s.MoodLabel,
                    Confidence = s.Confidence,
                    FeatureJson = s.FeatureJson
                })
                .ToListAsync(cancellationToken);

            var existingMap = existingSnapshots
                .ToDictionary(x => (x.NeighborhoodId, x.Timestamp));

            var eventsInWindow = await dbContext.Events
                .AsNoTracking()
                .Where(e => e.NeighborhoodId.HasValue
                            && e.StartTime >= forecastStartUtc
                            && e.StartTime < forecastEndUtcExclusive.AddHours(24))
                .ToListAsync(cancellationToken);

            var eventsByNeighborhood = eventsInWindow
                .GroupBy(e => e.NeighborhoodId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            var latestWeather = await dbContext.Weathers
                .AsNoTracking()
                .OrderByDescending(w => w.SnapshotHour)
                .FirstOrDefaultAsync(cancellationToken);

            var predictions = new List<NeighborhoodSnapshot>();

            foreach (var neighborhood in neighborhoods)
            {
                var neighborhoodEvents = eventsByNeighborhood.TryGetValue(neighborhood.Id, out var value)
                    ? value
                    : new List<Event>();

                for (var hourOffset = 0; hourOffset < 24; hourOffset++)
                {
                    var predictionTimestamp = new DateTime(
                        forecastStartUtc.Year,
                        forecastStartUtc.Month,
                        forecastStartUtc.Day,
                        forecastStartUtc.Hour,
                        0,
                        0,
                        DateTimeKind.Utc
                    ).AddHours(hourOffset);

                    var prediction = PredictMoodForNeighborhood(
                        neighborhood,
                        predictionTimestamp,
                        neighborhoodEvents,
                        latestWeather);

                    var key = (prediction.NeighborhoodId, prediction.Timestamp); ;

                    if (existingMap.TryGetValue(key, out var existing))
                    {
                        existing.MoodLabel = prediction.MoodLabel;
                        existing.Confidence = prediction.Confidence;
                        existing.FeatureJson = prediction.FeatureJson;
                    }
                    else
                    {
                        predictions.Add(prediction);
                    }
                }
            }

            // Only insert NEW ones
            if (predictions.Count > 0)
            {
                dbContext.NeighborhoodSnapshots.AddRange(predictions);
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Stored {Count} new snapshots and updated existing ones",
                predictions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating mood predictions");
        }
    }

    /// <summary>
    /// Generate a single mood prediction for a neighborhood at a specific hour.
    /// </summary>
    private NeighborhoodSnapshot PredictMoodForNeighborhood(
        Neighborhood neighborhood,
        DateTime timestamp,
        IReadOnlyCollection<Event> neighborhoodEvents,
        Weather? latestWeather)
    {
        // Forecast based on events occurring in the next 24 hours from this timestamp.
        var horizonEnd = timestamp.AddHours(24);
        var upcomingEvents = neighborhoodEvents
            .Where(e => e.StartTime >= timestamp && e.StartTime < horizonEnd)
            .ToList();

        var eventCount = upcomingEvents.Count;

        var hasLargeEvent = upcomingEvents.Any(e =>
            e.Title.Contains("PSV", StringComparison.OrdinalIgnoreCase)
            || e.Title.Contains("Football", StringComparison.OrdinalIgnoreCase)
            || e.Title.Contains("Concert", StringComparison.OrdinalIgnoreCase));

        // var isHoliday = await dbContext.Holidays
        //     .AnyAsync(h => h.Date.Date == timestamp.Date, cancellationToken);

        // Simple mood rules
        var (moodLabel, confidence) = DetermineMood(
            eventCount, hasLargeEvent, false/*hasPsvMatch*/, latestWeather, false/*isHoliday*/, timestamp
        );

        var prediction = new NeighborhoodSnapshot
        {
            NeighborhoodId = neighborhood.Id,
            Timestamp = timestamp,
            MoodLabel = moodLabel,
            Confidence = confidence,
            FeatureJson = new Dictionary<string, object>
            {
                { "event_count", eventCount },
                { "has_large_event", hasLargeEvent },
                { "has_psv_match", false/*hasPsvMatch*/ },
                { "temperature_c", latestWeather?.TemperatureC ?? 15.0 },
                { "is_holiday", false/*isHoliday*/ },
                { "hour_of_day", timestamp.Hour },
                // Flag data as stale if weather is older than 1 hour
                { "is_stale", latestWeather == null || (timestamp - latestWeather.SnapshotHour).TotalMinutes > 60 }
            }
        };

        return prediction;
    }

    private static (DateTime ForecastStartUtc, DateTime ForecastEndUtcExclusive) GetForecastWindowUtc()
    {
        var utcNow = DateTime.UtcNow;
        var nextHourUtc = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, 0, 0, DateTimeKind.Utc)
            .AddHours(1);

        return (nextHourUtc, nextHourUtc.AddHours(24));
    }

    /// <summary>
    /// Determine mood label based on aggregated features.
    /// Returns (mood_label, confidence_score).
    /// </summary>
    private (string moodLabel, double confidence) DetermineMood(
        int eventCount,
        bool hasLargeEvent,
        bool hasPsvMatch,
        Weather? weather,
        bool isHoliday,
        DateTime timestamp)
    {
        var temp = weather?.TemperatureC ?? 15.0;
        var hour = timestamp.Hour;
        var dayOfWeek = timestamp.DayOfWeek;

        // Weekend multiplier
        var isWeekend = dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday;

        // PSV match → Intense
        if (hasPsvMatch)
        {
            return ("Intense", 0.95);
        }

        // Large event (concert, festival) → Intense
        if (hasLargeEvent && eventCount > 3)
        {
            return ("Intense", 0.85);
        }

        // Many events (3+) during day → Busy
        if (eventCount >= 3 && hour >= 8 && hour < 18)
        {
            return ("Busy", 0.80);
        }

        // Many events (5+) + warm evening → Energetic
        if (eventCount >= 5 && temp >= 18 && hour >= 18 && hour < 24)
        {
            return ("Energetic", 0.80);
        }

        // Few events + warm + evening → Relaxed
        if (eventCount <= 2 && temp >= 15 && hour >= 18)
        {
            return ("Relaxed", 0.70);
        }

        // No events + night time → Calm
        if (eventCount == 0 && (hour < 8 || hour >= 23))
        {
            return ("Calm", 0.85);
        }

        // Cold weather → Calm
        if (temp < 10 && eventCount <= 1)
        {
            return ("Calm", 0.75);
        }

        // Holiday modifier
        if (isHoliday && eventCount >= 2)
        {
            return ("Energetic", 0.75);
        }

        // Default: Relaxed or Calm based on event count
        if (eventCount == 0)
        {
            return ("Calm", 0.60);
        }

        return ("Relaxed", 0.65);
    }
}
