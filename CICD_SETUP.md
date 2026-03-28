# Eindhoven Mood Radar - CI/CD & Deployment Setup

## Overview

This repository includes:

- ✅ **GitHub Actions CI/CD pipeline** (lint → test → build)
- ✅ **Render.com auto-deployment** from `main` branch
- ✅ **Docker support** for containerized deployment
- ✅ **Complete deployment documentation**

## Quick Start

### Development

#### Backend (C# ASP.NET Core)

```bash
cd moonshot-backend/MoodRadar.API
dotnet restore
dotnet build
dotnet run --environment Development
# Runs on http://localhost:5000
```

#### Frontend (Next.js)

```bash
cd moonshot-webapp
npm install
npm run dev
# Runs on http://localhost:3000
```

### Deployment

1. **Push to `main` branch** triggers GitHub Actions
2. **GitHub Actions** runs: lint → build → test
3. **Render.com** auto-deploys on successful CI/CD

For detailed setup: See [DEPLOYMENT.md](./DEPLOYMENT.md)

## Project Structure

```
Eindhoven-mood-radar/
├── .github/
│   └── workflows/
│       └── ci-cd.yml                 # GitHub Actions pipeline
├── moonshot-backend/
│   ├── Dockerfile                    # Container image (Render deployment)
│   ├── MoodRadar.sln
│   ├── MoodRadar.API/
│   │   ├── Controllers/
│   │   ├── Models/
│   │   ├── Services/
│   │   └── Program.cs
│   ├── TICKETMASTER_SETUP.md
│   ├── WEATHER_IMPLEMENTATION.md
│   ├── docs/api-contracts.md
│   └── README.md
├── moonshot-webapp/
│   ├── app/
│   ├── components/
│   ├── public/
│   ├── package.json
│   ├── next.config.ts
│   └── README.md
├── DEPLOYMENT.md                     # Full deployment guide
├── docs/
│   ├── api-contracts.md
│   ├── api-eventbrite-signal-audit.md
│   └── project-driven-creation-PMC.md
└── README.md (this file)
```

## CI/CD Pipeline

### Triggers

- Every PR to `main` or `develop`
- Every push to `main`

### Jobs (Run in Parallel)

**Frontend** (`npm run lint` → `npm run build`):

- Node.js 20
- ESLint verification
- Next.js build

**Backend** (`dotnet restore` → `dotnet build` → `dotnet test`):

- .NET 6.0
- Solution build
- Unit tests (if configured)

[View workflow file](./.github/workflows/ci-cd.yml)

## Deployment Architecture

```
GitHub Push to main
       │
       ▼
GitHub Actions (CI/CD)
  ├─ Frontend: lint → build
  └─ Backend: build → test
       │
       ▼ (on success)
Render.com Auto-Deploy
  ├─ Backend API → eindhoven-mood-radar-api.render.com
  └─ Frontend App → eindhoven-mood-radar-web.onrender.com
```

## Environment Variables

### Backend (Render)

```
ASPNETCORE_ENVIRONMENT=Production
TICKETMASTER_API_KEY=<your-api-key>
DATABASE_URL=<postgresql-connection-string>
```

### Frontend (Optional)

```
NEXT_PUBLIC_API_URL=https://eindhoven-mood-radar-api.render.com
```

## Troubleshooting

**CI/CD fails locally?**

```bash
# Frontend
cd moonshot-webapp && npm ci && npm run lint && npm run build

# Backend
cd moonshot-backend && dotnet build && dotnet test
```

**Render deployment fails?**
→ Check service logs in Render dashboard for errors

**All else fails?**
→ See [DEPLOYMENT.md](./DEPLOYMENT.md) full troubleshooting section

## Links

- 📖 [Full Deployment Guide](./DEPLOYMENT.md)
- 📋 [API Contracts](./moonshot-backend/docs/api-contracts.md)
- 🔗 [GitHub Actions Docs](https://docs.github.com/en/actions)
- 🚀 [Render.com Docs](https://render.com/docs)
- 🐳 [Docker Docs](https://docs.docker.com/)
