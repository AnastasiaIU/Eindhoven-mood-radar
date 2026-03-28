# MoodRadar API Contracts

**Version:** 1.0 (Phase 1)  
**Environment:** Development (Mock Data)  
**Base URL:** `http://localhost:5000/api` (development)

---

## Overview

This document defines the JSON contracts for REST API endpoints. **The backend now live-polls Ticketmaster Discovery API** and serves events from an in-memory cache. Phase 2 will add database persistence while maintaining these JSON contracts.

**⚠️ CRITICAL DATA LIMITATION**: Ticketmaster Discovery API returns sparse event coverage for Eindhoven (~5 events per 24-hour window). This is a known API limitation, not a code issue. See [TICKETMASTER_SETUP.md](../TICKETMASTER_SETUP.md) for full details on limitations, root causes, and Phase 2 mitigation strategies.

### Standard Response Structure (Events)

Events endpoints return paginated data with metadata:

```json
{
  "data": [...],
  "pagination": {"page": 0, "pageSize": 20, "totalPages": 1, "totalItems": 5}
}
```

For errors:

```json
{
  "error": "error message"
}
```

---

## 1. GET /api/zones

### Description

Returns all zones (districts) in Eindhoven.

### Request

```bash
GET /api/zones
```

### Response (200 OK)

**Content-Type:** `application/json`

```json
[
  {
    "id": 1,
    "name": "Centrum",
    "geoJsonBoundary": "{\"type\": \"Polygon\", \"coordinates\": [...]}",
    "createdAt": "2026-02-15T10:00:00Z"
  },
  {
    "id": 2,
    "name": "Woensel-Zuid",
    "geoJsonBoundary": "{\"type\": \"Polygon\", \"coordinates\": [...]}",
    "createdAt": "2026-02-15T10:00:00Z"
  },
  {
    "id": 3,
    "name": "Woensel-Noord",
    "geoJsonBoundary": "{\"type\": \"Polygon\", \"coordinates\": [...]}",
    "createdAt": "2026-02-15T10:00:00Z"
  },
  {
    "id": 4,
    "name": "Strijp",
    "geoJsonBoundary": "{\"type\": \"Polygon\", \"coordinates\": [...]}",
    "createdAt": "2026-02-15T10:00:00Z"
  }
]
```

### Field Definitions

| Field | Type | Description |
|-------|------|-------------|
| `id` | integer | Unique zone identifier |
| `name` | string | Human-readable zone name (e.g., "Centrum", "Woensel-Zuid") |
| `geoJsonBoundary` | string | GeoJSON polygon representing zone boundary (for Leaflet.js rendering) |
| `createdAt` | ISO 8601 timestamp | When zone was added to system |

### Notes

- GeoJSON will be provided as a string; frontend must parse as JSON
- All timestamps are in UTC
- No pagination in Phase 1; all zones returned

---

## 2. GET /api/zones/:id/mood

### Description

Returns the current mood prediction for a specific zone, including confidence score and feature vector used for prediction.

### Request

```bash
GET /api/zones/1/mood
```

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | integer | Yes | Zone ID |

### Response (200 OK)

**Content-Type:** `application/json`

```json
{
  "zoneId": 1,
  "zoneName": "Centrum",
  "moodLabel": "Energetic",
  "confidence": 0.85,
  "timestamp": "2026-03-17T21:45:00Z"
}
```

### Field Definitions

| Field | Type | Description |
|-------|------|-------------|
| `zoneId` | integer | Zone identifier |
| `zoneName` | string | Human-readable zone name |
| `moodLabel` | string | Predicted mood: `"Energetic"`, `"Intense"`, `"Busy"`, `"Relaxed"`, or `"Calm"` |
| `confidence` | number | Confidence score (0.0 to 1.0) |
| `timestamp` | ISO 8601 timestamp | When this prediction was generated |

### Mood Label Reference

| Mood Label | Description | Color (UI) |
|-----------|-------------|-----------|
| `Energetic` | High activity, many events, good weather | Pastel Yellow |
| `Intense` | High-energy events, competitions, crowded | Orange |
| `Busy` | Many concurrent events, peak traffic | Coral |
| `Relaxed` | Few events, calm atmosphere | Sky Blue |
| `Calm` | Minimal activity, quiet | Teal |

### Response (404 Not Found)

```json
{
  "message": "Zone 999 not found"
}
```

### Notes

