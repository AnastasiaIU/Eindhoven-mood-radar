namespace MoodRadar.API.Models.Dtos.Responses;

using System.Text.Json.Serialization;

/// <summary>
/// Response DTO for GET /api/quarters endpoint.
/// </summary>
public class QuarterResponseDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("districtId")]
    public int DistrictId { get; set; }

    [JsonPropertyName("geoJsonBoundary")]
    public string GeoJsonBoundary { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Response DTO for GET /api/quarters/{id} endpoint.
/// Includes neighborhoods list.
/// </summary>
public class QuarterDetailResponseDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("districtId")]
    public int DistrictId { get; set; }

    [JsonPropertyName("geoJsonBoundary")]
    public string GeoJsonBoundary { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("neighborhoods")]
    public List<NeighborhoodResponseDto> Neighborhoods { get; set; } = new();
}
