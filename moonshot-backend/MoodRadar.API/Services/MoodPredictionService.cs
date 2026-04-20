using MoodRadar.API.Data;
using MoodRadar.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace MoodRadar.API.Services;

public class MoodPredictionService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MoodPredictionService> _logger;

    public MoodPredictionService(IServiceProvider serviceProvider, ILogger<MoodPredictionService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<int> PredictAndStoreAsync(CancellationToken cancellationToken = default)
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

            // Load existing snapshots (tracked)
            var existingSnapshots = await dbContext.NeighborhoodSnapshots
                .Where(s => s.Timestamp >= forecastStartUtc && s.Timestamp < forecastEndUtcExclusive)
                .ToListAsync(cancellationToken);

            // Normalize timestamps
            foreach (var s in existingSnapshots)
            {
                s.Timestamp = new DateTime(
                    s.Timestamp.Year,
                    s.Timestamp.Month,
                    s.Timestamp.Day,
                    s.Timestamp.Hour,
                    0,
                    0,
                    DateTimeKind.Utc);
            }

            var existingMap = existingSnapshots
                .ToDictionary(x => (x.NeighborhoodId, x.Timestamp));

            // Fetch events
            var eventsInWindow = await dbContext.Events
                .AsNoTracking()
                .Where(e => e.NeighborhoodId.HasValue
                            && e.StartTime >= forecastStartUtc
                            && e.StartTime < forecastEndUtcExclusive.AddHours(24))
                .ToListAsync(cancellationToken);

            var eventsByNeighborhood = eventsInWindow
                .GroupBy(e => e.NeighborhoodId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Fetch latest weather
            var latestWeather = await dbContext.Weathers
                .AsNoTracking()
                .OrderByDescending(w => w.SnapshotHour)
                .FirstOrDefaultAsync(cancellationToken);

            var predictions = new List<NeighborhoodSnapshot>();
            int updatedCount = 0;

            foreach (var neighborhood in neighborhoods)
            {
                var neighborhoodEvents = eventsByNeighborhood.TryGetValue(neighborhood.Id, out var value)
                    ? value
                    : new List<Event>();

                for (int hourOffset = 0; hourOffset < 24; hourOffset++)
                {
                    var predictionTimestamp = forecastStartUtc.AddHours(hourOffset);

                    var prediction = PredictMoodForNeighborhood(
                        neighborhood,
                        predictionTimestamp,
                        neighborhoodEvents,
                        latestWeather);

                    var key = (prediction.NeighborhoodId, prediction.Timestamp);

                    if (existingMap.TryGetValue(key, out var existing))
                    {
                        existing.MoodLabel = prediction.MoodLabel;
                        existing.Confidence = prediction.Confidence;
                        existing.FeatureJson = prediction.FeatureJson;
                        updatedCount++;
                    }
                    else
                    {
                        predictions.Add(prediction);
                    }
                }
            }

            // Insert new snapshots
            if (predictions.Count > 0)
            {
                dbContext.NeighborhoodSnapshots.AddRange(predictions);
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Stored {NewCount} new snapshots, updated {UpdatedCount}",
                predictions.Count,
                updatedCount);

            return predictions.Count + updatedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating mood predictions");
            return 0;
        }
    }

    private NeighborhoodSnapshot PredictMoodForNeighborhood(
        Neighborhood neighborhood,
        DateTime timestamp,
        IReadOnlyCollection<Event> neighborhoodEvents,
        Weather? latestWeather)
    {
        var horizonEnd = timestamp.AddHours(24);

        var upcomingEvents = neighborhoodEvents
            .Where(e => e.StartTime >= timestamp && e.StartTime < horizonEnd)
            .ToList();

        int eventCount = upcomingEvents.Count;

        bool hasLargeEvent = upcomingEvents.Any(e =>
            e.Title != null &&
            (
                e.Title.Contains("PSV", StringComparison.OrdinalIgnoreCase) ||
                e.Title.Contains("Football", StringComparison.OrdinalIgnoreCase) ||
                e.Title.Contains("Concert", StringComparison.OrdinalIgnoreCase)
            ));

        bool hasPsvMatch = upcomingEvents.Any(e =>
            e.Title != null &&
            e.Title.Contains("PSV", StringComparison.OrdinalIgnoreCase));

        var (moodLabel, confidence) = DetermineMood(
            eventCount,
            hasLargeEvent,
            hasPsvMatch,
            latestWeather,
            false,
            timestamp);

        return new NeighborhoodSnapshot
        {
            NeighborhoodId = neighborhood.Id,
            Timestamp = timestamp,
            MoodLabel = moodLabel,
            Confidence = confidence,
            FeatureJson = new Dictionary<string, object>
            {
                { "event_count", eventCount },
                { "has_large_event", hasLargeEvent },
                { "has_psv_match", hasPsvMatch },
                { "temperature_c", latestWeather?.TemperatureC ?? 15.0 },
                { "is_holiday", false },
                { "hour_of_day", timestamp.Hour },
                { "is_stale", latestWeather == null || (timestamp - latestWeather.SnapshotHour).TotalMinutes > 60 }
            }
        };
    }

    private static (DateTime Start, DateTime End) GetForecastWindowUtc()
    {
        var utcNow = DateTime.UtcNow;

        var nextHour = new DateTime(
            utcNow.Year,
            utcNow.Month,
            utcNow.Day,
            utcNow.Hour,
            0,
            0,
            DateTimeKind.Utc).AddHours(1);

        return (nextHour, nextHour.AddHours(24));
    }

    private (string moodLabel, double confidence) DetermineMood(
        int eventCount,
        bool hasLargeEvent,
        bool hasPsvMatch,
        Weather? weather,
        bool isHoliday,
        DateTime timestamp)
    {
        double temp = weather?.TemperatureC ?? 15.0;
        int hour = timestamp.Hour;

        if (hasPsvMatch)
            return ("Intense", 0.95);

        if (hasLargeEvent && eventCount > 3)
            return ("Intense", 0.85);

        if (eventCount >= 3 && hour >= 8 && hour < 18)
            return ("Busy", 0.80);

        if (eventCount >= 5 && temp >= 18 && hour >= 18)
            return ("Energetic", 0.80);

        if (eventCount <= 2 && temp >= 15 && hour >= 18)
            return ("Relaxed", 0.70);

        if (eventCount == 0 && (hour < 8 || hour >= 23))
            return ("Calm", 0.85);

        if (temp < 10 && eventCount <= 1)
            return ("Calm", 0.75);

        if (isHoliday && eventCount >= 2)
            return ("Energetic", 0.75);

        if (eventCount == 0)
            return ("Calm", 0.60);

        return ("Relaxed", 0.65);
    }
}