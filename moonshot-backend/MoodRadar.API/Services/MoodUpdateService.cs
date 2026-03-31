using MoodRadar.API.Services;
using Microsoft.Extensions.DependencyInjection;
using MoodRadar.API.Models;

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

                    _logger.LogInformation("[{Time}] Running mood update pipeline", DateTime.UtcNow);

                    // 1️⃣ Ticketmaster (may be null if not implemented yet)
                    var events = ticketmaster != null
                        ? await ticketmaster.PollEindhovenEventsAsync(stoppingToken)
                        : new List<TicketmasterEvent>();

                    // 2️⃣ PSV Matches
                    var matches = football != null
                        ? await football.GetPsvMatchesAsync()
                        : new List<PsvMatch>();

                    // 3️⃣ Weather
                    var weatherData = weather != null
                        ? await weather.FetchEindhovenWeatherAsync(stoppingToken)
                        : new object();

                    // 4️⃣ Holidays
                    var holidays = holiday != null
                        ? await holiday.GetDutchHolidays2026Async()
                        : new List<Holiday>();

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