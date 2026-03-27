namespace MoodRadar.API.Models;

/// <summary>
/// Event data from external sources (Ticketmaster, etc).
/// Represents a single event pulled from APIs and stored in cache.
/// </summary>
public class Event
{
    public int Id { get; set; }
    public string Source { get; set; } = string.Empty;  // "Ticketmaster", "PSV", etc.
    public string ExternalId { get; set; } = string.Empty;  // API's ID
    public string Title { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? ZoneId { get; set; }  // FK to Zone (for mood calculation)
    public string Category { get; set; } = string.Empty;  // music, sports, arts, etc.
    public string? Description { get; set; }
    public string? Url { get; set; }
    public double? Latitude { get; set; }  // Venue location
    public double? Longitude { get; set; }
    public DateTime CachedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Response model for a single event (API contract).
/// Used when returning events via REST endpoints.
/// Simplified version for frontend consumption.
/// </summary>
public class EventResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Url { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

/// <summary>
/// Paginated response for events.
/// Supports frontend pagination and reduces payload size.
/// </summary>
public class EventPageResponse
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalEvents { get; set; }
    public int TotalPages { get; set; }
    public List<EventResponse> Events { get; set; } = new();
}
