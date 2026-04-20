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

    private readonly TimeSpan _interval = TimeSpan.FromMinutes(60);

    public ZoneSnapshotWorker(
        IServiceProvider serviceProvider,
        ILogger<ZoneSnapshotWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
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

                // 1️⃣ Fetch external data
                var events = await ticketService.PollEindhovenEventsAsync(stoppingToken);
                var matches = await footballService.GetPsvMatchesAsync();
                var weather = await weatherService.FetchEindhovenWeatherAsync(stoppingToken);

                _logger.LogInformation("Fetched: {Events} events, {Matches} matches, {Weather} weather entries",
                    events.Count, matches.Count, weather.Count);

                // 2️⃣ Run ML (THIS should store NeighborhoodSnapshots)
                var zonesUpdated = await predictionService.PredictAndStoreAsync(stoppingToken);

                // 3️⃣ Log run
                db.ModelRuns.Add(new ModelRun
                {
                    RunAt = DateTime.UtcNow,
                    ZonesUpdated = zonesUpdated,
                    Errors = ""
                });

                await db.SaveChangesAsync(stoppingToken);

                _logger.LogInformation("CRON SUCCESS {RunId}", runId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CRON FAILED {RunId}", runId);

                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                db.ModelRuns.Add(new ModelRun
                {
                    RunAt = DateTime.UtcNow,
                    ZonesUpdated = 0,
                    Errors = ex.Message
                });

                await db.SaveChangesAsync(stoppingToken);
            }

            var elapsed = DateTime.UtcNow - startTime;
            var delay = _interval - elapsed;

            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, stoppingToken);
        }
    }
}