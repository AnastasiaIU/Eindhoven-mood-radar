namespace MoodRadar.API.Models.Domain;

/// <summary>
/// Represents a neighborhood (wijk) in Eindhoven.
/// Third level of geographical subdivision, belongs to a Quarter.
/// </summary>
public class Neighborhood
{
    public int Id { get; set; }
    public int QuarterId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string GeoJsonBoundary { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Quarter? Quarter { get; set; }
}
