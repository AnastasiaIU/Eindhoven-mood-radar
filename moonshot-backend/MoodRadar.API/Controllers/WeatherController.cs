using Microsoft.AspNetCore.Mvc;
using MoodRadar.API.Services;
using MoodRadar.API.Models;

namespace MoodRadar.API.Controllers;

/// <summary>
/// Weather forecast endpoint providing hourly weather data for Eindhoven.
/// 
/// Data source: Open-Meteo API (free, no API key required).
/// Update frequency: Every 15 minutes via background service.
/// Data cached in memory for fast retrieval.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;
    private readonly ILogger<WeatherController> _logger;

    public WeatherController(IWeatherService weatherService, ILogger<WeatherController> logger)
    {
        _weatherService = weatherService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/weather
    /// Returns all cached hourly weather records for Eindhoven.
    /// 
    /// Response body: Array of Weather objects, sorted by SnapshotHour (ascending).
    /// </summary>
    [HttpGet(Name = "GetWeather")]
    public ActionResult<IEnumerable<Weather>> Get()
    {
        try
        {
            var cachedWeather = _weatherService.GetAllCachedWeather();
            _logger.LogInformation("Returning {Count} cached weather records", cachedWeather.Count);
            return Ok(cachedWeather);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached weather data");
            return StatusCode(500, new { error = "Error retrieving weather data" });
        }
    }

    /// <summary>
    /// GET /api/weather/hour?timestamp=2026-03-28T14:00:00Z
    /// Returns weather for a specific hour (rounded to nearest hour in cache).
    /// 
    /// Query parameter:
    ///   - timestamp (ISO 8601 format): Hour to retrieve, e.g. 2026-03-28T14:30:00Z
    ///                                   Will match to 2026-03-28T14:00:00Z
    /// 
    /// Response:
    ///   - 200 OK with Weather object if hour exists in cache
    ///   - 404 Not Found if hour is not in cache
    /// </summary>
    [HttpGet("hour")]
    public ActionResult<Weather> GetByHour([FromQuery] DateTime timestamp)
    {
        try
        {
            var weather = _weatherService.GetWeatherByHour(timestamp);

            if (weather == null)
            {
                _logger.LogInformation("No cached weather for timestamp: {Timestamp}", timestamp);
                return NotFound(new { error = "No weather data for requested hour" });
            }

            _logger.LogInformation("Retrieved weather for hour: {Hour}", weather.SnapshotHour);
            return Ok(weather);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving weather for timestamp: {Timestamp}", timestamp);
            return StatusCode(500, new { error = "Error retrieving weather data" });
        }
    }

    /// <summary>
    /// POST /api/weather/fetch
    /// Manually trigger a weather fetch from Open-Meteo API.
    /// Normally called by background service every 15 minutes.
    /// 
    /// Useful for testing or manual cache updates.
    /// 
    /// Response: Array of newly fetched Weather objects.
    /// </summary>
    [HttpPost("fetch")]
    public async Task<ActionResult<IEnumerable<Weather>>> FetchWeather(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Manual weather fetch triggered");
            var weather = await _weatherService.FetchEindhovenWeatherAsync(cancellationToken);
            _logger.LogInformation("Fetched {Count} weather records from Open-Meteo", weather.Count);
            return Ok(weather);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weather from Open-Meteo");
            return StatusCode(500, new { error = "Error fetching weather data from external API" });
        }
    }

    /// <summary>
    /// DELETE /api/weather/cache
    /// Clear old cached weather records (older than 1 days).
    /// 
    /// Query parameter (optional):
    ///   - olderThanDays (int): Age threshold in days. Default: 1
    /// 
    /// Response: { "cleared": 24 } showing count of removed records.
    /// </summary>
    [HttpDelete("cache")]
    public async Task<ActionResult<object>> ClearOldCache([FromQuery] int olderThanDays = 1)
    {
        try
        {
            await _weatherService.ClearOldCacheAsync(olderThanDays);
            var remainingCount = _weatherService.GetAllCachedWeather().Count;
            return Ok(new { message = $"Cache cleared (older than {olderThanDays} days). {remainingCount} records remain." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing old weather cache");
            return StatusCode(500, new { error = "Error clearing cache" });
        }
    }
}
