using Microsoft.AspNetCore.Mvc;
using MoodRadar.API.Services;
using System;
using System.Threading.Tasks;

namespace MoodRadar.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HolidaysController : ControllerBase
    {
        private readonly HolidayService _holidayService;

        public HolidaysController(HolidayService holidayService)
        {
            _holidayService = holidayService;
        }

        /// <summary>
        /// Returns all Dutch public holidays for 2026.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetHolidays()
        {
            var holidays = await _holidayService.GetDutchHolidays2026Async();
            return Ok(holidays);
        }
    }
}