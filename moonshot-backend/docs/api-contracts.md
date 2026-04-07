# MoodRadar API Contracts

Version: 3.0 (Current Implementation)
Environment: Development by default
Base URL: http://localhost:5000/api

---

## Scope

This document reflects the current backend implementation in controllers, DTOs, and services.
It intentionally does not describe removed legacy routes such as `/api/zones`.

---

## Production Restrictions

The following endpoints are decorated with `NonProductionOnlyAttribute` and return `403` in Production:

- `POST /api/events/refresh`
- `POST /api/weather/fetch`
- `POST /api/scraper/venues`

Error payload in production:

```json
{
  "error": "This endpoint is disabled in production."
}
```

---

## 1) Districts

### GET /api/districts
Returns all districts.

Response 200:

```json
[
  {
    "id": 1,
    "name": "Centrum",
    "geoJsonBoundary": "{\"type\":\"MultiPolygon\",...}",
    "createdAt": "2026-04-07T10:00:00Z"
  }
]
```

### GET /api/districts/{id}
Returns one district including quarters.

Response 200:

```json
{
  "id": 1,
  "name": "Centrum",
  "geoJsonBoundary": "{\"type\":\"MultiPolygon\",...}",
  "createdAt": "2026-04-07T10:00:00Z",
  "quarters": [
    {
      "id": 1,
      "name": "Centrum",
      "districtId": 1,
      "geoJsonBoundary": "{\"type\":\"MultiPolygon\",...}",
      "createdAt": "2026-04-07T10:00:00Z"
    }
  ]
}
```

Response 404:

```json
{
  "message": "District 999 not found"
}
```

---

## 2) Quarters

### GET /api/quarters?districtId={id}
Returns all quarters, optionally filtered by districtId.

Query params:
- `districtId` optional integer

Response 200:

```json
[
  {
    "id": 1,
    "name": "Centrum",
    "districtId": 1,
    "geoJsonBoundary": "{\"type\":\"MultiPolygon\",...}",
    "createdAt": "2026-04-07T10:00:00Z"
  }
]
```

### GET /api/quarters/{id}
Returns one quarter including neighborhoods.

Response 200:

```json
{
  "id": 1,
  "name": "Centrum",
  "districtId": 1,
  "geoJsonBoundary": "{\"type\":\"MultiPolygon\",...}",
  "createdAt": "2026-04-07T10:00:00Z",
  "neighborhoods": [
    {
      "id": 1,
      "name": "Binnenstad",
      "quarterId": 1,
      "quarterName": "Centrum",
      "geoJsonBoundary": "{\"type\":\"MultiPolygon\",...}",
      "createdAt": "2026-04-07T10:00:00Z"
    }
  ]
}
```

Response 404:

```json
{
  "message": "Quarter 999 not found"
}
```

---

## 3) Neighborhoods

### GET /api/neighborhoods?quarterId={id}&districtId={id}
Returns neighborhood metadata only (no mood data).

Query params:
- `quarterId` optional integer
- `districtId` optional integer

Response 200:

```json
[
  {
    "id": 1,
    "name": "Binnenstad",
    "quarterId": 1,
    "quarterName": "Centrum",
    "geoJsonBoundary": "{\"type\":\"MultiPolygon\",...}",
    "createdAt": "2026-04-07T10:00:00Z"
  }
]
```

### GET /api/neighborhoods/{id}
Returns one neighborhood metadata record (no mood data).

Response 404:

```json
{
  "message": "Neighborhood 999 not found"
}
```

---

## 4) Mood Forecasts (NeighborhoodSnapshot)

### GET /api/mood/neighborhood/{neighborhoodId}
Returns upcoming hourly snapshots for the next 24 hours for one neighborhood.

Response 200:

```json
{
  "neighborhoodId": 1,
  "neighborhoodName": "Binnenstad",
  "forecastStartUtc": "2026-04-07T15:00:00Z",
  "forecastEndUtcExclusive": "2026-04-08T15:00:00Z",
  "snapshots": [
    {
      "timestamp": "2026-04-07T15:00:00Z",
      "moodLabel": "Busy",
      "confidence": 0.8,
      "features": {
        "event_count": 3,
        "hour_of_day": 15
      }
    }
  ]
}
```

Response 404:

```json
{
  "error": "Neighborhood not found"
}
```

### GET /api/mood/all
Returns upcoming hourly snapshots for all neighborhoods.

Response 200:

```json
{
  "forecastStartUtc": "2026-04-07T15:00:00Z",
  "forecastEndUtcExclusive": "2026-04-08T15:00:00Z",
  "neighborhoods": [
    {
      "neighborhoodId": 1,
      "neighborhoodName": "Binnenstad",
      "forecastStartUtc": "2026-04-07T15:00:00Z",
      "forecastEndUtcExclusive": "2026-04-08T15:00:00Z",
      "snapshots": [
        {
          "timestamp": "2026-04-07T15:00:00Z",
          "moodLabel": "Busy",
          "confidence": 0.8,
          "features": {}
        }
      ]
    }
  ]
}
```

