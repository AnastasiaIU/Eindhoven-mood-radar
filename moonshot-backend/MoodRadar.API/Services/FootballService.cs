using System.Globalization;
using Newtonsoft.Json.Linq;
using MoodRadar.API.Models;

namespace MoodRadar.API.Services
{
    public class FootballService
    {
        private readonly HttpClient _client;
        private readonly ILogger<FootballService> _logger;
        private readonly string _apiKey;

        public FootballService(IHttpClientFactory factory, ILogger<FootballService> logger, IConfiguration config)
        {
            _client = factory.CreateClient("football");
            _logger = logger;
            _apiKey = config["FootballApi:ApiKey"];
            _client.DefaultRequestHeaders.Add("X-Auth-Token", _apiKey);
        }

        public async Task<List<PsvMatch>> GetPsvMatchesAsync()
        {
            try
            {
                var response = await _client.GetAsync("matches?teams=674");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Football API returned {StatusCode}: {ReasonPhrase}", 
                        response.StatusCode, response.ReasonPhrase);
                    return new List<PsvMatch>();
                }

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
                        "yyyy-MM-ddTHH:mm:ssZ",  // normal API format
                        "MM/dd/yyyy HH:mm:ss",   // US format (your error)
                        "dd/MM/yyyy HH:mm:ss"    // EU fallback
                    };

                    if (!DateTime.TryParseExact(
                            rawDate,
                            formats,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal,
                            out parsedDate))
                    {
                        _logger.LogWarning("Failed to parse date: {Date}", rawDate);
                        continue; // skip broken data instead of crashing
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching PSV matches from football API");
                return new List<PsvMatch>();
            }
        }
    }
}