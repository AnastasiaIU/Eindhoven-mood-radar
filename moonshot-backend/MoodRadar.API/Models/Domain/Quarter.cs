namespace MoodRadar.API.Models.Domain;

/// <summary>
/// Represents a quarter (buurt) in Eindhoven.
/// Second level of geographical subdivision, belongs to a District.
/// </summary>
public class Quarter
{
    public int Id { get; set; }
    public int DistrictId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string GeoJsonBoundary { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public District? District { get; set; }
    public ICollection<Neighborhood> Neighborhoods { get; set; } = new List<Neighborhood>();
}
