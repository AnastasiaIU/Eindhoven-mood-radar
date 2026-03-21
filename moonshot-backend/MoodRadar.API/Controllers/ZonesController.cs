namespace MoodRadar.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using MoodRadar.API.Services;

/// <summary>
/// API endpoints for zone mood data and predictions.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ZonesController : ControllerBase
{
    private readonly IMockDataService _mockDataService;
    private readonly ILogger<ZonesController> _logger;

    public ZonesController(IMockDataService mockDataService, ILogger<ZonesController> logger)
    {
        _mockDataService = mockDataService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/zones
    /// Returns all zones in Eindhoven.
    /// </summary>
    /// <returns>List of all zones with their basic information</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAllZones()
    {
        _logger.LogInformation("Fetching all zones");
        var zones = _mockDataService.GetAllZones();
        return Ok(zones);
    }

    /// <summary>
    /// GET /api/zones/{id}/mood
    /// Returns the current mood prediction for a specific zone.
    /// </summary>
    /// <param name="id">Zone ID</param>
    /// <returns>Zone with current mood label and confidence score</returns>
    [HttpGet("{id}/mood")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetZoneMood(int id)
    {
        _logger.LogInformation($"Fetching mood for zone {id}");
        var zoneMood = _mockDataService.GetZoneMood(id);

        if (zoneMood == null)
        {
            _logger.LogWarning($"Zone {id} not found");
            return NotFound(new { message = $"Zone {id} not found" });
        }

        return Ok(zoneMood);
    }
}
