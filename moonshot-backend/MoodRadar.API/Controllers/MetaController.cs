namespace MoodRadar.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using MoodRadar.API.Models.Dtos.Responses;

/// <summary>
/// API endpoints for model metadata and transparency information.
/// Used by the Transparency Panel to explain mood calculations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MetaController : ControllerBase
{
    private readonly ILogger<MetaController> _logger;

    public MetaController(ILogger<MetaController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// GET /api/meta
    /// Returns model metadata, mood descriptions, data sources, and known limitations.
    /// Used by the Transparency Panel to explain:
    /// - How moods are calculated
    /// - What data sources feed the model
    /// - Known biases and limitations
    /// - Confidence score meaning
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<MetadataResponseDto> GetMetadata()
    {
        _logger.LogInformation("GET /api/meta - Returning model metadata");

        try
        {
            var response = new MetadataResponseDto
            {
                ModelDescription = "Eindhoven Mood Radar uses a trained ML model to predict the 'mood' of each city zone " +
                    "based on real-time event data, weather, and PSV match status. Moods reflect the overall vibe and " +
                    "energy level of a zone at a specific time.",

                MoodLabels = new Dictionary<string, string>
                {
                    { "Energetic", "High activity, many events, warm weather. Typical of nightlife or weekend atmosphere." },
                    { "Intense", "Peak energy: major concert, PSV home match, or similar high-draw event creating crowds." },
                    { "Busy", "Moderate-high activity during daytime; markets, fairs, or multiple concurrent events." },
                    { "Relaxed", "Lower activity; pleasant weather and few events create a calm, leisure atmosphere." },
                    { "Calm", "Very low activity; early morning, late night, or quiet residential zones." }
                },

                DataSourcesUsed = new List<string>
                {
                    "Ticketmaster Discovery API (event listings)",
                    "football-data.org (PSV Eindhoven match schedule and status)",
                    "Open-Meteo (weather: temperature, precipitation, cloud cover)",
                    "System clock (time of day, day of week, public holiday flags)"
                },

                KnownLimitations = "Event data sourced from Ticketmaster Discovery API; coverage is sparse for tier-2 Dutch cities like " +
                    "Eindhoven (~2 events per 10 days window). Commercial venue partnerships dominate over local independent venues. " +
                    "Central-Eindhoven (Centrum, Strijp) zones are over-represented; quieter residential zones (Woensel-Noord, Stratum) " +
                    "may default to 'Calm' due to event listing bias. This bias is structural to the API, not a model flaw.",

                ConfidenceScoreExplanation = "Confidence (0–1) reflects model certainty in the mood prediction. " +
                    "Higher = more confident. Affected by feature variance, data recency, and signal strength. " +
                    "Low confidence (<0.6) suggests ambiguous zone state; high confidence (>0.85) indicates strong signals.",

                FeatureExplanations = new Dictionary<string, string>
                {
                    { "active_events", "Number of events occurring in this zone within the current time window." },
                    { "temperature", "Ambient temperature (°C) from weather data; warmer weather may increase outdoor activity." },
                    { "precipitation_probability", "Probability (0–1) of rain in the next hours; affects outdoor event turnout." },
                    { "is_psv_match_day", "Binary flag: true if PSV Eindhoven has a home match today; strongly drives Strijp/Centrum moods." },
                    { "is_holiday", "Binary flag: true if today is a Dutch public holiday; affects typical activity patterns." }
                }
            };

            _logger.LogInformation("Returned model metadata successfully");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving model metadata");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve metadata", details = ex.Message });
        }
    }
}
