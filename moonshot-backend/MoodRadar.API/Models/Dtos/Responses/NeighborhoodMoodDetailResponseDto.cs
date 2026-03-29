namespace MoodRadar.API.Models.Dtos.Responses;

using System.Text.Json.Serialization;

/// <summary>
/// Response DTO for GET /api/neighborhoods/{id}/mood endpoint.
/// Contains detailed mood information including SHAP feature explanations.
/// </summary>
public class NeighborhoodMoodDetailResponseDto
{
    [JsonPropertyName("neighborhoodId")]
    public int NeighborhoodId { get; set; }

    [JsonPropertyName("neighborhoodName")]
    public string NeighborhoodName { get; set; } = string.Empty;

    [JsonPropertyName("moodLabel")]
    public string MoodLabel { get; set; } = string.Empty;

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// SHAP feature importances or ML model input features.
    /// Provides transparency into which factors drove this mood prediction.
    /// </summary>
    [JsonPropertyName("features")]
    public Dictionary<string, object>? Features { get; set; }

    [JsonPropertyName("lastUpdated")]
    public DateTime LastUpdated { get; set; }
}
