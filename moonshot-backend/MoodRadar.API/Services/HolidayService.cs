using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Linq;
using MoodRadar.API.Models;

namespace MoodRadar.API.Services
{
    public class HolidayService
    {
        private readonly HttpClient _client;
        private List<Holiday> _cachedHolidays;

        public HolidayService(IHttpClientFactory factory)
        {
            _client = factory.CreateClient();
        }

        public async Task<List<Holiday>> GetDutchHolidays2026Async()
        {
            if (_cachedHolidays != null) return _cachedHolidays;

            var url = "https://date.nager.at/api/v3/PublicHolidays/2026/NL";
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            _cachedHolidays = JsonConvert.DeserializeObject<List<Holiday>>(json);

            return _cachedHolidays;
        }

        public async Task<bool> IsHolidayAsync(DateTime date)
        {
            var holidays = await GetDutchHolidays2026Async();
            return holidays.Any(h => h.Date.Date == date.Date);
        }
    }
}