- Predictions are updated every 15 minutes (Phase 2)
- Phase 1 returns mock data with recent timestamp
- Confidence reflects model certainty (higher is more confident)

---

## 3. GET /api/events/ticketmaster

### Description

Returns paginated events from Ticketmaster in-memory cache. Events sourced from last `POST /api/events/refresh` call. Sorted by start time (ascending).

### Request

```bash
GET /api/events/ticketmaster?page=0&pageSize=20
```

### Query Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `page` | integer | 0 | Page number (0-indexed) |
| `pageSize` | integer | 20 | Results per page (1-50) |

### Response (200 OK)

**Content-Type:** `application/json`

```json
{
  "data": [
    {
      "id": 1,
      "title": "Derek Ogilvie - Up Close and Personal",
      "source": "Ticketmaster",
      "startTime": "2026-04-03T11:30:00Z",
      "endTime": null,
      "category": "Miscellaneous",
      "url": "https://www.ticketmaster.nl/event/derek-ogilvie-up-close-and-personal-tickets/399246674",
      "latitude": 51.44466,
      "longitude": 5.47564
    },
    {
      "id": 2,
      "title": "Magic Men: World Tour 2026",
      "source": "Ticketmaster",
      "startTime": "2026-06-06T18:00:00Z",
      "endTime": null,
      "category": "Music",
      "url": "https://www.ticketmaster.nl/event/magic-men-world-tour-2026-tickets/1243385837",
      "latitude": 51.41285,
      "longitude": 5.48132
    }
  ],
  "pagination": {
    "page": 0,
    "pageSize": 20,
    "totalPages": 1,
    "totalItems": 5
  }
}
```

### EventResponse Field Definitions

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `id` | integer | No | Internal cache ID (parsed from Ticketmaster event ID) |
| `title` | string | No | Event name |
| `source` | string | No | Always `"Ticketmaster"` (Phase 1) |
| `startTime` | ISO 8601 | No | Event start time (UTC) |
| `endTime` | ISO 8601 | Yes | Event end time (UTC), null if not provided by API |
| `category` | string | No | Classification segment (e.g., "Music", "Sports", "Miscellaneous") |
| `url` | string | Yes | Ticketmaster event page URL |
| `latitude` | double | Yes | Venue latitude (may be null if venue data incomplete) |
| `longitude` | double | Yes | Venue longitude (may be null if venue data incomplete) |

### Response (400 Bad Request)

```json
{
  "error": "pageSize must be between 1 and 50"
}
```

### Response (200 OK - Empty List)

```json
{
  "data": [],
  "pagination": {
    "page": 0,
    "pageSize": 20,
    "totalPages": 0,
    "totalItems": 0
  }
}
```

### Notes

- **Typical cache size**: ~5 events for 24-hour Eindhoven search (Ticketmaster API limitation)
- **Cache source**: Updated by `POST /api/events/refresh` call
- **All timestamps**: UTC
- **Sort order**: By startTime ascending (earliest first)
- **Phase 1 limitation**: No zone assignment yet; Phase 2 will add geospatial mapping
- **Coordinate handling**: Some venues lack geo-coordinates in Ticketmaster data; frontend must handle nullable lat/lon

---

## 4. POST /api/events/refresh

### Description

Manually trigger a Ticketmaster poll. Fetches events for Eindhoven (next 24 hours) and updates in-memory cache. Intended for cron/background service Phase 2; can be called manually for testing.

### Request

```bash
POST /api/events/refresh
```

### Response (200 OK)

**Content-Type:** `application/json`

```json
{
  "message": "Ticketmaster poll completed",
  "cachedCount": 5,
  "timestamp": "2026-03-27T14:30:00Z"
}
```

### Response (500 Internal Server Error)

```json
{
  "error": "Failed to poll Ticketmaster",
  "details": "HTTP 401: Unauthorized - check API key in environment variables"
}
```

### Field Definitions

| Field | Type | Description |
|-------|------|-------------|
| `message` | string | Status message |
| `cachedCount` | integer | Total events now in memory cache (typically ~5 for Eindhoven) |
| `timestamp` | ISO 8601 | When poll completed (UTC) |
| `error` | string | Error message (only on failure) |
| `details` | string | Additional error details |

### Ticketmaster Query Parameters (Fixed, Non-Configurable)

These parameters are hardcoded in the service; shown here for transparency:

