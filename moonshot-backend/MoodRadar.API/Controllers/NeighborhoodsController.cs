namespace MoodRadar.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoodRadar.API.Data;
using MoodRadar.API.Models.Domain;
using MoodRadar.API.Models.Dtos.Responses;

/// <summary>
/// API endpoints for neighborhoods (buurten).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class NeighborhoodsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<NeighborhoodsController> _logger;

    public NeighborhoodsController(ApplicationDbContext dbContext, ILogger<NeighborhoodsController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/neighborhoods
    /// Returns all neighborhoods in Eindhoven with their current moods, optionally filtered by quarter or district.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<NeighborhoodResponseDto>>> GetAllNeighborhoods(
        [FromQuery] int? quarterId = null,
        [FromQuery] int? districtId = null)
    {
        _logger.LogInformation($"Fetching neighborhoods (quarterId={quarterId}, districtId={districtId})");
        
        try
        {
            IQueryable<Neighborhood> query = _dbContext.Neighborhoods.AsNoTracking().Include(n => n.Quarter);

            if (quarterId.HasValue)
            {
                query = query.Where(n => n.QuarterId == quarterId.Value);
            }

            if (districtId.HasValue)
            {
                query = query.Where(n => n.Quarter!.DistrictId == districtId.Value);
            }

            var neighborhoods = await query
                .OrderBy(n => n.Name)
                .ToListAsync();

            var response = new List<NeighborhoodResponseDto>();

            foreach (var neighborhood in neighborhoods)
            {
                var latestSnapshot = await _dbContext.NeighborhoodSnapshots
                    .AsNoTracking()
                    .Where(s => s.NeighborhoodId == neighborhood.Id)
                    .OrderByDescending(s => s.Timestamp)
                    .FirstOrDefaultAsync();

                response.Add(new NeighborhoodResponseDto
                {
                    Id = neighborhood.Id,
                    Name = neighborhood.Name,
                    QuarterId = neighborhood.QuarterId,
                    QuarterName = neighborhood.Quarter?.Name ?? "Unknown",
                    GeoJsonBoundary = neighborhood.GeoJsonBoundary,
                    CreatedAt = neighborhood.CreatedAt,
                    CurrentMood = latestSnapshot?.MoodLabel,
                    Confidence = latestSnapshot?.Confidence,
                    LastMoodUpdate = latestSnapshot?.Timestamp
                });
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching neighborhoods");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to fetch neighborhoods", details = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/neighborhoods/{id}
    /// Returns a specific neighborhood with its current mood.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NeighborhoodMoodDetailResponseDto>> GetNeighborhoodMood(int id)
    {
        _logger.LogInformation($"Fetching mood for neighborhood {id}");
        
        try
        {
            var neighborhood = await _dbContext.Neighborhoods
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == id);

            if (neighborhood == null)
            {
                _logger.LogWarning($"Neighborhood {id} not found");
                return NotFound(new { message = $"Neighborhood {id} not found" });
            }

            var snapshot = await _dbContext.NeighborhoodSnapshots
                .AsNoTracking()
                .Where(s => s.NeighborhoodId == id)
                .OrderByDescending(s => s.Timestamp)
                .FirstOrDefaultAsync();

            if (snapshot == null)
            {
                _logger.LogWarning($"No mood snapshot found for neighborhood {id}");
                return NotFound(new { message = $"No mood data available for neighborhood {id}" });
            }

            return Ok(new NeighborhoodMoodDetailResponseDto
            {
                NeighborhoodId = neighborhood.Id,
                NeighborhoodName = neighborhood.Name,
                MoodLabel = snapshot.MoodLabel,
                Confidence = snapshot.Confidence,
                Timestamp = snapshot.Timestamp,
                Features = snapshot.FeatureJson,
                LastUpdated = snapshot.Timestamp
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching mood for neighborhood {id}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to fetch neighborhood mood", details = ex.Message });
        }
    }
}
