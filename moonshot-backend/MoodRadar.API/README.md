# MoodRadar Backend API

Phase 1 REST API for the Eindhoven Mood Radar project. Built with C# ASP.NET Core.

## Overview

This is the backend API server that provides:

- Zone (district) data for Eindhoven
- Real-time mood predictions for each zone
- Event data from multiple sources (Eventbrite, Ticketmaster, PSV, etc.)

**Status:** Phase 1 Development (Mock Data)  
**Framework:** C# with ASP.NET Core 8.0 (LTS)  
**Database:** PostgreSQL

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later ([download](https://dotnet.microsoft.com/download))
- Git

### Installation

1. Clone or navigate to the project:

    ```bash
    cd \moonshot-backend\MoodRadar.API
    ```

2. Restore dependencies:

    ```bash
    dotnet restore
    ```

3. Build the project:

    ```bash
    dotnet build
    ```

### Running Locally

**Development Mode:**

```bash
dotnet run
```

The API will start at:

- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001` (requires dev certificate; auto-generated on first run)

## API Endpoints

All endpoints return mock data in Phase 1.

### 1. Get All Zones

```bash
GET /api/zones
```

Returns list of all Eindhoven zones with boundaries.

**Example Response:**

```json
[
  {
    "id": 1,
    "name": "Centrum",
    "geoJsonBoundary": "{...}",
    "createdAt": "2026-02-15T10:00:00Z"
  }
]
```

### 2. Get Zone Mood

```bash
GET /api/zones/{id}/mood
```

Returns current mood prediction for a zone.

**Example Response:**

```json
{
  "zoneId": 1,
  "zoneName": "Centrum",
  "moodLabel": "Energetic",
  "confidence": 0.85,
  "timestamp": "2026-03-17T21:45:00Z"
}
```

**Mood Labels:** Energetic, Intense, Busy, Relaxed, Calm

### 3. Get All Events

```bash
GET /api/events
```

Returns all upcoming events across Eindhoven.

**Example Response:**

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
  }
]
```

See [`/docs/api-contracts.md`](../docs/api-contracts.md) for complete API documentation.

## Project Structure

```bash
MoodRadar.API/
├── Controllers/
│   ├── ZonesController.cs          # GET /api/zones, GET /api/zones/:id/mood
│   ├── EventsController.cs         # GET /api/events
│   └── WeatherController.cs # TODO: Replace with Open-Meteo connector
├── Models/
│   ├── Zone.cs                 # Zone entity
│   ├── ZoneSnapshot.cs         # Mood prediction snapshot
│   ├── Event.cs                # Event entity
│   └── Weather.cs              # Weather forecast (Phase 1 mock, Phase 2: Open-Meteo)
├── Services/
│   └── MockDataService.cs      # Phase 1: Returns mock data
├── Program.cs                  # Application setup
├── appsettings.json            # Configuration
└── MoodRadar.API.csproj        # Project file
```

### CORS

Frontend origins allowed (configured in `Program.cs`):

- `http://localhost:3000` (Next.js dev)

## Development Notes

### Phase 1 (Current)

- Returns hardcoded mock data
- All endpoints functional with placeholder responses
- Focus: API contract stability for frontend development

### Phase 2 (Next)

- Replace `IMockDataService` with `IDataService` backed by PostgreSQL
- Integrate external API connectors:
  - Eventbrite API
  - Ticketmaster Discovery API
  - football-data.org (PSV matches)
  - Open-Meteo (weather)
  - Nager.Date (Dutch holidays)
- Integrate ML service for real mood predictions
- Implement cron job for 15-minute refresh

## Testing

For now, test endpoints with cURL or Postman:

1. Start the server: `dotnet run`
2. Try endpoints using cURL:

    ```bash
    # Get all zones
    curl http://localhost:5000/api/zones

    # Get mood for zone 1
    curl http://localhost:5000/api/zones/1/mood

    # Get all events
    curl http://localhost:5000/api/events
    ```

## Deployment

### Render.com (Phase 2)

1. Connect GitHub repository
2. Configure build command:

    ```bash
    dotnet build --configuration Release
    ```

3. Configure start command:

    ```bash
    dotnet MoodRadar.API.dll
    ```

Environment variables:

- `ASPNETCORE_ENVIRONMENT=Production` - Disables development features (like Swagger), enables proper error handling and logging
- `ConnectionString=` (PostgreSQL, Phase 2)
- API keys for external services (Phase 2)

## Logging

Logs are configured in `appsettings.json`. By default, Information level logs are shown.

Controllers log:

- Request entry points (e.g., "Fetching all zones")
- Not found conditions (404)

## Technical Decisions

1. **Mock Data Service Pattern:** Allows for easy replacement with real DB queries in Phase 2
2. **Dependency Injection:** Used for services, enabling testability
3. **CORS Enabled:** Frontend can develop independently

## Links

- [API Contracts](/docs/api-contracts.md) - Complete endpoint specification

## Support

For issues or questions:

1. Check the API Contracts documentation
2. Test endpoints with cURL or Postman
3. Contact backend team

---

**Version:** 1.0 (Phase 1)  
**Last Updated:** 2026-03-19
