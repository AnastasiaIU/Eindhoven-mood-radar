namespace MoodRadar.API.Services;

using MoodRadar.API.Models.Integrations;
using MoodRadar.API.Models.Domain;
using MoodRadar.API.Data;
using MoodRadar.API.Utilities;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Service for polling Ticketmaster Discovery API v2.
/// Fetches events from the API and returns results.
/// 
/// API Documentation: https://developer.ticketmaster.com/products-and-docs/apis/discovery-api/v2/
/// </summary>
public interface ITicketmasterService
{
    /// <summary>
    /// Poll Ticketmaster for all Eindhoven events (next 24 hours).
    /// Fetches all pages, returns results.
    /// Called by: cron job / background service to refresh data.
    /// </summary>
    Task<List<TicketmasterEvent>> PollEindhovenEventsAsync(CancellationToken cancellationToken = default);
}

public class TicketmasterService : ITicketmasterService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TicketmasterService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    // City name for search (Eindhoven)
    private const string CityName = "Eindhoven";

    // API constants
    private const string BaseUrl = "https://app.ticketmaster.com/discovery/v2/";
    private const int MaxPageSize = 50; // Ticketmaster API limit per page
    private const int EventLookAheadHours = 24; // Only fetch events for next 24 hours
    public TicketmasterService(HttpClient httpClient, ILogger<TicketmasterService> logger, IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;

        // Set base address
        _httpClient.BaseAddress = new Uri(BaseUrl);
        
        // Get API key from configuration
        var apiKey = _configuration["Ticketmaster:ApiKey"];

        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("Ticketmaster API key not configured. Configure 'TICKETMASTER__APIKEY' in environment variables.");
        }
    }

    /// <summary>
    /// Poll Ticketmaster for all Eindhoven events across all pages.
    /// Fetches all available events in minimal API calls (typically just 1 call for a city search).
    /// With exponential backoff retry (max 3 retries per page).
    /// </summary>
    public async Task<List<TicketmasterEvent>> PollEindhovenEventsAsync(CancellationToken cancellationToken = default)
    {
        var allEvents = new List<TicketmasterEvent>();
        int currentPage = 0;
        bool hasMorePages = true;
        var retryPolicy = new RetryPolicy(_logger, maxRetries: 3, initialDelayMs: 1000);

        _logger.LogInformation("Starting Ticketmaster poll for Eindhoven events. Time window: next {Hours}h", EventLookAheadHours);

        try
        {
            // Fetch all pages with minimal API calls (typically 1 call for a city-based search)
            // Ticketmaster Discovery API limit: size * page < 1000 (e.g., 50 per page max 20 pages)
            while (hasMorePages && currentPage < 20) // Max 20 pages = 1000 events per poll
            {
                _logger.LogDebug("Fetching page {Page} from Ticketmaster with retries...", currentPage);
                
                // Retry with exponential backoff
                var response = await retryPolicy.ExecuteAsync(
                    ct => FetchPageAsync(currentPage, MaxPageSize, ct),
                    $"Ticketmaster page {currentPage}",
                    cancellationToken
                );

                if (response?.Embedded?.Events == null || !response.Embedded.Events.Any())
                {
                    _logger.LogInformation("No more events. Completed after fetching {PageCount} page(s).", currentPage);
                    hasMorePages = false;
                    break;
                }

                allEvents.AddRange(response.Embedded.Events);
                _logger.LogDebug("Fetched page {Page}: {Count} events (Total: {Total})",
                    currentPage, response.Embedded.Events.Count, allEvents.Count);

                // Check if there are more pages
                if (response?.Page != null && (currentPage + 1) < response.Page.TotalPages)
                {
                    currentPage++;
                    // Rate limiting: small delay between requests (Ticketmaster: 5 req/sec max)
                    await Task.Delay(300, cancellationToken);
                }
                else
                {
                    hasMorePages = false;
                }
            }

            // Return fetched results
            _logger.LogInformation("Poll completed. Fetched {Count} total events.", allEvents.Count);
            
            // Save to database
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // Only clear old events if we fetched new ones; preserve old events if API failed
                    if (allEvents.Any())
                    {
                        // Clear old events first (keep only next 24 hours)
                        var cutoffTime = DateTime.UtcNow;
                        var oldRecords = dbContext.Events.Where(e => e.StartTime < cutoffTime).ToList();
                        if (oldRecords.Any())
                        {
                            dbContext.Events.RemoveRange(oldRecords);
                            _logger.LogDebug("Removed {Count} old events from database", oldRecords.Count);
                        }

                        // Map TicketmasterEvent to Event domain objects
                        var domainEvents = allEvents.Select(te => new Event
                        {
                            ExternalId = te.Id,
                            Source = "Ticketmaster",
                            Title = te.Name,
                            StartTime = te.Dates?.Start?.DateTime ?? DateTime.UtcNow,
                            EndTime = te.Dates?.End?.DateTime,
                            Category = te.Classifications.FirstOrDefault()?.Segment?.Name ?? "Other",
                            CachedAt = DateTime.UtcNow
                        }).ToList();

                        // Add new records
                        dbContext.Events.AddRange(domainEvents);
                        await dbContext.SaveChangesAsync();
                        _logger.LogInformation("Saved {Count} events to database", domainEvents.Count);
                    }
                    else
                    {
                        _logger.LogWarning("No new events fetched; keeping existing events in database");
                    }
                }
            }
            catch (Exception dbEx)
            {
                _logger.LogError(dbEx, "Error saving events to database.");
            }
            
            return allEvents;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Ticketmaster poll was cancelled - loading fallback data from database");
            return await LoadFallbackEventsFromDbAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during Ticketmaster poll - loading fallback data from database");
            return await LoadFallbackEventsFromDbAsync();
        }
    }

    /// <summary>
    /// Load existing events from database as fallback when API fails.
    /// Returns events for next 24 hours (same window as live poll).
    /// </summary>
    private async Task<List<TicketmasterEvent>> LoadFallbackEventsFromDbAsync()
    {
        try
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var now = DateTime.UtcNow;
                
                var fallbackEvents = await dbContext.Events
                    .Where(e => e.Source == "Ticketmaster" && e.StartTime >= now && e.StartTime < now.AddHours(24))
                    .ToListAsync();
                
                _logger.LogInformation("Loaded {Count} fallback events from database (stale data due to API failure)", fallbackEvents.Count);
                
                // Convert domain Events back to TicketmasterEvent (lossy conversion, but preserves key data)
                return fallbackEvents.Select(e => new TicketmasterEvent
                {
                    Id = e.ExternalId,
                    Name = e.Title,
                    Dates = new TicketmasterDates
                    {
                        Start = new TicketmasterDateTime { DateTime = e.StartTime },
                        End = new TicketmasterDateTime { DateTime = e.EndTime }
                    },
                    Classifications = new List<TicketmasterClassification>
                    {
                        new TicketmasterClassification
                        {
                            Segment = new TicketmasterSegment { Name = e.Category }
                        }
                    }
                }).ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load fallback events from database");
            return new List<TicketmasterEvent>();
        }
    }

    /// <summary>
    /// Fetch a single page of events from Ticketmaster.
    /// </summary>
    private async Task<TicketmasterSearchResponse?> FetchPageAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var apiKey = _configuration["Ticketmaster:ApiKey"];
        var startDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        var endDateTime = DateTime.UtcNow.AddHours(EventLookAheadHours).ToString("yyyy-MM-ddTHH:mm:ssZ");
        
        // Build query string with Ticketmaster parameters
        var queryParams = new Dictionary<string, string>
        {
            { "apikey", apiKey ?? string.Empty },
            { "city", CityName },
            { "size", pageSize.ToString() },
            { "page", page.ToString() },
            { "startDateTime", startDateTime },
            { "endDateTime", endDateTime },
            { "includeTBA", "yes" },
            { "includeTBD", "yes" }
        };

        var queryString = string.Join("&", queryParams.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
        var requestUrl = $"events.json?{queryString}";

        try
        {
            var response = await _httpClient.GetAsync(requestUrl, cancellationToken);

            // Log rate limit headers
            LogRateLimitHeaders(response.Headers);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = false,
                PropertyNamingPolicy = null
            };

            var parsed = JsonSerializer.Deserialize<TicketmasterSearchResponse>(content, options);
            
            if (parsed == null)
            {
                _logger.LogError("Failed to deserialize Ticketmaster response to TicketmasterSearchResponse");
                return null;
            }

            _logger.LogDebug("Parsed response: Embedded={EmbeddedNull}, Events={EventCount}, Page={PageInfo}",
                parsed.Embedded == null ? "null" : "exists",
                parsed.Embedded?.Events?.Count ?? 0,
                parsed.Page == null ? "null" : $"page {parsed.Page.Number}/{parsed.Page.TotalPages}");

            return parsed;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogError("Ticketmaster authentication failed. Check API key in configuration.");
            throw;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            _logger.LogWarning("Ticketmaster rate limit exceeded (429). Backing off before retry.");
            await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Log rate limit information from response headers.
    /// </summary>
    private void LogRateLimitHeaders(HttpResponseHeaders headers)
    {
        try
        {
            var info = new RateLimitInfo();

            if (headers.TryGetValues("X-Rate-Limit", out var limitValues))
            {
                if (int.TryParse(limitValues.First(), out var limit))
                    info.Limit = limit;
            }

            if (headers.TryGetValues("X-Rate-Limit-Remaining", out var remainingValues))
            {
                if (int.TryParse(remainingValues.First(), out var remaining))
                    info.Remaining = remaining;
            }

            if (headers.TryGetValues("X-Rate-Limit-Reset", out var resetValues))
            {
                if (long.TryParse(resetValues.First(), out var resetUnix))
                    info.ResetSeconds = (int)(resetUnix - DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            }

            if (info.Limit > 0 || info.Remaining > 0)
            {
                _logger.LogDebug("Ticketmaster {RateLimitInfo}", info);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse rate limit headers");
        }
    }
}
