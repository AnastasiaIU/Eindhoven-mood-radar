namespace MoodRadar.API.Models;

/// <summary>
/// Event data from external sources (Eventbrite, Ticketmaster, etc).
/// </summary>
public class Event
{
    public int Id { get; set; }
    public string Source { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? ZoneId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Url { get; set; }
}

/// <summary>
/// Response model for event list.
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
}
