using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoodRadar.API.Data;
using MoodRadar.API.Models;
using MoodRadar.API.Models.Domain;
using MoodRadar.API.Models.Integrations;
using MoodRadar.API.Services;
using System.Text;
using System.Text.Json;

public class ZoneSnapshotWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ZoneSnapshotWorker> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly TimeSpan _interval = TimeSpan.FromMinutes(60);

    public ZoneSnapshotWorker(IServiceProvider serviceProvider,ILogger<ZoneSnapshotWorker> logger,IHttpClientFactory httpClientFactory)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ZoneSnapshotWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var startTime = DateTime.UtcNow;
            var runId = Guid.NewGuid();

            try
            {
                using var scope = _serviceProvider.CreateScope();

                var ticketService = scope.ServiceProvider.GetRequiredService<ITicketmasterService>();
                var footballService = scope.ServiceProvider.GetRequiredService<FootballService>();
                var weatherService = scope.ServiceProvider.GetRequiredService<WeatherService>();
                var predictionService = scope.ServiceProvider.GetRequiredService<MoodPredictionService>();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                _logger.LogInformation("CRON START {RunId}", runId);

                // 1. Fetch external data (ingestion layer)
                var events = await ticketService.PollEindhovenEventsAsync(stoppingToken);
                var matches = await footballService.GetPsvMatchesAsync();
                var weather = await weatherService.FetchEindhovenWeatherAsync(stoppingToken);

                     // 3. Run ML prediction (should internally read from DB)
                await predictionService.PredictAndStoreAsync(stoppingToken);

                // 4. Create snapshot
                db.ZoneSnapshots.Add(new ZoneSnapshot
                {
                    CreatedAt = DateTime.UtcNow,
                    EventCount = events.Count,
                    PsvMatchCount = matches.Count,
                    WeatherSummary = weather.FirstOrDefault()?.TemperatureC.ToString()
                });
                await db.SaveChangesAsync(stoppingToken);

                _logger.LogInformation("CRON SUCCESS {RunId}", runId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CRON FAILED");
            }

            // Maintain strict 15-min interval (no drift)
            var elapsed = DateTime.UtcNow - startTime;
            var delay = _interval - elapsed;

            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, stoppingToken);
        }
    } 
}