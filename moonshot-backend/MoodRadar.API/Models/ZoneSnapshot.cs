namespace MoodRadar.API.Models;

/// <summary>
/// Mood prediction snapshot for a zone at a specific timestamp.
/// </summary>
public class ZoneSnapshot
{
    public int Id { get; set; }
    public int ZoneId { get; set; }
    public DateTime Timestamp { get; set; }
    public string MoodLabel { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public Dictionary<string, object>? FeatureJson { get; set; }
}

/// <summary>
/// Response model for zone mood data.
/// </summary>
public class ZoneMoodResponse
{
    public int ZoneId { get; set; }
    public string ZoneName { get; set; } = string.Empty;
    public string MoodLabel { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public DateTime Timestamp { get; set; }
}
