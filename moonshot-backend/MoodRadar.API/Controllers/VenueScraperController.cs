namespace MoodRadar.API.Controllers;

using MoodRadar.API.Services;
using Microsoft.AspNetCore.Mvc;
using MoodRadar.API.Utilities;

/// <summary>
/// API endpoint for manual venue scraping (Uit in Eindhoven agenda).
/// 
/// POST /api/scraper/venues - Trigger a manual scraping run
/// 
/// PATTERN: Like EventsController.RefreshEvents, this endpoint is provided for manual testing
/// and monitoring. The background job (MoodUpdateService) calls IVenueScraperService 
/// directly as an injected service, not via this endpoint.
/// </summary>
[ApiController]
[Route("api/scraper")]
public class VenueScraperController : ControllerBase
{
    private readonly IVenueScraperService _scraperService;
    private readonly ILogger<VenueScraperController> _logger;

    public VenueScraperController(
        IVenueScraperService scraperService,
        ILogger<VenueScraperController> logger)
    {
        _scraperService = scraperService;
        _logger = logger;
    }

    /// <summary>
    /// Manually trigger venue scraping from Uit in Eindhoven.
    /// 
    /// Returns:
    /// - 200 OK: Scraping completed successfully
    /// - 500 Internal Server Error: Scraping failed
    /// 
    /// This endpoint is provided for manual testing and monitoring.
    /// The background job (MoodUpdateService) injects IVenueScraperService 
    /// and calls it directly (not via HTTP), following the pattern used by 
    /// other services (Ticketmaster, Football, Weather).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the request</param>
    /// <returns>Result of scraping operation</returns>
    [HttpPost("venues")]
    [NonProductionOnly]
    public async Task<IActionResult> ScrapeVenues(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Manual scraping request received for Uit in Eindhoven");
            
            var events = await _scraperService.ScrapeAllVenuesAsync(cancellationToken);
            
            return Ok(new
            {
                success = true,
                message = $"Scraping completed successfully",
                eventsCount = events.Count,
                timestamp = DateTime.UtcNow
            });
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Scraping request was cancelled");
            return BadRequest(new
            {
                success = false,
                message = "Scraping request was cancelled",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scraping failed with error: {Message}", ex.Message);
            return StatusCode(500, new
            {
                success = false,
                message = $"Scraping failed: {ex.Message}",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
