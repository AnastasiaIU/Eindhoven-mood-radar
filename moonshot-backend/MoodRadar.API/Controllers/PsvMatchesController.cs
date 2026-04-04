using Microsoft.AspNetCore.Mvc;
using MoodRadar.API.Services;
using MoodRadar.API.Models;

namespace MoodRadar.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PsvMatchesController : ControllerBase
    {
        private readonly FootballService _footballService;

        public PsvMatchesController(FootballService footballService)
        {
            _footballService = footballService;
        }

        /// <summary>
        /// Returns all upcoming or live PSV matches.
        /// </summary>
        /// <returns>List of PSV matches with date, home/away, kickoff time, status, and opponent</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<PsvMatch>), 200)]
        public async Task<ActionResult<List<PsvMatch>>> Get()
        {
            var matches = await _footballService.GetPsvMatchesAsync();
            if (matches.Count == 0)
            {
                return Ok(new List<PsvMatch>()); // Optional: return empty list
            }
            return Ok(matches);
        }
    }
}