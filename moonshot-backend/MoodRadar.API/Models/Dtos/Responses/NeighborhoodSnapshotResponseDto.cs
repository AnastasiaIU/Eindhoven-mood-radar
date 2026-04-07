namespace MoodRadar.API.Models.Dtos.Responses;

using System.Text.Json.Serialization;

/// <summary>
/// Single mood snapshot for a neighborhood at a specific timestamp.
/// </summary>
public class NeighborhoodSnapshotResponseDto
{
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("moodLabel")]
    public string MoodLabel { get; set; } = string.Empty;

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }

    [JsonPropertyName("features")]
    public Dictionary<string, object>? Features { get; set; }
}
