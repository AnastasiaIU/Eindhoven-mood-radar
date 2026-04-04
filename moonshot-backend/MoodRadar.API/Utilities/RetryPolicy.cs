namespace MoodRadar.API.Utilities;

/// <summary>
/// Retry policy with exponential backoff for external API calls.
/// Handles transient failures gracefully.
/// </summary>
public class RetryPolicy
{
    private readonly ILogger _logger;
    private readonly int _maxRetries;
    private readonly int _initialDelayMs;

    /// <summary>
    /// Create a retry policy with exponential backoff.
    /// </summary>
    /// <param name="logger">Logger for retry attempts</param>
    /// <param name="maxRetries">Maximum number of retries (default: 3)</param>
    /// <param name="initialDelayMs">Initial delay in milliseconds (default: 1000)</param>
    public RetryPolicy(ILogger logger, int maxRetries = 3, int initialDelayMs = 1000)
    {
        _logger = logger;
        _maxRetries = maxRetries;
        _initialDelayMs = initialDelayMs;
    }

    /// <summary>
    /// Execute an async operation with exponential backoff retries.
    /// </summary>
    /// <typeparam name="T">Return type of the operation</typeparam>
    /// <param name="operation">Async function to execute</param>
    /// <param name="operationName">Name of the operation (for logging)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the operation, or default(T) if all retries fail</returns>
    public async Task<T?> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        string operationName,
        CancellationToken cancellationToken = default) where T : class
    {
        int attemptCount = 0;
        int delayMs = _initialDelayMs;

        while (attemptCount <= _maxRetries)
        {
            try
            {
                attemptCount++;
                _logger.LogDebug("Attempt {AttemptNumber}/{MaxRetries} for {OperationName}", attemptCount, _maxRetries + 1, operationName);
                
                return await operation(cancellationToken);
            }
            catch (HttpRequestException ex) when (attemptCount <= _maxRetries && IsTransientError(ex))
            {
                _logger.LogWarning(
                    "Transient error on attempt {AttemptNumber}/{MaxRetries} for {OperationName}: {StatusCode}. Retrying in {DelayMs}ms",
                    attemptCount, _maxRetries + 1, operationName, ex.StatusCode, delayMs);

                if (attemptCount <= _maxRetries)
                {
                    await Task.Delay(delayMs, cancellationToken);
                    delayMs = (int)(delayMs * 1.5); // Exponential backoff: 1.5x multiplier
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Operation {OperationName} was cancelled", operationName);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Non-transient error on {OperationName}", operationName);
                throw; // Non-transient errors shouldn't be retried
            }
        }

        _logger.LogError("Failed to execute {OperationName} after {MaxRetries} retries", operationName, _maxRetries + 1);
        return null;
    }

    /// <summary>
    /// Determine if an HTTP error is transient (retryable).
    /// </summary>
    private static bool IsTransientError(HttpRequestException ex)
    {
        // Transient errors: timeouts, server errors (5xx), too many requests (429)
        if (ex.StatusCode == null)
            return true; // Network error (connection timeout, DNS failure, etc.)

        return (int)ex.StatusCode >= 500 || (int)ex.StatusCode == 429 || (int)ex.StatusCode == 408;
    }
}
