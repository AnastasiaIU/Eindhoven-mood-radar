namespace MoodRadar.API.Models.Dtos.Responses;

using System.Text.Json.Serialization;

/// <summary>
/// Response DTO for GET /api/meta endpoint.
/// Contains model metadata, mood descriptions, and feature explanations for the Transparency Panel.
/// </summary>
public class MetadataResponseDto
{
    [JsonPropertyName("modelDescription")]
    public string ModelDescription { get; set; } = string.Empty;

    [JsonPropertyName("moodLabels")]
    public Dictionary<string, string> MoodLabels { get; set; } = new();

    [JsonPropertyName("dataSourcesUsed")]
    public List<string> DataSourcesUsed { get; set; } = new();

    [JsonPropertyName("knownLimitations")]
    public string KnownLimitations { get; set; } = string.Empty;

    [JsonPropertyName("confidenceScoreExplanation")]
    public string ConfidenceScoreExplanation { get; set; } = string.Empty;

    [JsonPropertyName("featureExplanations")]
    public Dictionary<string, string> FeatureExplanations { get; set; } = new();
}
