# MoodRadar Backend API

Backend API for Eindhoven Mood Radar, built with ASP.NET Core 8 and PostgreSQL.

## Current Status

This codebase is no longer on the old zones-only mock API.
It currently exposes district, quarter, neighborhood, mood forecast, events, weather, holidays, PSV matches, metadata, and scraper endpoints.

- Framework: .NET 8 (ASP.NET Core)
- ORM: Entity Framework Core + Npgsql
- Database: PostgreSQL
- Scheduled pipeline: every 15 minutes via hosted service

## Prerequisites

- .NET 8 SDK
- PostgreSQL (local install or Docker)

## Local Configuration

Update connection settings in [appsettings.Development.json](appsettings.Development.json):

```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5432;Database=moodradar_dev;Username=postgres;Password=postgres"
  }
}
```

Optional API keys:

- `Ticketmaster:ApiKey`
- `FootballApi:ApiKey`

## Run Locally

```bash
cd moonshot-backend/MoodRadar.API
dotnet restore
dotnet build
dotnet run --environment Development
```

On startup in Development:

- Migrations are applied.
- Core table existence is validated.
- If migration chain is broken, development fallback rebuilds schema.
- Database seeding runs.

## API Endpoint Overview

### Geography

- `GET /api/districts`
- `GET /api/districts/{id}`
- `GET /api/quarters?districtId={id}`
- `GET /api/quarters/{id}`
- `GET /api/neighborhoods?quarterId={id}&districtId={id}`
- `GET /api/neighborhoods/{id}`

### Mood Forecast (NeighborhoodSnapshot-based)

- `GET /api/mood/neighborhood/{neighborhoodId}`
  - Returns upcoming hourly snapshots for next 24h.
- `GET /api/mood/all`
  - Returns upcoming hourly snapshots for next 24h for all neighborhoods.
- `GET /api/mood/neighborhood/{neighborhoodId}/snapshot?timestamp={ISO8601}`
  - Returns exact snapshot at timestamp.

### Events

- `GET /api/events?page=0&pageSize=20&neighborhoodId={id}`
- `GET /api/events/{id}`
- `POST /api/events/refresh` (non-production only)

Notes:

- Events endpoint returns next 24h window.
- `category` query parameter is accepted by code but currently not applied in query filtering.
- Event DTO currently does not expose a category field.

### Weather

- `GET /api/weather`
- `GET /api/weather/hour?timestamp={ISO8601}`
- `POST /api/weather/fetch` (non-production only)
- `DELETE /api/weather/cache?olderThanDays=1`

### Other

- `GET /api/holidays`
- `GET /api/psvmatches`
- `GET /api/meta`
- `POST /api/scraper/venues` (non-production only)

## Non-Production Endpoints

The following routes are blocked in Production by `NonProductionOnlyAttribute` and return `403`:

- `POST /api/events/refresh`
- `POST /api/weather/fetch`
- `POST /api/scraper/venues`

## Background Pipeline

Hosted service: `MoodUpdateService`

- Runs immediately on startup, then every 15 minutes.
- Polls Ticketmaster.
- Fetches weather.
- Fetches PSV matches.
- Fetches holidays.
- Generates 24-hour hourly neighborhood mood snapshots.
- Venue scraping runs once per 24 hours, but is disabled in Development.

## Web Scraping

Manual endpoint:

- `POST /api/scraper/venues`

Current scraper source:

- Uit in Eindhoven agenda (`uitineindhoven.nl/agenda`)

The scraper stores events in `Events` and attempts coordinate and neighborhood mapping when detail-page data is available.

## CORS

CORS policy is configured in `Program.cs`.
In Development it allows:

- `http://localhost:3000`

## Where To Find Full Contracts

For request and response examples, see:

- [../docs/api-contracts.md](../docs/api-contracts.md)
- [../docs/ticketmaster_api_audit.md](../docs/ticketmaster_api_audit.md)
- [../POSTGRESQL_SETUP.md](../POSTGRESQL_SETUP.md)
