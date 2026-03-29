namespace MoodRadar.API.Models.Domain;

/// <summary>
/// Mood prediction snapshot for a neighborhood at a specific timestamp.
/// Stores the model's predicted mood label, confidence score, and feature vectors (SHAP values).
/// </summary>
public class NeighborhoodSnapshot
{
    public int Id { get; set; }
    public int NeighborhoodId { get; set; }
    public DateTime Timestamp { get; set; }
    public string MoodLabel { get; set; } = string.Empty;
    public double Confidence { get; set; }
    /// <summary>
    /// SHAP values or feature importance scores as JSON.
    /// Stores model explainability data for the Transparency Panel.
    /// </summary>
    public Dictionary<string, object>? FeatureJson { get; set; }
}
