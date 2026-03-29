namespace MoodRadar.API.Models.Dtos.Responses;

using System.Text.Json.Serialization;

/// <summary>
/// Response DTO for GET /api/events endpoint (paginated list).
/// Wraps individual event DTOs with pagination metadata.
/// </summary>
public class EventListResponseDto
{
    [JsonPropertyName("data")]
    public List<EventResponseDto> Data { get; set; } = new();

    [JsonPropertyName("pagination")]
    public PaginationMetaDto Pagination { get; set; } = new();
}

/// <summary>
/// Response DTO for a single event.
/// Represents simplified event information sent to the frontend.
/// Omits sensitive backend fields (e.g., ExternalId, CachedAt).
/// </summary>
public class EventResponseDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("startTime")]
    public DateTime StartTime { get; set; }

    [JsonPropertyName("endTime")]
    public DateTime? EndTime { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("latitude")]
    public double? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double? Longitude { get; set; }

    [JsonPropertyName("neighborhoodId")]
    public int? NeighborhoodId { get; set; }
}

/// <summary>
/// Pagination metadata for list responses.
/// Enables client-side pagination and result sizing.
/// </summary>
public class PaginationMetaDto
{
    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }

    [JsonPropertyName("totalItems")]
    public int TotalItems { get; set; }
}
