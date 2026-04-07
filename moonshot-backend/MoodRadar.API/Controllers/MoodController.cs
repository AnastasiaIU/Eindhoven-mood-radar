using Microsoft.AspNetCore.Mvc;
using MoodRadar.API.Data;
using MoodRadar.API.Models.Domain;
using MoodRadar.API.Models.Dtos.Responses;
using Microsoft.EntityFrameworkCore;

namespace MoodRadar.API.Controllers;

/// <summary>
/// API endpoints for neighborhood mood snapshots.
/// Returns mood predictions calculated by the mood prediction service.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MoodController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<MoodController> _logger;

    public MoodController(ApplicationDbContext dbContext, ILogger<MoodController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/mood/neighborhood/{neighborhoodId}
    /// Returns upcoming hourly mood snapshots for the next 24 hours for a specific neighborhood.
    /// </summary>
    [HttpGet("neighborhood/{neighborhoodId}")]
    public async Task<ActionResult<NeighborhoodForecastResponseDto>> GetUpcomingMoodForNeighborhood(int neighborhoodId)
    {
        try
        {
            var neighborhood = await _dbContext.Neighborhoods
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == neighborhoodId);

            if (neighborhood == null)
            {
                return NotFound(new { error = "Neighborhood not found" });
            }

            var (forecastStartUtc, forecastEndUtcExclusive) = GetForecastWindowUtc();

            var snapshots = await _dbContext.NeighborhoodSnapshots
                .AsNoTracking()
                .Where(s => s.NeighborhoodId == neighborhoodId
                            && s.Timestamp >= forecastStartUtc
                            && s.Timestamp < forecastEndUtcExclusive)
                .OrderBy(s => s.Timestamp)
                .ThenByDescending(s => s.Id)
                .ToListAsync();

            // Keep only the latest row per hour when duplicates exist.
            var normalizedSnapshots = snapshots
                .GroupBy(s => s.Timestamp)
                .Select(g => g.OrderByDescending(x => x.Id).First())
                .OrderBy(s => s.Timestamp)
                .Select(ToSnapshotDto)
                .ToList();

            return Ok(new NeighborhoodForecastResponseDto
            {
                NeighborhoodId = neighborhood.Id,
                NeighborhoodName = neighborhood.Name,
                ForecastStartUtc = forecastStartUtc,
                ForecastEndUtcExclusive = forecastEndUtcExclusive,
                Snapshots = normalizedSnapshots
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving mood for neighborhood {NeighborhoodId}", neighborhoodId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// GET /api/mood/all
    /// Returns upcoming hourly mood snapshots for the next 24 hours for all neighborhoods.
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<object>> GetUpcomingMoodsForAllNeighborhoods()
    {
        try
        {
            var (forecastStartUtc, forecastEndUtcExclusive) = GetForecastWindowUtc();

            var neighborhoods = await _dbContext.Neighborhoods
                .AsNoTracking()
                .OrderBy(n => n.Name)
                .Select(n => new { n.Id, n.Name })
                .ToListAsync();

            var snapshots = await _dbContext.NeighborhoodSnapshots
                .AsNoTracking()
                .Where(s => s.Timestamp >= forecastStartUtc
                            && s.Timestamp < forecastEndUtcExclusive)
                .OrderBy(s => s.Timestamp)
                .ThenByDescending(s => s.Id)
                .ToListAsync();

            // Keep only the latest row per neighborhood+hour when duplicates exist.
            var snapshotLookup = snapshots
                .GroupBy(s => new { s.NeighborhoodId, s.Timestamp })
                .Select(g => g.OrderByDescending(x => x.Id).First())
                .GroupBy(s => s.NeighborhoodId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(x => x.Timestamp).Select(ToSnapshotDto).ToList());

            var results = neighborhoods
                .Select(n => new NeighborhoodForecastResponseDto
                {
                    NeighborhoodId = n.Id,
                    NeighborhoodName = n.Name,
                    ForecastStartUtc = forecastStartUtc,
                    ForecastEndUtcExclusive = forecastEndUtcExclusive,
                    Snapshots = snapshotLookup.TryGetValue(n.Id, out var neighborhoodSnapshots)
                        ? neighborhoodSnapshots
                        : new List<NeighborhoodSnapshotResponseDto>()
                })
                .ToList();

            return Ok(new
            {
                forecastStartUtc,
                forecastEndUtcExclusive,
                neighborhoods = results
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all neighborhood moods");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// GET /api/mood/neighborhood/{neighborhoodId}/snapshot?timestamp={ISO8601}
    /// Returns a single mood snapshot for a neighborhood at an exact UTC timestamp.
    /// </summary>
    [HttpGet("neighborhood/{neighborhoodId}/snapshot")]
    public async Task<ActionResult<NeighborhoodSnapshotResponseDto>> GetSnapshotForNeighborhoodAtTimestamp(
        int neighborhoodId,
        [FromQuery] DateTime timestamp)
    {
        try
        {
            if (timestamp == default)
            {
                return BadRequest(new { error = "Query parameter 'timestamp' is required (ISO-8601 format, UTC recommended)." });
            }

            var requestedTimestampUtc = NormalizeToUtc(timestamp);

            var neighborhoodExists = await _dbContext.Neighborhoods
                .AsNoTracking()
                .AnyAsync(n => n.Id == neighborhoodId);

            if (!neighborhoodExists)
            {
                return NotFound(new { error = "Neighborhood not found" });
            }

            var snapshot = await _dbContext.NeighborhoodSnapshots
                .AsNoTracking()
                .Where(s => s.NeighborhoodId == neighborhoodId)
                .Where(s => s.Timestamp == requestedTimestampUtc)
                .OrderByDescending(s => s.Id)
                .FirstOrDefaultAsync();

            if (snapshot == null)
            {
                return NotFound(new
                {
                    error = "No mood snapshot found for this neighborhood at the requested timestamp",
                    neighborhoodId,
                    timestamp = requestedTimestampUtc
                });
            }

            return Ok(ToSnapshotDto(snapshot));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving timestamp snapshot for neighborhood {NeighborhoodId}", neighborhoodId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    private static (DateTime ForecastStartUtc, DateTime ForecastEndUtcExclusive) GetForecastWindowUtc()
    {
        var utcNow = DateTime.UtcNow;
        var nextHourUtc = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, 0, 0, DateTimeKind.Utc)
            .AddHours(1);

        return (nextHourUtc, nextHourUtc.AddHours(24));
    }

    private static NeighborhoodSnapshotResponseDto ToSnapshotDto(NeighborhoodSnapshot snapshot)
    {
        return new NeighborhoodSnapshotResponseDto
        {
            Timestamp = snapshot.Timestamp,
            MoodLabel = snapshot.MoodLabel,
            Confidence = snapshot.Confidence,
            Features = snapshot.FeatureJson
        };
    }

    private static DateTime NormalizeToUtc(DateTime timestamp)
    {
        return timestamp.Kind switch
        {
            DateTimeKind.Utc => timestamp,
            DateTimeKind.Local => timestamp.ToUniversalTime(),
            _ => DateTime.SpecifyKind(timestamp, DateTimeKind.Utc)
        };
    }
}
