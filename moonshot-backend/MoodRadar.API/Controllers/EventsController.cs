namespace MoodRadar.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using MoodRadar.API.Models;
using MoodRadar.API.Services;

/// <summary>
/// API endpoints for event data from external sources.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IMockDataService _mockDataService;
    private readonly ILogger<EventsController> _logger;

    public EventsController(IMockDataService mockDataService, ILogger<EventsController> logger)
    {
        _mockDataService = mockDataService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/events
    /// Returns all active and upcoming events in Eindhoven.
    /// Sorted by start time (descending).
    /// </summary>
    /// <returns>List of events with details from multiple sources</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAllEvents()
    {
        _logger.LogInformation("Fetching all events");
        var events = _mockDataService.GetAllEvents();

        var response = events.Select(e => new EventResponse
        {
            Id = e.Id,
            Title = e.Title,
            Source = e.Source,
            StartTime = e.StartTime,
            EndTime = e.EndTime,
            Category = e.Category,
            Url = e.Url
        }).ToList();

        return Ok(response);
    }
}
