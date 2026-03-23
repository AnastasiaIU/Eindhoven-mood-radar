using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using MoodRadar.API.Models;

namespace MoodRadar.API.Services
{
    public class FootballService
    {
        private readonly HttpClient _client;
        private readonly ILogger<FootballService> _logger;
        private readonly string _apiKey;

        public FootballService(
            IHttpClientFactory factory,
            ILogger<FootballService> logger,
            IConfiguration config)
        {
            _client = factory.CreateClient("football");
            _logger = logger;

            _apiKey = config["FootballApi:ApiKey"];

            if (!_client.DefaultRequestHeaders.Contains("X-Auth-Token"))
            {
                _client.DefaultRequestHeaders.Add("X-Auth-Token", _apiKey);
            }
        }

        public async Task<List<PsvMatch>> GetPsvMatchesAsync()
        {
            var response = await _client.GetAsync("matches?teams=674");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var data = JObject.Parse(json);

            var matches = new List<PsvMatch>();

            foreach (var match in data["matches"])
            {
                try
                {
                    bool isHome = (int)match["homeTeam"]["id"] == 674;

                    string opponent = isHome
                        ? (string)match["awayTeam"]["name"]
                        : (string)match["homeTeam"]["name"];

                    var rawDate = (string)match["utcDate"];

                    DateTime parsedDate;

                    var formats = new[]
                    {
                        "yyyy-MM-ddTHH:mm:ssZ",
                        "MM/dd/yyyy HH:mm:ss",
                        "dd/MM/yyyy HH:mm:ss"
                    };

                    if (!DateTime.TryParseExact(
                            rawDate,
                            formats,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal,
                            out parsedDate))
                    {
                        _logger.LogWarning("Failed to parse date: {Date}", rawDate);
                        continue;
                    }

                    matches.Add(new PsvMatch
                    {
                        MatchDate = parsedDate,
                        KickOffTime = parsedDate,
                        HomeAway = isHome ? "HOME" : "AWAY",
                        Status = (string)match["status"],
                        Opponent = opponent
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing match");
                }
            }

            return matches;
        }
    }
}