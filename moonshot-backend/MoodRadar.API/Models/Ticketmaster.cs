namespace MoodRadar.API.Models;

using System.Text.Json.Serialization;

/// <summary>
/// Response models for Ticketmaster Discovery API v2.
/// Documents the actual structure returned by Ticketmaster.
/// API Reference: https://developer.ticketmaster.com/products-and-docs/apis/discovery-api/v2/
/// </summary>
/// 
/// <summary>
/// Root response from Ticketmaster events search endpoint.
/// GET /discovery/v2/events.json
/// </summary>
public class TicketmasterSearchResponse
{
    [JsonPropertyName("_embedded")]
    public TicketmasterEmbedded? Embedded { get; set; }

    [JsonPropertyName("page")]
    public TicketmasterPageInfo? Page { get; set; }
}

/// <summary>
/// Embedded data container in Ticketmaster response.
/// </summary>
public class TicketmasterEmbedded
{
    [JsonPropertyName("events")]
    public List<TicketmasterEvent> Events { get; set; } = new();
}

/// <summary>
/// Single event from Ticketmaster search results.
/// </summary>
public class TicketmasterEvent
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("dates")]
    public TicketmasterDates? Dates { get; set; }

    [JsonPropertyName("classifications")]
    public List<TicketmasterClassification> Classifications { get; set; } = new();

    [JsonPropertyName("_embedded")]
    public TicketmasterEventEmbedded? Embedded { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("priceRanges")]
    public List<TicketmasterPriceRange>? PriceRanges { get; set; }

    [JsonPropertyName("images")]
    public List<TicketmasterImage>? Images { get; set; }
}

/// <summary>
/// Date/time information for a Ticketmaster event.
/// </summary>
public class TicketmasterDates
{
    [JsonPropertyName("start")]
    public TicketmasterDateTime? Start { get; set; }

    [JsonPropertyName("end")]
    public TicketmasterDateTime? End { get; set; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }
}

/// <summary>
/// DateTime information including both date and time.
/// </summary>
public class TicketmasterDateTime
{
    [JsonPropertyName("dateTime")]
    public DateTime? DateTime { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("localDate")]
    public string? LocalDate { get; set; }

    [JsonPropertyName("localTime")]
    public string? LocalTime { get; set; }
}

/// <summary>
/// Event classification (segment, genre, sub-genre, type).
/// </summary>
public class TicketmasterClassification
{
    [JsonPropertyName("segment")]
    public TicketmasterSegment? Segment { get; set; }

    [JsonPropertyName("genre")]
    public TicketmasterGenre? Genre { get; set; }

    [JsonPropertyName("subGenre")]
    public TicketmasterSubGenre? SubGenre { get; set; }

    [JsonPropertyName("type")]
    public TicketmasterType? Type { get; set; }

    [JsonPropertyName("subType")]
    public TicketmasterSubType? SubType { get; set; }
}

/// <summary>
/// Primary segment (Music, Sports, Arts, etc).
/// </summary>
public class TicketmasterSegment
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Genre classification.
/// </summary>
public class TicketmasterGenre
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Sub-genre classification.
/// </summary>
public class TicketmasterSubGenre
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Type classification.
/// </summary>
public class TicketmasterType
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Sub-type classification.
/// </summary>
public class TicketmasterSubType
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Embedded data within an event (venues, attractions).
/// </summary>
public class TicketmasterEventEmbedded
{
    [JsonPropertyName("venues")]
    public List<TicketmasterVenue>? Venues { get; set; }

    [JsonPropertyName("attractions")]
    public List<TicketmasterAttraction>? Attractions { get; set; }
}

/// <summary>
/// Venue information with location coordinates.
/// </summary>
public class TicketmasterVenue
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public TicketmasterAddress? Address { get; set; }

    [JsonPropertyName("location")]
    public TicketmasterLocation? Location { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }
}

/// <summary>
/// Address information for a venue.
/// </summary>
public class TicketmasterAddress
{
    [JsonPropertyName("address")]
    public string? Street { get; set; }

    [JsonPropertyName("line1")]
    public string? Line1 { get; set; }

    [JsonPropertyName("line2")]
    public string? Line2 { get; set; }
}

/// <summary>
/// Geographic location (latitude, longitude).
/// Note: Ticketmaster returns these as strings, e.g., "51.4416"
/// </summary>
public class TicketmasterLocation
{
    [JsonPropertyName("latitude")]
    public string? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public string? Longitude { get; set; }
}

/// <summary>
/// Attraction (artist, performer, team).
/// </summary>
public class TicketmasterAttraction
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

/// <summary>
/// Price range for an event.
/// </summary>
public class TicketmasterPriceRange
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("min")]
    public decimal? Min { get; set; }

    [JsonPropertyName("max")]
    public decimal? Max { get; set; }
}

/// <summary>
/// Event image.
/// </summary>
public class TicketmasterImage
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("height")]
    public int? Height { get; set; }

    [JsonPropertyName("width")]
    public int? Width { get; set; }
}

/// <summary>
/// Pagination metadata from Ticketmaster response.
/// </summary>
public class TicketmasterPageInfo
{
    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("totalElements")]
    public int TotalElements { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }

    [JsonPropertyName("number")]
    public int Number { get; set; } // 0-indexed page number
}

/// <summary>
/// HTTP response headers from Ticketmaster containing rate limit information.
/// Shared model used across API services.
/// </summary>
public class RateLimitInfo
{
    public int Limit { get; set; }
    public int Remaining { get; set; }
    public int ResetSeconds { get; set; }
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

    public override string ToString()
    {
        return $"RateLimit: {Remaining}/{Limit} remaining, resets in {ResetSeconds}s";
    }
}
