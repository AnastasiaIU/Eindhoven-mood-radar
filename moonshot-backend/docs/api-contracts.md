# MoodRadar API Contracts

**Version:** 1.0 (Phase 1)  
**Environment:** Development (Mock Data)  
**Base URL:** `http://localhost:5000/api` (development)

---

## Overview

This document defines the JSON contracts for all REST API endpoints. During Phase 1, all endpoints return mock data. Phase 2 will replace mock data with live data from the database and external API connectors while maintaining the same JSON contracts.

### Common Response Envelope

All responses follow a standard structure:

```json
{
  "data": {...},
  "timestamp": "2026-03-17T21:45:32.123Z",
  "success": true
}
```

For errors:

```json
{
  "error": "error message",
  "timestamp": "2026-03-17T21:45:32.123Z",
  "success": false
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

## 3. GET /api/events

### Description

Returns all active and upcoming events in Eindhoven from multiple sources (Eventbrite, Ticketmaster, etc). Events are sorted by start time (most recent first).

### Request

```bash
GET /api/events
```

### Query Parameters (Future Use)

| Parameter | Type | Description |
|-----------|------|-------------|
| `zoneId` | integer | (Optional) Filter events by zone ID |
| `category` | string | (Optional) Filter by category (e.g., "Sports", "Music", "Conference") |
| `limit` | integer | (Optional) Maximum number of events to return (default: 100) |

### Response (200 OK)

**Content-Type:** `application/json`

```json
[
  {
    "id": 1,
    "title": "Tech Conference 2026",
    "source": "Eventbrite",
    "startTime": "2026-03-17T23:00:00Z",
    "endTime": "2026-03-18T05:00:00Z",
    "category": "Conference",
    "url": "https://eventbrite.com/e/tech-conference"
  },
  {
    "id": 2,
    "title": "PSV vs AFC Ajax",
    "source": "football-data.org",
    "startTime": "2026-03-18T01:00:00Z",
    "endTime": "2026-03-18T03:00:00Z",
    "category": "Sports",
    "url": "https://psv.nl"
  },
  {
    "id": 3,
    "title": "Live Jazz Night",
    "source": "Ticketmaster",
    "startTime": "2026-03-18T03:00:00Z",
    "endTime": "2026-03-18T07:00:00Z",
    "category": "Music",
    "url": "https://ticketmaster.com/jazz"
  }
]
```

### Field Definitions

| Field | Type | Description |
|-------|------|-------------|
| `id` | integer | Unique event identifier in cache |
| `title` | string | Event name |
| `source` | string | Data source: `"Eventbrite"`, `"Ticketmaster"`, `"football-data.org"`, or other |
| `startTime` | ISO 8601 timestamp | Event start (UTC) |
| `endTime` | ISO 8601 timestamp (nullable) | Event end (UTC), may be null for open-ended events |
| `category` | string | Event type (e.g., "Conference", "Sports", "Music", "Market") |
| `url` | string (nullable) | URL to event details or booking page |

### Response (200 OK - Empty List)

```json
[]
```

Returns empty array if no events match the filter criteria.

### Source Reference

| Source | Data Provider | Update Frequency |
|--------|---------------|-----------------|
| `Eventbrite` | Eventbrite API | Every 30 minutes |
| `Ticketmaster` | Ticketmaster Discovery API | Every 30 minutes |
| `football-data.org` | Football Data API | Daily (for PSV matches) |
| `Local Events` | Manual entry / community | As needed |

### Notes

- Phase 1 returns mock data
- Phase 2 will include zone assignment for each event
- All timestamps are in UTC
- Events are pre-filtered to exclude past events (startTime >= now)
- Results sorted by startTime descending (closest events first)

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
| 1.0 | 2026-03-17 | Initial Phase 1 contracts, mock data |
| 1.1 (planned) | 2026-04-01 | Add pagination, filtering by zone/category |
| 2.0 (planned) | 2026-05-01 | Live data integration, confidence intervals |

---

**Last Updated:** 2026-03-19
**Owner:** Backend Team (Sia & Ivan)
**Status:** Phase 1 Development
