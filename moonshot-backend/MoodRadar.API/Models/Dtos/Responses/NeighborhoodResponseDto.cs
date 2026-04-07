namespace MoodRadar.API.Models.Dtos.Responses;

using System.Text.Json.Serialization;

/// <summary>
/// Response DTO for GET /api/neighborhoods endpoint.
/// Returns neighborhood metadata only.
/// </summary>
public class NeighborhoodResponseDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("quarterId")]
    public int QuarterId { get; set; }

    [JsonPropertyName("quarterName")]
    public string QuarterName { get; set; } = string.Empty;

    [JsonPropertyName("geoJsonBoundary")]
    public string GeoJsonBoundary { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}