| Parameter | Value | Description |
|-----------|-------|-------------|
| `apikey` | From config | Ticketmaster Discovery API key |
| `city` | `Eindhoven` | City name (not latitude/longitude) |
| `size` | `50` | Max events per page |
| `page` | `0` | First page (auto-fetches all pages if >1) |
| `startDateTime` | Now (UTC) | Search window start |
| `endDateTime` | Now + 24h (UTC) | Search window end |
| `includeTBA` | `yes` | Include "To Be Announced" events |
| `includeTBD` | `yes` | Include "To Be Determined" events |

### Response Codes

| Code | Meaning | Example |
|------|---------|---------|
| 200 | Success | Poll completed, cache updated |
| 500 | Error | API key missing, network error, JSON parse error |

### Notes

- **Ticketmaster API limitation**: Returns ~5 events for 24-hour Eindhoven search (free tier constraint)
- **Rate limiting**: Ticketmaster allows 5 requests/second and 5,000/day; service adds 300ms delays between pages
- **Cache type**: In-memory only (Phase 1); persists across requests but lost on app restart
- **No pagination on input**: Service fetches all available pages automatically; max 20 pages per poll (Ticketmaster limit)
- **Error handling**: Network errors and JSON parsing errors return 500; check logs for details

---

## 5. GET /api/weather

### Description

Returns all cached hourly weather forecasts for Eindhoven. Data sourced from Open-Meteo API (free, no key required). Cache updated every 15 minutes by background service.

