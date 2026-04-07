# PostgreSQL Setup Guide

For local development and testing of MoodRadar backend.

---

## Quick Setup

### Option 1: Local PostgreSQL (Windows)

1. Install PostgreSQL from https://www.postgresql.org/download/windows/
2. During setup, set user password (example: `postgres`)
3. Verify:

```bash
psql --version
```

### Option 2: Docker

```bash
docker run --name mood-radar-db -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=moodradar_dev -p 5432:5432 -d postgres:15
```

Start existing container later:

```bash
docker start mood-radar-db
```

---

## Backend Connection Configuration

Edit [MoodRadar.API/appsettings.Development.json](MoodRadar.API/appsettings.Development.json):

```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5432;Database=moodradar_dev;Username=postgres;Password=postgres"
  }
}
```

Important: the key name must be `PostgreSQL` because `Program.cs` reads `GetConnectionString("PostgreSQL")`.

---

## Start Backend

```bash
cd moonshot-backend/MoodRadar.API
dotnet run --environment Development
```

Development startup behavior:

- Applies migrations.
- Verifies core tables exist.
- If migration chain is inconsistent, performs development fallback schema rebuild.
- Runs database seeding.

---

## Quick API Checks

```bash
# District list
curl http://localhost:5000/api/districts

# Neighborhood list (metadata only)
curl "http://localhost:5000/api/neighborhoods?districtId=1"

# Mood forecast for one neighborhood (next 24h hourly snapshots)
curl http://localhost:5000/api/mood/neighborhood/1

# Exact snapshot by timestamp (ISO-8601)
curl "http://localhost:5000/api/mood/neighborhood/1/snapshot?timestamp=2026-04-07T15:00:00Z"

# Events (next 24h)
curl "http://localhost:5000/api/events?page=0&pageSize=20"

# Weather cache
curl http://localhost:5000/api/weather
```

---

## Non-Production Test Endpoints

The following endpoints are blocked in Production by `NonProductionOnlyAttribute`:

- `POST /api/events/refresh`
- `POST /api/weather/fetch`
- `POST /api/scraper/venues`

In Development, they are available for manual testing.

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Could not connect to PostgreSQL | Ensure local service or Docker container is running |
| Connection string not found | Ensure `ConnectionStrings.PostgreSQL` exists in appsettings/environment |
| `42P01: relation "Districts" does not exist` | Run in Development. Startup has schema fallback rebuild if core tables are missing |
| Migrations failed on local dev DB | Stop app, drop dev DB, rerun app in Development to recreate schema + seed |

---

## Related Docs

- [docs/api-contracts.md](docs/api-contracts.md)
- [docs/ticketmaster_api_audit.md](docs/ticketmaster_api_audit.md)
- [MoodRadar.API/README.md](MoodRadar.API/README.md)
