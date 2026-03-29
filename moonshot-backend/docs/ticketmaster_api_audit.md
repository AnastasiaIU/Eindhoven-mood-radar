# Ticketmaster Discovery API v2 - Integration & Coverage Audit

**Prepared for**: AI Specialist, Frontend Team, Backend Team  
**Date**: March 2026  
**Purpose**: Document actual Ticketmaster response shape, coverage limitations, and data quality for mood signal generation

---

## 1. Ticketmaster Discovery API Authentication

### API Credentials

- **Endpoint**: https://app.ticketmaster.com/discovery/v2/
- **Authentication**: API key in query parameter (simple, no OAuth)
- **Key format**: Query param `apikey={your_key_here}`
- **Developer account**: https://developer.ticketmaster.com/
- **Free tier**: No credit card required; rate limits: 5 req/sec, 5,000/day

### Getting Your API Key

1. Go to https://developer.ticketmaster.com/
2. Sign in or create free developer account
3. Create new app → receive API key immediately
4. Store in environment variable: `TICKETMASTER__APIKEY=...` (never commit to git)
5. Key is immediately usable (no activation delay)

---

## 2. Events Search Endpoint

### Request

```
GET /discovery/v2/events.json
```

### Query Parameters (Current Implementation)

```json
{
  "apikey": "your_key_here",
  "city": "Eindhoven",
  "size": 50,
  "page": 0,
  "startDateTime": "2026-03-27T14:30:00Z",
  "endDateTime": "2026-03-28T14:30:00Z",
  "includeTBA": "yes",
  "includeTBD": "yes"
}
```

### Parameters Explained

| Parameter | Current Value | Notes |
|-----------|---------------|-------|
| `apikey` | From config | Ticketmaster Discovery API key |
| `city` | `Eindhoven` | City name (not coordinates) |
| `size` | `50` | Max events per page (max 50) |
| `page` | `0, 1, 2...` | 0-indexed pagination |
| `startDateTime` | ISO 8601 UTC | Search window start (now) |
| `endDateTime` | ISO 8601 UTC | Search window end (now + 24h) |
| `includeTBA` | `yes` | Include "To Be Announced" events |
| `includeTBD` | `yes` | Include "To Be Determined" events |

### Alternative Query Parameters (Not Currently Used)

Per Ticketmaster API docs, these could supplement the search but aren't needed:

| Parameter | Example | Note |
|-----------|---------|------|
| `countryCode` | `NL` | Already implicit with city search |
| `geoPoint` | `51.4416,5.4699` | Alternative to city (geographic) |
| `radius` | `10` | Radius in km (with geoPoint) |
| `classificationName` | `music`, `sports` | Filter by event type |
| `preferredCountry` | `us`, `ca` | Popularity boost (default: us) |

---

## 3. Response Shape

### Full Response Example

Actual response from Ticketmaster (March 2026):

```json
{
  "_embedded": {
    "events": [
      {
        "id": "G5vGZ7QVE8B-h",
        "name": "Derek Ogilvie - Up Close and Personal",
        "url": "https://www.ticketmaster.nl/event/derek-ogilvie-up-close-and-personal-tickets/399246674?language=en-us",
        "dates": {
          "start": {
            "localDate": "2026-04-03",
            "localTime": "11:30:00",
            "dateTime": "2026-04-03T11:30:00Z",
            "timezone": "Europe/Amsterdam"
          },
          "end": null,
          "status": {
            "code": "offsale"
          },
          "spanMultipleDays": false
        },
        "classifications": [
          {
            "primary": true,
            "segment": {
              "id": "KZFzniwnSyZfZ7v6na",
              "name": "Miscellaneous"
            },
            "genre": {
              "id": "KZazBEonSMnZfZ7v7l1",
              "name": "Miscellaneous"
            }
          }
        ],
        "_embedded": {
          "venues": [
            {
              "id": "Z58tJ2c",
              "name": "Lecture Hall Eindhoven",
              "type": "Venue",
              "address": {
                "address": "Ton Dubbelaarsplein 1"
              },
              "city": {
                "name": "Eindhoven"
              },
              "state": {
                "name": "North Brabant",
                "stateCode": "NB"
              },
              "country": {
                "name": "Netherlands",
                "countryCode": "NL"
              },
              "location": {
                "longitude": "5.47564",
                "latitude": "51.44466"
              },
              "timezone": "Europe/Amsterdam"
            }
          ]
        }
      }
    ]
  },
  "page": {
    "size": 50,
    "totalElements": 2,
    "totalPages": 1,
    "number": 0
  }
}
```

### Mapped to Backend Models

