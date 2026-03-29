# PostgreSQL Setup Guide

**For:** Local development and testing  

---

## Quick Setup

### Option 1: Local PostgreSQL Installation (Recommended for Windows)

1. **Download & Install**
   - Visit https://www.postgresql.org/download/windows/
   - Run the installer, set password to `postgres`
   - Accept all defaults

2. **Verify Installation**

   ```bash
   psql --version
   ```

### Option 2: Docker (Linux/Mac friendly)

1. **Run once**

   ```bash
   docker run --name mood-radar-db \
     -e POSTGRES_PASSWORD=postgres \
     -e POSTGRES_DB=moodradar_dev \
     -p 5432:5432 \
     -d postgres:15
   ```

2. **Future starts**

   ```bash
   docker start mood-radar-db
   ```

---

## Database Configuration

Edit [appsettings.Development.json](MoodRadar.API/appsettings.Development.json):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=moodradar_dev;Username=postgres;Password=postgres"
  }
}
```

**Connection Details:**

- Host: `localhost`
- Port: `5432`
- Database: `moodradar_dev`
- User: `postgres`
- Password: `postgres`

---

## Starting the Backend

```bash
cd moonshot-backend/MoodRadar.API
dotnet run --environment Development
```

**Expected Output:**
```
Applying database migrations...
✓ Migrations applied successfully
Running database seeder...
✓ Seeded 7 districts
✓ Seeded 19 quarters
✓ Seeded 110+ neighborhoods
✓ Seeded 110+ neighborhood snapshots with mood predictions
✓ Database seeding completed successfully!
...
Now listening on: http://localhost:5000
```

---

## Testing an Endpoint

```bash
# Get all neighborhoods with current mood
curl http://localhost:5000/api/neighborhoods | python -m json.tool
```

**Expected Response:**
```json
[
  {
    "id": 1,
    "name": "Binnenstad",
    "currentMood": "Busy",
    "confidence": 0.76,
    "lastMoodUpdate": "2026-03-29T18:33:57.253288Z"
  },
  ...
]
```

---

## More Endpoints

Refer to [api-contracts.md](docs/api-contracts.md) for complete API documentation.

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| "Could not connect to server" | Ensure PostgreSQL/Docker is running |
| "Database moodradar_dev does not exist" | App auto-creates it on first run |
| "Migrations failed" | Run `dotnet ef database drop`, then restart app |

---

**Questions?** Check [api-contracts.md](docs/api-contracts.md) or ask the backend team.
