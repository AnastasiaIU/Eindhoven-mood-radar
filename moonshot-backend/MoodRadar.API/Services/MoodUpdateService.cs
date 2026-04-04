namespace MoodRadar.API.Services;

using MoodRadar.API.Models;

/// <summary>
/// Background service that runs a mood update job every 15 minutes.
/// </summary>
public class MoodUpdateService : BackgroundService
{
    private readonly ILogger<MoodUpdateService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly int _intervalMinutes = 15;

    public MoodUpdateService(
        ILogger<MoodUpdateService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Mood Update Service started. Running every {Minutes} minutes.", _intervalMinutes);

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

            _logger.LogInformation("[{Time}] Running mood update pipeline", DateTime.UtcNow);

            // 1️⃣ Ticketmaster
            if (ticketmaster != null)
                await ticketmaster.PollEindhovenEventsAsync(stoppingToken);

            // 2️⃣ PSV Matches
            var matches = football != null
                ? await football.GetPsvMatchesAsync()
                : new List<PsvMatch>();

            // 3️⃣ Weather
            if (weather != null)
                await weather.FetchEindhovenWeatherAsync(stoppingToken);

            // 4️⃣ Holidays
            var holidays = holiday != null
                ? await holiday.GetDutchHolidays2026Async()
                : new List<Holiday>();

            // 5️⃣ Mood Predictions (mock for Phase 1)
            if (moodPrediction != null)
                await moodPrediction.PredictAndStoreAsync(stoppingToken);

            _logger.LogInformation("Mood update SUCCESS at {Time}", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mood update FAILED");
        }
    }
}