```csharp
public class EventResponse
{
    public int Id { get; set; }                    // Parsed from event.id
    public string Title { get; set; }              // event.name
    public string Source { get; set; }             // Always "Ticketmaster"
    public DateTime StartTime { get; set; }        // event.dates.start.dateTime
    public DateTime? EndTime { get; set; }         // event.dates.end.dateTime (nullable)
    public string Category { get; set; }           // event.classifications[0].segment.name
    public string? Url { get; set; }               // event.url
    public double? Latitude { get; set; }          // event._embedded.venues[0].location.latitude (string → double)
    public double? Longitude { get; set; }         // event._embedded.venues[0].location.longitude (string → double)
}
```

### Key Observations

1. **Coordinates as strings**: Ticketmaster returns lat/lon as JSON strings ("51.44466"), not numbers. Backend parses to double using `InvariantCulture`.
2. **End time nullable**: Many events lack end time; frontend must handle null.
3. **Nested venues**: Location data is deeply nested; requires drilling `_embedded.venues[0].location`.
4. **Pagination at page level**: `page.number` (0-indexed), `page.size`, `page.totalElements`, `page.totalPages`.

---

## 4. Event Categories

### Classification Structure

Ticketmaster uses segment/genre/subgenre hierarchy. Most relevant: **segment.name**

```json
"classifications": [
  {
    "primary": true,
    "segment": { "id": "KZFzniwnSyZfZ7v6na", "name": "Music" },
    "genre": { "id": "ABCxyz", "name": "Rock" },
    "subGenre": { "id": "XYZabc", "name": "Alternative Rock" }
  }
]
```

---

## 5. CRITICAL COVERAGE LIMITATIONS

### What We Tested

**Date Range**: March 27 - April 5, 2026 (10 days, but search window is 24h at a time)  
**Search Method**: `city=Eindhoven` (not geoPoint)  
**Result**: **~2 events for 10 days window**

**Example discoverable events**:
- Derek Ogilvie - Up Close and Personal (2 times on April 3)

### What Ticketmaster.nl Website Shows

Browsing ticketmaster.nl directly with city=Eindhoven filter shows ~10 events, but Discovery API returns only 2. **Data sources are different.**

### Root Causes

**1. Limited Syndication Partners**

- API sources only from: Ticketmaster, TicketWeb, Universe, FrontGate, Ticketmaster Sport, MoshTix
- Many local Dutch venues and independent promoters NOT included
- Example: Small theater groups, university events, community festivals

**2. Tier-2 City Bias**

- Ticketmaster's data concentration: USA (primary), then UK, then Western Europe
- Netherlands is "supported" but sparse relative to major markets
- Eindhoven is a tier-2 city (population ~230K); not a priority for syndicated event data

**3. Free Tier Limitations**

- Official FAQ: "If your use case demands higher limits, consider Discovery Feed" (paid tier)
- Free tier may receive delayed or limited data vs. paid tiers
- No official documentation of free-tier event filtering, but behavior suggests it

**4. Event Visibility & Status**

- API respects `publicVisibilityStartDateTime`; presale/draft events filtered
- TBA/TBD events included via `includeTBA=yes` but still limited

### Verified Comparisons

| Source | Eindhoven Events | Time Window | Notes |
|--------|------------------|-------------|-------|
| Ticketmaster Discovery API (free) | ~2 | 10d | City-based search |
| ticketmaster.nl (website) | ~10 | 10d | Same search criteria |
| Difference | -80% | Same | API returns subset |

---

## 6. Mitigation Strategy (Phase 2+)

### Short-term (Phase 1)

- ✅ Accept 2 events/10d as MVP validation data
- ✅ Document bias in Transparency Panel ("Event data sourced from Ticketmaster Discovery API")
- ✅ Acknowledge data quality gap to stakeholders

### Medium-term (Phase 2)

1. **Add Secondary Data Source**
   - Integrate Eventim.nl (Dutch event platform)
   - Or integrate ThreeTickets / De Ticketshop
   - Fetch and merge with Ticketmaster results

2. **Direct Venue Partnerships**
   - PSV Stadion: Already via football-data.org
   - Philips Stadion: API available
   - Local music venues: Direct API integration or web scraping

---

## 7. Known Issues & Workarounds

### Issue 1: Very Few Events (Expected)

- **Symptom**: Only 2 events returned for 10 days Eindhoven search
- **Cause**: Ticketmaster free tier limited syndication for tier-2 Dutch cities
- **Workaround**: Add local Dutch event APIs in Phase 2

### Issue 2: Sparse Coverage in Outer Districts

- **Symptom**: Woensel-Noord, Stratum zones show no events
- **Cause**: Ticketmaster centralizes data in city center (Centrum, Strijp-S)
- **Workaround**: Zone-mapping must budget for "Calm" mood in low-event zones

### Issue 3: Null Coordinates on Some Venues

- **Symptom**: Some events have latitude/longitude == null
- **Cause**: Ticketmaster venue data incomplete
- **Workaround**: Frontend handles nullable coordinates, uses event URL for fallback
