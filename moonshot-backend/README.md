# MoodRadar Backend - moonshot-backend

C# ASP.NET Core backend for the Eindhoven Mood Radar project.

## Quick Start

```bash
# Build the solution
dotnet build MoodRadar.sln

# Run the API
cd MoodRadar.API
dotnet run --environment Development
```

Server starts on `http://localhost:5000` (or assigned port)

## Project Structure

```bash
moonshot-backend/
├── MoodRadar.sln                 # Solution file
├── MoodRadar.API/                # API project
│   ├── Controllers/              # API endpoints
│   │   ├── ZonesController.cs    # GET /api/zones, /api/zones/:id/mood
│   │   ├── EventsController.cs   # GET /api/events
│   │   └── WeatherController.cs
│   ├── Models/                   # Data entities
│   │   ├── Zone.cs
│   │   ├── ZoneSnapshot.cs
│   │   ├── Event.cs
│   │   └── Weather.cs    # Weather forecast (Phase 1 mock, Phase
│   ├── Services/                 # Business logic
│   │   └── MockDataService.cs    # Phase 1: Mock data provider
│   ├── Program.cs                # Application setup, DI, middleware
│   ├── MoodRadar.API.csproj
│   └── README.md
├── docs/
│   └── api-contracts.md          # Complete API specification
└── README.md                     # This file
```

## Documentation

- **[API Contracts](docs/api-contracts.md)** - Full endpoint specification for frontend integration
- **[Backend README](MoodRadar.API/README.md)** - Setup, configuration, and development guide

## Deployment

See [DEPLOYMENT.md](../DEPLOYMENT.md) for:

- Local development setup
- Render.com deployment guide
- GitHub Actions CI/CD pipeline
- Environment configuration

Quick start for production:

```bash
# Build Docker image
docker build -f moonshot-backend/Dockerfile -t mood-radar-api .

# Run locally
docker run -p 5000:5000 mood-radar-api
```

## API Endpoints

| Method | URL | Description |
|--------|-----|-------------|
| GET | `/api/zones` | All Eindhoven zones |
| GET | `/api/zones/{id}/mood` | Zone mood prediction |
| GET | `/api/events` | All upcoming events |

See [API Contracts](docs/api-contracts.md) for detailed specifications.
