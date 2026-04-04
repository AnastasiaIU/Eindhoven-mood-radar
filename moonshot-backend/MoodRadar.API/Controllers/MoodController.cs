using Microsoft.AspNetCore.Mvc;
using MoodRadar.API.Data;
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
    /// Returns the latest mood prediction for a specific neighborhood.
    /// </summary>
    [HttpGet("neighborhood/{neighborhoodId}")]
    public async Task<ActionResult<object>> GetLatestMoodForNeighborhood(int neighborhoodId)
    {
        try
        {
            var snapshot = await _dbContext.NeighborhoodSnapshots
                .Where(s => s.NeighborhoodId == neighborhoodId)
                .OrderByDescending(s => s.Timestamp)
                .FirstOrDefaultAsync();

            if (snapshot == null)
            {
                return NotFound(new { error = "No mood prediction found for this neighborhood" });
            }

            return Ok(new
            {
                neighborhoodId = snapshot.NeighborhoodId,
                moodLabel = snapshot.MoodLabel,
                confidence = snapshot.Confidence,
                timestamp = snapshot.Timestamp,
                features = snapshot.FeatureJson
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
    /// Returns the latest mood predictions for all neighborhoods.
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<object>>> GetLatestMoodsForAllNeighborhoods()
    {
        try
        {
            var neighborhoods = await _dbContext.Neighborhoods.ToListAsync();
            var results = new List<object>();

            foreach (var neighborhood in neighborhoods)
            {
                var latestSnapshot = await _dbContext.NeighborhoodSnapshots
                    .Where(s => s.NeighborhoodId == neighborhood.Id)
                    .OrderByDescending(s => s.Timestamp)
                    .FirstOrDefaultAsync();

                results.Add(new
                {
                    neighborhoodId = neighborhood.Id,
                    neighborhoodName = neighborhood.Name,
                    moodLabel = latestSnapshot?.MoodLabel ?? "Unknown",
                    confidence = latestSnapshot?.Confidence ?? 0.0,
                    timestamp = latestSnapshot?.Timestamp ?? DateTime.MinValue,
                    features = latestSnapshot?.FeatureJson
                });
            }

            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all neighborhood moods");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// GET /api/mood/neighborhood/{neighborhoodId}/history
    /// Returns mood prediction history for a neighborhood (last 24 hours, paginated).
    /// Query parameters: limit (0-100), offset (0-based)
    /// </summary>
    [HttpGet("neighborhood/{neighborhoodId}/history")]
    public async Task<ActionResult<IEnumerable<object>>> GetMoodHistory(int neighborhoodId, int limit = 10, int offset = 0)
    {
        try
        {
            if (limit < 1 || limit > 100)
                return BadRequest(new { error = "limit must be between 1 and 100" });

            if (offset < 0)
                return BadRequest(new { error = "offset must be >= 0" });

            var snapshots = await _dbContext.NeighborhoodSnapshots
                .Where(s => s.NeighborhoodId == neighborhoodId)
                .OrderByDescending(s => s.Timestamp)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            if (!snapshots.Any())
            {
                return NotFound(new { error = "No mood history found for this neighborhood" });
            }

            var results = snapshots.Select(s => new
            {
                timestamp = s.Timestamp,
                moodLabel = s.MoodLabel,
                confidence = s.Confidence,
                features = s.FeatureJson
            });

            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving mood history for neighborhood {NeighborhoodId}", neighborhoodId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }
}
