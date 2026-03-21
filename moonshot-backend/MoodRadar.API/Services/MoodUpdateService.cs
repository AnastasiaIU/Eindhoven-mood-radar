namespace MoodRadar.API.Services;

/// <summary>
/// Background service that runs a mood update job every 15 minutes.
/// Phase 1: Logs 'running mood update' to console.
/// Phase 2: Will call the ML service to generate mood predictions.
/// </summary>
public class MoodUpdateService : BackgroundService
{
    private readonly ILogger<MoodUpdateService> _logger;
    private readonly int _intervalMinutes = 15;

    public MoodUpdateService(ILogger<MoodUpdateService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Mood Update Service starting. Will run every {IntervalMinutes} minutes.", _intervalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Phase 1: Log the mood update task
                _logger.LogInformation("[{Timestamp}] Running mood update", DateTime.UtcNow);

                // Phase 2 placeholder: This is where we'll call the ML service
                // var moodPredictions = await _mlService.PredictMoodsAsync();
                // await _database.UpdateZoneSnapshotsAsync(moodPredictions);

                // Wait for 15 minutes before next run
                await Task.Delay(TimeSpan.FromMinutes(_intervalMinutes), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Mood Update Service is stopping due to cancellation request.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in Mood Update Service.");
                // Continue running even if an error occurs
                await Task.Delay(TimeSpan.FromMinutes(_intervalMinutes), stoppingToken);
            }
        }

        _logger.LogInformation("Mood Update Service has stopped.");
    }
}
