namespace MoodRadar.API.Services;

using MoodRadar.API.Models;

/// <summary>
/// Background service that runs a mood update job every 15 minutes.
/// Also runs venue scraping once per day.
/// 
/// Pattern: All services (Ticketmaster, VenueScraper, Football, Weather) are injected
/// directly and called as methods, consistent with EventsController pattern.
/// </summary>
public class MoodUpdateService : BackgroundService
{
    private readonly ILogger<MoodUpdateService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly bool _isDevelopment;
    private readonly int _intervalMinutes = 15;
    private DateTime _lastDailyScrapingTime = DateTime.MinValue; // Track last daily scraping

    public MoodUpdateService(
        ILogger<MoodUpdateService> logger,
        IServiceProvider serviceProvider,
        IHostEnvironment hostEnvironment)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _isDevelopment = hostEnvironment.IsDevelopment();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Mood Update Service started. Running every {Minutes} minutes. Daily scraping scheduled once per 24h.",
            _intervalMinutes);

        if (_isDevelopment)
        {
            _logger.LogInformation("Development environment detected: daily venue scraping is disabled.");
        }

        // Run immediately on startup
        await RunMoodUpdateAsync(stoppingToken);

        // Then run on schedule every 15 minutes
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(_intervalMinutes), stoppingToken);
            await RunMoodUpdateAsync(stoppingToken);
        }

        _logger.LogInformation("Mood Update Service stopped.");
    }

    private async Task RunMoodUpdateAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();

            var football = scope.ServiceProvider.GetService<FootballService>();
            var holiday = scope.ServiceProvider.GetService<HolidayService>();
            var ticketmaster = scope.ServiceProvider.GetService<TicketmasterService>();
            var weather = scope.ServiceProvider.GetService<WeatherService>();
            var moodPrediction = scope.ServiceProvider.GetService<MoodPredictionService>();
            var venueScraper = scope.ServiceProvider.GetService<IVenueScraperService>();

            _logger.LogInformation("[{Time}] Running mood update pipeline", DateTime.UtcNow);

            // Ticketmaster
            if (ticketmaster != null)
                await ticketmaster.PollEindhovenEventsAsync(stoppingToken);

            // Daily Venue Scraping (once per 24 hours)
            // Runs if 24+ hours have passed since last scraping
            if (!_isDevelopment && venueScraper != null && ShouldRunDailyScraping())
            {
                _logger.LogInformation("Running venue scraping for Uit in Eindhoven");
                try
                {
                    await venueScraper.ScrapeAllVenuesAsync(stoppingToken);
                    _lastDailyScrapingTime = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Venue scraping failed, but continuing other updates");
                }
            }

            // PSV Matches
            var matches = football != null
                ? await football.GetPsvMatchesAsync()
                : new List<PsvMatch>();

            // Weather
            if (weather != null)
                await weather.FetchEindhovenWeatherAsync(stoppingToken);

            // Holidays
            var holidays = holiday != null
                ? await holiday.GetDutchHolidays2026Async()
                : new List<Holiday>();

            // Mood Predictions (mock for Phase 1)
            if (moodPrediction != null)
                await moodPrediction.PredictAndStoreAsync(stoppingToken);

            _logger.LogInformation("Mood update SUCCESS at {Time}", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mood update FAILED");
        }
    }



    /// <summary>
    /// Check if 24+ hours have passed since last daily scraping.
    /// </summary>
    private bool ShouldRunDailyScraping()
    {
        var timeSinceLastScraping = DateTime.UtcNow - _lastDailyScrapingTime;
        return timeSinceLastScraping.TotalHours >= 24;
    }
}