**Data source:** [Open-Meteo](https://open-meteo.com) Free Weather API  
**Update frequency:** Every 15 minutes (background service)  
**Coverage:** Eindhoven (51.4416°N, 5.4699°E)  
**Forecast window:** Next day, 1-hour resolution

### Request

```bash
GET /api/weather
```

### Response (200 OK)

**Content-Type:** `application/json`

```json
[
  {
    "id": 1,
    "snapshotHour": "2026-03-28T14:00:00Z",
    "temperatureC": 12.3,
    "precipitationProbability": 25,
    "cloudCover": 65,
    "cachedAt": "2026-03-28T13:58:00Z"
  },
  {
    "id": 2,
    "snapshotHour": "2026-03-28T15:00:00Z",
    "temperatureC": 13.1,
    "precipitationProbability": 20,
    "cloudCover": 70,
    "cachedAt": "2026-03-28T13:58:00Z"
  }
]
```

### Field Definitions

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `id` | integer | No | Internal cache row ID |
| `snapshotHour` | ISO 8601 | No | Hour timestamp (UTC, rounded to nearest hour) |
| `temperatureC` | double | No | Temperature in Celsius (2m above ground) |
| `precipitationProbability` | integer | No | Probability of precipitation (0–100 %) |
| `cloudCover` | integer | No | Cloud cover percentage (0–100 %) |
| `cachedAt` | ISO 8601 | No | When this record was cached (UTC) |

### Notes

- All timestamps are in UTC
- Returned in ascending order by `snapshotHour`
- Cache is in-memory; persists across requests, lost on app restart
- Typical cache size: ~24 records (1 day × 24 hours)

### Response (500 Internal Server Error)

```json
{
  "error": "Error retrieving weather data"
}
```

---

## 6. GET /api/weather/hour

### Description

Retrieve weather for a specific hour from cache. Matches to nearest hour in cache (ignores minutes/seconds).

### Request

```bash
GET /api/weather/hour?timestamp=2026-03-28T14:30:00Z
```

### Query Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `timestamp` | ISO 8601 | Yes | Hour to retrieve. Will match to nearest hour in cache (e.g., `14:30:00Z` → `14:00:00Z`) |

### Response (200 OK)

**Content-Type:** `application/json`

```json
{
  "id": 1,
  "snapshotHour": "2026-03-28T14:00:00Z",
  "temperatureC": 12.3,
  "precipitationProbability": 25,
  "cloudCover": 65,
  "cachedAt": "2026-03-28T13:58:00Z"
}
```

### Response (404 Not Found)

```json
{
  "error": "No weather data for requested hour"
}
```

Hour is not in cache (cache older than 1 day, or hour is in the past).

### Response (500 Internal Server Error)

```json
{
  "error": "Error retrieving weather data"
}
```

---

## 7. POST /api/weather/fetch

### Description

Manually trigger a weather fetch from Open-Meteo API. Normally called by background service every 15 minutes. Useful for testing or forcing manual updates.

### Request

```bash
POST /api/weather/fetch
```

### Response (200 OK)

**Content-Type:** `application/json`

```json
[
  {
    "id": 1,
    "snapshotHour": "2026-03-28T14:00:00Z",
    "temperatureC": 12.3,
    "precipitationProbability": 25,
    "cloudCover": 65,
    "cachedAt": "2026-03-28T13:58:15Z"
  },
  {
    "id": 2,
    "snapshotHour": "2026-03-28T15:00:00Z",
    "temperatureC": 13.1,
    "precipitationProbability": 20,
    "cloudCover": 70,
    "cachedAt": "2026-03-28T13:58:15Z"
  }
]
```

### Response (500 Internal Server Error)

```json
{
  "error": "Error fetching weather data from external API"
}
```

Network error, OpenMeteo API unreachable, or JSON parsing error.

### Notes

- Returns newly fetched records (1-day forecast)
- Replaces entire cache on each call
- No rate limiting on client side; Open-Meteo API allows unlimited free calls
- Response is list of all cached records after fetch is complete

---

## 8. DELETE /api/weather/cache

### Description

Remove old weather records from cache (older than specified days). Reduces memory footprint for long-running apps.

### Request

```bash
DELETE /api/weather/cache?olderThanDays=1
```

### Query Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `olderThanDays` | integer | 1 | Age threshold in days |

### Response (200 OK)

**Content-Type:** `application/json`

```json
{
  "message": "Cache cleared (older than 1 day). 42 records remain."
}
```

### Response (500 Internal Server Error)

```json
{
  "error": "Error clearing cache"
}
```

### Notes

- Operates on in-memory cache only
- Returns count of remaining records after deletion
- Safe to call multiple times (idempotent)

---

## Error Handling

All endpoints may return these error responses:

### 400 Bad Request

```json
{
  "message": "Invalid query parameters",
  "errors": ["limit must be between 1 and 1000"]
}
```

### 404 Not Found

```json
{
  "message": "Resource not found"
}
```

### 500 Internal Server Error

```json
{
  "message": "Internal server error",
  "traceId": "0HN1GKOFP6M61:00000001"
}
```

---

## CORS Headers

All endpoints support CORS for requests from:

- `http://localhost:3000` (Next.js development server)
- Production: `https://moodradar-frontend.render.com`

---

## Rate Limiting (Phase 2)

Will implement rate limiting:

- Public endpoints: 100 requests per minute per IP
- Authenticated clients: 1000 requests per minute

---

## Typography

**Timestamp Format:** ISO 8601 (e.g., `2026-03-17T21:45:32.123Z`)

**Confidence Format:** Decimal 0.0–1.0

**Boolean Format:** JSON `true`/`false`

---

## Implementation Notes for Frontend

### Parsing GeoJSON Boundaries

```javascript
const boundary = JSON.parse(zone.geoJsonBoundary);
// Use with Leaflet.js:
// L.geoJSON(boundary).addTo(map);
```

### Mood Color Mapping

```javascript
const moodColors = {
  "Energetic": "#FFFACD",  // Pastel Yellow
  "Intense": "#FFB347",    // Orange
  "Busy": "#FF7F50",        // Coral
  "Relaxed": "#87CEEB",     // Sky Blue
  "Calm": "#20B2AA"         // Teal
};
```

### Confidence Display

Show as percentage: `(confidence * 100).toFixed(0) + "%"`

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 2.1 | 2026-03-28 | Added weather endpoints (GET /api/weather, GET /api/weather/hour, POST /api/weather/fetch, DELETE /api/weather/cache) sourced from Open-Meteo API; background service now polls weather every 15 min |
| 2.0 | 2026-03-27 | Updated to Ticketmaster Discovery API v2 (live); removed mock data; added coverage limitations |
| 1.1 | 2026-03-17 | Added pagination, filtering by zone/category (planned) |
| 1.0 | 2026-03-17 | Initial Phase 1 contracts with mock data |

---

**Last Updated:** 2026-03-28  
**Owner:** Backend Team  
**Status:** Phase 1 Development (Live with Ticketmaster Discovery API v2 + Open-Meteo Weather)  

**Important**: All Ticketmaster endpoints return live data from free tier API. Event coverage is sparse (~5 events per 24 hours for Eindhoven). See [TICKETMASTER_SETUP.md](../TICKETMASTER_SETUP.md) for detailed limitations, causes, and Phase 2+ mitigation strategies.