### GET /api/mood/neighborhood/{neighborhoodId}/snapshot?timestamp={ISO8601}
Returns one exact snapshot at timestamp.

Query params:
- `timestamp` required DateTime (ISO-8601). Timestamp is normalized to UTC in controller logic.

Response 200:

```json
{
  "timestamp": "2026-04-07T15:00:00Z",
  "moodLabel": "Busy",
  "confidence": 0.8,
  "features": {
    "event_count": 3
  }
}
```

Response 400:

```json
{
  "error": "Query parameter 'timestamp' is required (ISO-8601 format, UTC recommended)."
}
```

Response 404:

```json
{
  "error": "No mood snapshot found for this neighborhood at the requested timestamp",
  "neighborhoodId": 1,
  "timestamp": "2026-04-07T15:00:00Z"
}
```

---

## 5) Events

### GET /api/events
Returns paginated events for next 24 hours (`StartTime >= now && StartTime <= now+24h`).

Query params:
- `page` default `0`, must be `>= 0`
- `pageSize` default `20`, range `1..50`
- `neighborhoodId` optional integer
- `category` optional string (currently accepted but not applied in DB query)

Response 200:

```json
{
  "data": [
    {
      "id": 1,
      "title": "Example Event",
      "source": "Ticketmaster",
      "startTime": "2026-04-07T18:00:00Z",
      "endTime": null,
      "url": null,
      "latitude": null,
      "longitude": null,
      "neighborhoodId": null
    }
  ],
  "pagination": {
    "page": 0,
    "pageSize": 20,
    "totalPages": 1,
    "totalItems": 1
  }
}
```

Response 400 examples:

```json
{ "error": "pageSize must be between 1 and 50" }
```

```json
{ "error": "page must be >= 0" }
```

### GET /api/events/{id}
Returns one event by database ID.

Response 404:

```json
{ "error": "Event '999' not found" }
```

### POST /api/events/refresh (non-production only)
Triggers Ticketmaster poll and updates database cache.

Response 200:

```json
{
  "message": "Ticketmaster poll completed",
  "cachedCount": 5,
  "timestamp": "2026-04-07T14:00:00Z"
}
```

---

## 6) Weather

### GET /api/weather
Returns all cached weather rows sorted by `snapshotHour`.

Response 200:

```json
[
  {
    "snapshotHour": "2026-04-07T15:00:00Z",
    "temperatureC": 14.2,
    "precipitationProbability": 30,
    "cloudCover": 70,
    "cachedAt": "2026-04-07T14:00:00Z"
  }
]
```

### GET /api/weather/hour?timestamp={ISO8601}
Returns weather at exact normalized hour.

Response 404:

```json
{ "error": "No weather data for requested hour" }
```

### POST /api/weather/fetch (non-production only)
Manually fetches Open-Meteo and replaces weather cache table content.

Response 200: array of weather rows.

### DELETE /api/weather/cache?olderThanDays=1
Deletes old weather rows older than threshold.

Response 200:

```json
{
  "message": "Cache cleared (older than 1 days). 24 records remain."
}
```

---

## 7) Holidays

### GET /api/holidays
Returns Dutch public holidays for 2026 from Nager.Date (cached in service).

Response 200:

```json
[
  {
    "date": "2026-01-01T00:00:00",
    "localName": "Nieuwjaarsdag",
    "name": "New Year's Day"
  }
]
```

---

## 8) PSV Matches

### GET /api/psvmatches
Returns upcoming/live PSV matches parsed from football-data.org.

Response 200:

```json
[
  {
    "matchDate": "2026-04-12T14:30:00Z",
    "homeAway": "HOME",
    "status": "SCHEDULED",
    "kickOffTime": "2026-04-12T14:30:00Z",
    "opponent": "Ajax"
  }
]
```

---

## 9) Metadata / Transparency

### GET /api/meta
Returns transparency payload used by frontend information panel.

Response 200 fields:
- `modelDescription`
- `moodLabels` (map)
- `dataSourcesUsed` (list)
- `knownLimitations`
- `confidenceScoreExplanation`
- `featureExplanations` (map)

---

## 10) Venue Scraper

### POST /api/scraper/venues (non-production only)
Manual trigger for Uit in Eindhoven scraper.

Response 200:

```json
{
  "success": true,
  "message": "Scraping completed successfully",
  "eventsCount": 120,
  "timestamp": "2026-04-07T14:00:00Z"
}
```

Response 500:

```json
{
  "success": false,
  "message": "Scraping failed: ...",
  "timestamp": "2026-04-07T14:00:00Z"
}
```

---

## Notes

- Swagger is enabled by current Program.cs setup.
- Timestamps are handled as UTC in forecast logic and snapshot lookup.
- Event category exists in query parameter but is not currently applied in events query.
- Legacy `/api/zones` contracts are obsolete and not implemented in current controllers.
