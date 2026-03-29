namespace MoodRadar.API.Models.Dtos.Responses;

using System.Text.Json.Serialization;

/// <summary>
/// Response DTO for GET /api/districts endpoint.
/// </summary>
public class DistrictResponseDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("geoJsonBoundary")]
    public string GeoJsonBoundary { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Response DTO for GET /api/districts/{id} endpoint.
/// Includes quarters list.
/// </summary>
public class DistrictDetailResponseDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("geoJsonBoundary")]
    public string GeoJsonBoundary { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("quarters")]
    public List<QuarterResponseDto> Quarters { get; set; } = new();
}
