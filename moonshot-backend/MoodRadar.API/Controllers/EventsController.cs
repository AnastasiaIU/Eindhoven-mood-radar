namespace MoodRadar.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using MoodRadar.API.Models;
using MoodRadar.API.Services;

/// <summary>
/// API endpoints for event polling and serving.
/// 
/// Internal endpoints (for cron/background service):
/// - POST /api/events/refresh - trigger Ticketmaster poll
/// 
/// Frontend endpoints (serve from cache):
/// - GET /api/events?page=0&pageSize=20 - paginated event list
/// - GET /api/events/{id} - single event by ID
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly ITicketmasterService _ticketmasterService;
    private readonly ILogger<EventsController> _logger;

    public EventsController(ITicketmasterService ticketmasterService, ILogger<EventsController> logger)
    {
        _ticketmasterService = ticketmasterService;
        _logger = logger;
    }

    /// <summary>
    /// POST /api/events/refresh
    /// Internal endpoint: fetch events from Ticketmaster and update cache.
    /// Called by: cron job / background service every 15 minutes (Phase 2).
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> RefreshEvents(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("POST /api/events/refresh - Polling Ticketmaster for fresh events");
        
        try
        {
            var events = await _ticketmasterService.PollEindhovenEventsAsync(cancellationToken: cancellationToken);
            
            _logger.LogInformation("Poll completed. Cached {Count} events", events.Count);
            return Ok(new
            {
                message = "Ticketmaster poll completed",
                cachedCount = events.Count,
                timestamp = DateTime.UtcNow
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during Ticketmaster poll");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "Failed to poll Ticketmaster", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during poll");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "Unexpected error", details = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/events/ticketmaster
    /// Frontend endpoint: return paginated events from cache.
    /// Page is 0-indexed. Default: page=0, pageSize=20.
    /// </summary>
    [HttpGet("ticketmaster")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<object> GetEvents(int page = 0, int pageSize = 20)
    {
        _logger.LogInformation("GET /api/events/ticketmaster - page={Page}, pageSize={PageSize}", page, pageSize);
        
        // Validate
        if (pageSize < 1 || pageSize > 50)
            return BadRequest(new { error = "pageSize must be between 1 and 50" });
        
        if (page < 0)
            return BadRequest(new { error = "page must be >= 0" });

        try
        {
            var allEvents = _ticketmasterService.GetCachedEvents();
            
            // Paginate
            var totalPages = (int)Math.Ceiling((double)allEvents.Count / pageSize);
            var paginatedEvents = allEvents
                .Skip(page * pageSize)
                .Take(pageSize)
                .OrderBy(e => e.Dates?.Start?.DateTime ?? DateTime.UtcNow)
                .ToList();

            var response = paginatedEvents.Select(e => new EventResponse
            {
                Id = int.TryParse(e.Id, out var id) ? id : 0,
                Title = e.Name,
                Source = "Ticketmaster",
                StartTime = e.Dates?.Start?.DateTime ?? DateTime.UtcNow,
                EndTime = e.Dates?.End?.DateTime,
                Category = e.Classifications?.FirstOrDefault()?.Segment?.Name ?? "Other",
                Url = e.Url,
                Latitude = ParseCoordinate(e.Embedded?.Venues?.FirstOrDefault()?.Location?.Latitude),
                Longitude = ParseCoordinate(e.Embedded?.Venues?.FirstOrDefault()?.Location?.Longitude)
            })
            .ToList();

            _logger.LogInformation("Returned {Count} events from page {Page}/{TotalPages}", response.Count, page, totalPages);
            
            return Ok(new
            {
                data = response,
                pagination = new
                {
                    page,
                    pageSize,
                    totalPages,
                    totalItems = allEvents.Count
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching paginated events");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "Unexpected error", details = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/events/ticketmaster/{id}
    /// Frontend endpoint: return single event by ID from cache.
    /// Returns 404 if not found.
    /// </summary>
    [HttpGet("ticketmaster/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<EventResponse> GetEventById(string id)
    {
        _logger.LogInformation("GET /api/events/ticketmaster/{Id} - Fetching event from cache", id);
        
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new { error = "Event ID is required" });

        try
        {
            var ticketmasterEvent = _ticketmasterService.GetCachedEventById(id);
            
            if (ticketmasterEvent == null)
            {
                _logger.LogWarning("Event {EventId} not found in cache", id);
                return NotFound(new { error = $"Event '{id}' not found" });
            }

            var response = new EventResponse
            {
                Id = int.TryParse(ticketmasterEvent.Id, out var eventId) ? eventId : 0,
                Title = ticketmasterEvent.Name,
                Source = "Ticketmaster",
                StartTime = ticketmasterEvent.Dates?.Start?.DateTime ?? DateTime.UtcNow,
                EndTime = ticketmasterEvent.Dates?.End?.DateTime,
                Category = ticketmasterEvent.Classifications?.FirstOrDefault()?.Segment?.Name ?? "Other",
                Url = ticketmasterEvent.Url,
                Latitude = ParseCoordinate(ticketmasterEvent.Embedded?.Venues?.FirstOrDefault()?.Location?.Latitude),
                Longitude = ParseCoordinate(ticketmasterEvent.Embedded?.Venues?.FirstOrDefault()?.Location?.Longitude)
            };

            _logger.LogInformation("Retrieved event from cache: {Title}", response.Title);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching event {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "Unexpected error", details = ex.Message });
        }
    }

    /// <summary>
    /// Helper method to convert coordinate strings from Ticketmaster API to double.
    /// Ticketmaster returns coordinates as strings (e.g., "51.4416"), not JSON numbers.
    /// </summary>
    private double? ParseCoordinate(string? coordinateString)
    {
        if (string.IsNullOrWhiteSpace(coordinateString))
            return null;

        if (double.TryParse(coordinateString, System.Globalization.NumberStyles.Any, 
            System.Globalization.CultureInfo.InvariantCulture, out var result))
            return result;

        return null;
    }
}
