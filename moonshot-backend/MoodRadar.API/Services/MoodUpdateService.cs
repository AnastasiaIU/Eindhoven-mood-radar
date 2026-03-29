using MoodRadar.API.Services;

namespace MoodRadar.API.Services
{
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

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();

                    var football = scope.ServiceProvider.GetService<FootballService>();
                    var holiday = scope.ServiceProvider.GetService<HolidayService>();
                    var ticketmaster = scope.ServiceProvider.GetService<TicketmasterService>();
                    var weather = scope.ServiceProvider.GetService<WeatherService>();
                    var ml = scope.ServiceProvider.GetService<MLService>();

                    _logger.LogInformation("[{Time}] Running mood update pipeline", DateTime.UtcNow);

                    // 1?? Ticketmaster (may be null if not implemented yet)
                    var events = ticketmaster != null
                        ? await ticketmaster.GetEventsAsync()
                        : new List<object>();

                    // 2?? PSV Matches
                    var matches = football != null
                        ? await football.GetPsvMatchesAsync()
                        : new List<object>();

                    // 3?? Weather
                    var weatherData = weather != null
                        ? await weather.GetWeatherAsync()
                        : new object();

                    // 4?? Holidays
                    var holidays = holiday != null
                        ? await holiday.GetDutchHolidays2026Async()
                        : new List<object>();

                    // 5?? ML Prediction
                    if (ml != null)
                    {
                        var input = new
                        {
                            Events = events,
                            Matches = matches,
                            Weather = weatherData,
                            Holidays = holidays
                        };

                        var prediction = await ml.PredictAsync(input);

                        _logger.LogInformation("ML prediction result: {Result}", prediction);

                        // TODO: Save to database (zone_snapshots)
                    }
                    else
                    {
                        _logger.LogWarning("ML service not available yet.");
                    }

                    _logger.LogInformation("Mood update SUCCESS at {Time}", DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Mood update FAILED");
                }

                await Task.Delay(TimeSpan.FromMinutes(_intervalMinutes), stoppingToken);
            }

            _logger.LogInformation("Mood Update Service stopped.");
        }
    }
}