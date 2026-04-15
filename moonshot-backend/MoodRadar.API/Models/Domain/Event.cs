namespace MoodRadar.API.Models.Domain;

/// <summary>
/// Event data from external sources (Ticketmaster, PSV, etc.).
/// Represents a single event pulled from APIs and stored in cache.
/// Used as an input for mood calculation and for event feed retrieval.
/// </summary>
public class Event
{
    public int Id { get; set; }
    
    /// <summary>
    /// API source identifier: "Ticketmaster", "PSV", "LocalVenue", etc.
    /// </summary>
    public string Source { get; set; } = string.Empty;
    
    /// <summary>
    /// External API's native event ID (if applicable).
    /// Allows us to track which API provided this event and avoid duplicates.
    /// </summary>
    public string ExternalId { get; set; } = string.Empty;
    
    /// <summary>
    /// Event title/name.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Event start date and time (UTC).
    /// </summary>
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// Event end date and time (UTC). May be null for open-ended events.
    /// </summary>
    public DateTime? EndTime { get; set; }
    
    /// <summary>
    /// Foreign key to Neighborhood (for mood calculation and spatial partitioning).
    /// Null if event location has not yet been assigned to a neighborhood.
    /// </summary>
    public int? NeighborhoodId { get; set; }
    
    /// <summary>
    /// Long-form event description from API.
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// URL to event details or ticket sales page.
    /// </summary>
    public string? Url { get; set; }
    
    /// <summary>
    /// Venue latitude (WGS84).
    /// </summary>
    public double? Latitude { get; set; }
    
    /// <summary>
    /// Venue longitude (WGS84).
    /// </summary>
    public double? Longitude { get; set; }
    
    /// <summary>
    /// When this record was inserted or last refreshed from the API.
    /// Used to determine cache staleness.
    /// </summary>
    public DateTime CachedAt { get; set; } = DateTime.UtcNow;

    public string RawData { get; set; }  // JSONB
}
