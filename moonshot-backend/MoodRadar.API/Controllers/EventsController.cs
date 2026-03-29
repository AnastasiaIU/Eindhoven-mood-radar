namespace MoodRadar.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoodRadar.API.Data;
using MoodRadar.API.Models.Dtos.Responses;
using MoodRadar.API.Services;

/// <summary>
/// API endpoints for event polling and serving.
/// 
/// Internal endpoints (for cron/background service):
/// - POST /api/events/refresh - trigger Ticketmaster poll
/// 
/// Frontend endpoints (serve from cache):
/// - GET /api/events - paginated event list, filterable by zone and category, 24h window
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ITicketmasterService _ticketmasterService;
    private readonly ILogger<EventsController> _logger;

    public EventsController(
        ApplicationDbContext dbContext,
        ITicketmasterService ticketmasterService,
        ILogger<EventsController> logger)
    {
        _dbContext = dbContext;
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
    /// GET /api/events
    /// Frontend endpoint: return paginated events from database cache (next 24 hours).
    /// Supports filtering by neighborhoodId and category.
    /// Query parameters: page (0-indexed), pageSize (1-50), neighborhoodId (optional), category (optional)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EventListResponseDto>> GetEvents(
        int page = 0,
        int pageSize = 20,
        int? neighborhoodId = null,
        string? category = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "GET /api/events - page={Page}, pageSize={PageSize}, neighborhoodId={NeighborhoodId}, category={Category}",
            page, pageSize, neighborhoodId, category);

        // Validate
        if (pageSize < 1 || pageSize > 50)
            return BadRequest(new { error = "pageSize must be between 1 and 50" });

        if (page < 0)
            return BadRequest(new { error = "page must be >= 0" });

        try
        {
            // Define 24-hour window: now to now + 24 hours UTC
            var now = DateTime.UtcNow;
            var in24Hours = now.AddHours(24);

            // Query events: next 24 hours, with optional filters
            var query = _dbContext.Events
                .AsNoTracking()
                .Where(e => e.StartTime >= now && e.StartTime <= in24Hours);

            // Apply neighborhood filter if provided
            if (neighborhoodId.HasValue)
            {
                query = query.Where(e => e.NeighborhoodId == neighborhoodId.Value);
            }

            // Apply category filter if provided
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(e => e.Category.ToLower() == category.ToLower());
            }

            // Count total for pagination
            var totalItems = await query.CountAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Fetch paginated results, sorted by start time
            var events = await query
                .OrderBy(e => e.StartTime)
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            // Map to DTOs
            var eventDtos = events.Select(e => new EventResponseDto
            {
                Id = e.Id,
                Title = e.Title,
                Source = e.Source,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                Category = e.Category,
                Url = e.Url,
                Latitude = e.Latitude,
                Longitude = e.Longitude,
                NeighborhoodId = e.NeighborhoodId
            }).ToList();

            var response = new EventListResponseDto
            {
                Data = eventDtos,
                Pagination = new PaginationMetaDto
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    TotalItems = totalItems
                }
            };

            _logger.LogInformation(
                "Returned {Count} events from page {Page}/{TotalPages}",
                events.Count, page, totalPages);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching paginated events");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to fetch events", details = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/events/{id}
    /// Frontend endpoint: return single event by ID from database.
    /// Returns 404 if not found.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponseDto>> GetEventById(int id)
    {
        _logger.LogInformation("GET /api/events/{Id} - Fetching event from database", id);

        try
        {
            var dbEvent = await _dbContext.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (dbEvent == null)
            {
                _logger.LogWarning("Event {EventId} not found", id);
                return NotFound(new { error = $"Event '{id}' not found" });
            }

            var response = new EventResponseDto
            {
                Id = dbEvent.Id,
                Title = dbEvent.Title,
                Source = dbEvent.Source,
                StartTime = dbEvent.StartTime,
                EndTime = dbEvent.EndTime,
                Category = dbEvent.Category,
                Url = dbEvent.Url,
                Latitude = dbEvent.Latitude,
                Longitude = dbEvent.Longitude,
                NeighborhoodId = dbEvent.NeighborhoodId
            };

            _logger.LogInformation("Retrieved event from database: {Title}", response.Title);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching event {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to fetch event", details = ex.Message });
        }
    }
}
