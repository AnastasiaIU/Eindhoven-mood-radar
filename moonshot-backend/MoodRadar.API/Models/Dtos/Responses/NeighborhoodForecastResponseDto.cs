namespace MoodRadar.API.Models.Dtos.Responses;

using System.Text.Json.Serialization;

/// <summary>
/// Upcoming mood forecast for a neighborhood.
/// Contains hourly snapshots for the next 24 hours.
/// </summary>
public class NeighborhoodForecastResponseDto
{
    [JsonPropertyName("neighborhoodId")]
    public int NeighborhoodId { get; set; }

    [JsonPropertyName("neighborhoodName")]
    public string NeighborhoodName { get; set; } = string.Empty;

    [JsonPropertyName("forecastStartUtc")]
    public DateTime ForecastStartUtc { get; set; }

    [JsonPropertyName("forecastEndUtcExclusive")]
    public DateTime ForecastEndUtcExclusive { get; set; }

    [JsonPropertyName("snapshots")]
    public List<NeighborhoodSnapshotResponseDto> Snapshots { get; set; } = new();
}
