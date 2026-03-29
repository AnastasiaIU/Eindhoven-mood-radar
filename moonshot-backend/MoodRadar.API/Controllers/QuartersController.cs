namespace MoodRadar.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoodRadar.API.Data;
using MoodRadar.API.Models.Dtos.Responses;

/// <summary>
/// API endpoints for quarters (wijken).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class QuartersController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<QuartersController> _logger;

    public QuartersController(ApplicationDbContext dbContext, ILogger<QuartersController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/quarters
    /// Returns all quarters in Eindhoven, optionally filtered by district.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<QuarterResponseDto>>> GetAllQuarters([FromQuery] int? districtId = null)
    {
        _logger.LogInformation($"Fetching quarters (districtId={districtId})");
        
        try
        {
            var query = _dbContext.Quarters.AsNoTracking();
            
            if (districtId.HasValue)
            {
                query = query.Where(q => q.DistrictId == districtId.Value);
            }

            var quarters = await query
                .OrderBy(q => q.Name)
                .ToListAsync();

            var response = quarters.Select(q => new QuarterResponseDto
            {
                Id = q.Id,
                Name = q.Name,
                DistrictId = q.DistrictId,
                GeoJsonBoundary = q.GeoJsonBoundary,
                CreatedAt = q.CreatedAt
            }).ToList();

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching quarters");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to fetch quarters", details = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/quarters/{id}
    /// Returns a specific quarter with its neighborhoods.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuarterDetailResponseDto>> GetQuarter(int id)
    {
        _logger.LogInformation($"Fetching quarter {id}");
        
        try
        {
            var quarter = await _dbContext.Quarters
                .AsNoTracking()
                .Include(q => q.Neighborhoods)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quarter == null)
            {
                return NotFound(new { message = $"Quarter {id} not found" });
            }

            var response = new QuarterDetailResponseDto
            {
                Id = quarter.Id,
                Name = quarter.Name,
                DistrictId = quarter.DistrictId,
                GeoJsonBoundary = quarter.GeoJsonBoundary,
                CreatedAt = quarter.CreatedAt,
                Neighborhoods = quarter.Neighborhoods?.Select(n => new NeighborhoodResponseDto
                {
                    Id = n.Id,
                    Name = n.Name,
                    QuarterId = n.QuarterId,
                    QuarterName = quarter.Name,
                    GeoJsonBoundary = n.GeoJsonBoundary,
                    CreatedAt = n.CreatedAt
                }).OrderBy(n => n.Name).ToList() ?? new()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching quarter {id}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to fetch quarter", details = ex.Message });
        }
    }
}
