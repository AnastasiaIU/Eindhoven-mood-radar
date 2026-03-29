namespace MoodRadar.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoodRadar.API.Data;
using MoodRadar.API.Models.Dtos.Responses;

/// <summary>
/// API endpoints for geographic data: districts, quarters, and neighborhoods.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DistrictsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DistrictsController> _logger;

    public DistrictsController(ApplicationDbContext dbContext, ILogger<DistrictsController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/districts
    /// Returns all districts in Eindhoven.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DistrictResponseDto>>> GetAllDistricts()
    {
        _logger.LogInformation("Fetching all districts");
        
        try
        {
            var districts = await _dbContext.Districts
                .AsNoTracking()
                .OrderBy(d => d.Name)
                .ToListAsync();

            var response = districts.Select(d => new DistrictResponseDto
            {
                Id = d.Id,
                Name = d.Name,
                GeoJsonBoundary = d.GeoJsonBoundary,
                CreatedAt = d.CreatedAt
            }).ToList();

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching districts");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to fetch districts", details = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/districts/{id}
    /// Returns a specific district with its quarters.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DistrictDetailResponseDto>> GetDistrict(int id)
    {
        _logger.LogInformation($"Fetching district {id}");
        
        try
        {
            var district = await _dbContext.Districts
                .AsNoTracking()
                .Include(d => d.Quarters)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (district == null)
            {
                return NotFound(new { message = $"District {id} not found" });
            }

            var response = new DistrictDetailResponseDto
            {
                Id = district.Id,
                Name = district.Name,
                GeoJsonBoundary = district.GeoJsonBoundary,
                CreatedAt = district.CreatedAt,
                Quarters = district.Quarters?.Select(q => new QuarterResponseDto
                {
                    Id = q.Id,
                    Name = q.Name,
                    DistrictId = q.DistrictId,
                    GeoJsonBoundary = q.GeoJsonBoundary,
                    CreatedAt = q.CreatedAt
                }).OrderBy(q => q.Name).ToList() ?? new()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching district {id}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to fetch district", details = ex.Message });
        }
    }
}
