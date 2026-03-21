namespace MoodRadar.API.Models;

/// <summary>
/// Represents a district/zone in Eindhoven with mood prediction.
/// </summary>
public class Zone
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string GeoJsonBoundary { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
