namespace MoodRadar.API.Models.Domain;

/// <summary>
/// Represents a district (wijkgebied) in Eindhoven.
/// Main level of geographical subdivision.
/// </summary>
public class District
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string GeoJsonBoundary { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<Quarter> Quarters { get; set; } = new List<Quarter>();
}
