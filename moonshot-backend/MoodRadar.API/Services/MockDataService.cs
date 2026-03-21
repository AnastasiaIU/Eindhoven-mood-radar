namespace MoodRadar.API.Services;

using MoodRadar.API.Models;

/// <summary>
/// Mock data service for Phase 1 development.
/// This will be replaced with real database queries in Phase 2.
/// </summary>
public interface IMockDataService
{
    List<Zone> GetAllZones();
    Zone? GetZoneById(int id);
    ZoneMoodResponse? GetZoneMood(int id);
    List<Event> GetAllEvents();
    List<Event> GetEventsByZone(int zoneId);
}

public class MockDataService : IMockDataService
{
    private readonly List<Zone> _zones;
    private readonly List<ZoneSnapshot> _zoneSnapshots;
    private readonly List<Event> _events;

    public MockDataService()
    {
        _zones = InitializeMockZones();
        _zoneSnapshots = InitializeMockZoneSnapshots();
        _events = InitializeMockEvents();
    }

    public List<Zone> GetAllZones()
    {
        return _zones;
    }

    public Zone? GetZoneById(int id)
    {
        return _zones.FirstOrDefault(z => z.Id == id);
    }

    public ZoneMoodResponse? GetZoneMood(int id)
    {
        var zone = GetZoneById(id);
        var snapshot = _zoneSnapshots
            .Where(s => s.ZoneId == id)
            .OrderByDescending(s => s.Timestamp)
            .FirstOrDefault();

        if (zone == null || snapshot == null)
            return null;

        return new ZoneMoodResponse
        {
            ZoneId = zone.Id,
            ZoneName = zone.Name,
            MoodLabel = snapshot.MoodLabel,
            Confidence = snapshot.Confidence,
            Timestamp = snapshot.Timestamp
        };
    }

    public List<Event> GetAllEvents()
    {
        return _events.OrderByDescending(e => e.StartTime).ToList();
    }

    public List<Event> GetEventsByZone(int zoneId)
    {
        return _events
            .Where(e => e.ZoneId == zoneId)
            .OrderByDescending(e => e.StartTime)
            .ToList();
    }

    private static List<Zone> InitializeMockZones()
    {
        return new List<Zone>
        {
            new Zone
            {
                Id = 1,
                Name = "Centrum",
                GeoJsonBoundary = "{\"type\": \"Polygon\", \"coordinates\": [...]}",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Zone
            {
                Id = 2,
                Name = "Woensel-Zuid",
                GeoJsonBoundary = "{\"type\": \"Polygon\", \"coordinates\": [...]}",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Zone
            {
                Id = 3,
                Name = "Woensel-Noord",
                GeoJsonBoundary = "{\"type\": \"Polygon\", \"coordinates\": [...]}",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Zone
            {
                Id = 4,
                Name = "Strijp",
                GeoJsonBoundary = "{\"type\": \"Polygon\", \"coordinates\": [...]}",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            }
        };
    }

    private static List<ZoneSnapshot> InitializeMockZoneSnapshots()
    {
        return new List<ZoneSnapshot>
        {
            new ZoneSnapshot
            {
                Id = 1,
                ZoneId = 1,
                Timestamp = DateTime.UtcNow,
                MoodLabel = "Energetic",
                Confidence = 0.85,
                FeatureJson = new Dictionary<string, object>
                {
                    { "active_events", 5 },
                    { "temperature", 15.2 },
                    { "precipitation_probability", 0.1 },
                    { "is_psv_match_day", false },
                    { "is_holiday", false }
                }
            },
            new ZoneSnapshot
            {
                Id = 2,
                ZoneId = 2,
                Timestamp = DateTime.UtcNow,
                MoodLabel = "Calm",
                Confidence = 0.72,
                FeatureJson = new Dictionary<string, object>
                {
                    { "active_events", 1 },
                    { "temperature", 14.8 },
                    { "precipitation_probability", 0.2 },
                    { "is_psv_match_day", false },
                    { "is_holiday", false }
                }
            },
            new ZoneSnapshot
            {
                Id = 3,
                ZoneId = 3,
                Timestamp = DateTime.UtcNow,
                MoodLabel = "Busy",
                Confidence = 0.88,
                FeatureJson = new Dictionary<string, object>
                {
                    { "active_events", 8 },
                    { "temperature", 15.0 },
                    { "precipitation_probability", 0.0 },
                    { "is_psv_match_day", true },
                    { "is_holiday", false }
                }
            },
            new ZoneSnapshot
            {
                Id = 4,
                ZoneId = 4,
                Timestamp = DateTime.UtcNow,
                MoodLabel = "Relaxed",
                Confidence = 0.79,
                FeatureJson = new Dictionary<string, object>
                {
                    { "active_events", 2 },
                    { "temperature", 15.5 },
                    { "precipitation_probability", 0.15 },
                    { "is_psv_match_day", false },
                    { "is_holiday", false }
                }
            }
        };
    }

    private static List<Event> InitializeMockEvents()
    {
        return new List<Event>
        {
            new Event
            {
                Id = 1,
                Source = "Eventbrite",
                ExternalId = "evt_001",
                Title = "Tech Conference 2026",
                StartTime = DateTime.UtcNow.AddHours(2),
                EndTime = DateTime.UtcNow.AddHours(8),
                ZoneId = 1,
                Category = "Conference",
                Description = "Annual tech conference in Eindhoven",
                Url = "https://eventbrite.com/e/tech-conference"
            },
            new Event
            {
                Id = 2,
                Source = "Eventbrite",
                ExternalId = "evt_002",
                Title = "PSV vs AFC Ajax",
                StartTime = DateTime.UtcNow.AddHours(4),
                EndTime = DateTime.UtcNow.AddHours(6),
                ZoneId = 3,
                Category = "Sports",
                Description = "Eredivisie match at Philips Stadion",
                Url = "https://psv.nl"
            },
            new Event
            {
                Id = 3,
                Source = "Ticketmaster",
                ExternalId = "evt_003",
                Title = "Live Jazz Night",
                StartTime = DateTime.UtcNow.AddHours(6),
                EndTime = DateTime.UtcNow.AddHours(10),
                ZoneId = 1,
                Category = "Music",
                Description = "Evening jazz performance at the cultural center",
                Url = "https://ticketmaster.com/jazz"
            },
            new Event
            {
                Id = 4,
                Source = "Eventbrite",
                ExternalId = "evt_004",
                Title = "Weekend Market",
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(5),
                ZoneId = 2,
                Category = "Market",
                Description = "Local farmers and crafts market",
                Url = "https://eventbrite.com/e/market"
            }
        };
    }
}
