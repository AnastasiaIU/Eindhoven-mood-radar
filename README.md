# Eindhoven Mood Radar

A real-time web application that aggregates live public event data and displays AI-generated mood labels for different Eindhoven city zones, with full transparency and confidence scoring.

**Project**: Eindhoven Mood Radar | **Team**: Moonshot (7 Fontys ICT students) | **Duration**: 14 weeks (March – June 2026)

---

## What is it?

The Mood Radar shows you the "vibe" of different areas in Eindhoven in real-time:

| Mood | Colour | Typical Trigger |
|------|--------|-----------------|
| 🟨 **Energetic** | Pastel Yellow | Nightlife + weekend + warm evening |
| 🟧 **Intense** | Pastel Orange | PSV home match or large concert |
| 🟥 **Busy** | Pastel Coral | Market + many events + daytime |
| 🔵 **Relaxed** | Pastel Sky | Few events, mild weather, evening |
| 🟦 **Calm** | Pastel Teal | No events, early morning / late night |

Every mood label includes:
- **Confidence score** (0-100%)
- **Plain-language explanation** of what drove it
- **Data sources** and known limitations
- **Transparency Panel** explaining how moods are calculated

---

## Core Principles

✅ **Transparency First** – Model reasoning always visible to users  
✅ **No Personal Data** – Only public APIs (events, weather, sports)  
✅ **Honest About Bias** – Central-Eindhoven coverage bias documented  
✅ **GDPR Compliant** – By design; EU AI Act Article 50 (limited-risk AI)  
❌ **Not for**: Policing, crowd control, insurance pricing, surveillance

---

## Project Structure

```
Eindhoven-mood-radar/
├── docs/                           # Research & specifications
│   ├── api-contracts.md           # Data shape contracts
│   ├── ticketmaster_api_audit.md  # API findings & rate limits
│   ├── boundary_coordinate_generation.md
│   └── WEB_SCRAPING_LEGAL_RESEARCH.md
│
├── moonshot-backend/              # C# ASP.NET Core 8.0 API
│   ├── MoodRadar.API/
│   │   ├── Controllers/           # REST endpoints
│   │   ├── Services/              # Business logic & API connectors
│   │   ├── Models/                # Domain models & DTOs
│   │   └── Program.cs             # Dependency injection & middleware
│   ├── Data/                      # EF Core DbContext & migrations
│   ├── Migrations/                # Database schema versions
│   ├── MoodRadar.sln
│   └── README.md                  # Backend setup instructions
│
└── moonshot-app/                  # React Native + Expo (TypeScript)
    ├── app/                       # Expo Router pages
    │   ├── index.tsx              # Map view
    │   └── _layout.tsx            # Root layout
    ├── components/                # Reusable UI components
    ├── assets/                    # Images & static files
    ├── package.json
    └── README.md                  # Frontend setup instructions
```

---

## Tech Stack

### Backend
- **Language**: C# 12.0
- **Framework**: ASP.NET Core 8.0 LTS
- **Database**: PostgreSQL (Render.com free tier)
- **ORM**: Entity Framework Core
- **CI/CD**: GitHub Actions → Render.com auto-deploy
- **Code Quality**: ESLint, Prettier

### Frontend
- **Framework**: React Native + Expo
- **Language**: TypeScript
- **Routing**: Expo Router
- **Map**: Leaflet.js + OpenStreetMap
- **Styling**: Tailwind CSS (react-native compatible)

### Data Sources (Phase 1)
| Source | API | Rate Limit | Status |
|--------|-----|-----------|--------|
| Events | Ticketmaster Discovery | 5,000/day | Active |
| Local Events | Uit in Eindhoven (scraping) | 1/day | Active ⭐ |
| PSV Matches | football-data.org | 10 req/min | Active |
| Weather | Open-Meteo | Unlimited | Active |
| Holidays | Nager.Date | Unlimited | Active |

---

## Phase 1 Deliverables (March 15 – May 3, 2026)

### ✅ Backend
- [x] PostgreSQL schema (zones, events_cache, zone_snapshots, weather, holidays)
- [x] Ticketmaster API connector (geolocation search, rate-limit logging)
- [x] PSV match status connector (football-data.org)
- [x] Open-Meteo weather polling (hourly data)
- [x] Uit in Eindhoven web scraper (legal research completed)
- [x] REST API endpoints: `/api/zones`, `/api/zones/:id/mood`, `/api/events`
- [x] Background service (15-min polling interval)
- [x] CI/CD pipeline (GitHub Actions → Render.com auto-deploy)
- [x] Docker configuration

### ✅ Frontend
- [x] React Native + Expo scaffold with TypeScript
- [x] Expo Router setup (4 screens: Map, Event Feed, Area Detail, Transparency)
- [x] Component stubs: MapView, EventFeed, AreaDetailPanel, TransparencyPanel
- [x] Design system: 5 mood colours, typography, reusable components
- [x] Figma wireframes (all 4 screens)
- [x] api-contracts.md alignment (data shapes documented)

### ✅ AI/ML Research
- [x] ML Research Document (dataset design, label rules, model recommendation, coverage gaps)
- [x] Feature vector schema (zones × time-slot snapshots)
- [x] Synthetic labelling baseline rules
- [x] Model family evaluation (scikit-learn: Random Forest vs. XGBoost/LightGBM)
- [x] Coverage gap analysis & known biases

### 📚 Documentation
- [x] Ticketmaster API audit (response shapes, pagination, geolocation search)
- [x] Web scraping legal research (Uit in Eindhoven, GDPR compliance)
- [x] Zone boundary coordinate generation (GeoJSON polygons)
- [x] API contracts specification (JSON schemas)

---

## Quick Start

### Prerequisites
- **Backend**: .NET 8.0 SDK, PostgreSQL
- **Frontend**: Node.js 18+, npm or pnpm, Expo Go (mobile testing)

### Backend Setup
```bash
cd moonshot-backend/MoodRadar.API
dotnet build
dotnet run --environment Development
# Runs on http://localhost:5000 (HTTP) or https://localhost:5001 (HTTPS)
```
See [moonshot-backend/README.md](moonshot-backend/README.md) for full setup.

### Frontend Setup
```bash
cd moonshot-app
npm install  # or pnpm install
npm run dev  # Starts Expo dev server
# Scan QR code with Expo Go app on mobile, or use emulator
```
See [moonshot-app/README.md](moonshot-app/README.md) for full setup.

---

## Team Structure

| Direction | Role | Focus | Deliverable |
|-----------|------|-------|-------------|
| **Backend** | 2 IT members | API, database, CI/CD | REST API + PostgreSQL schema |
| **Frontend** | 2 IT leads + 2 non-IT | UI/UX, design system, components | React Native app + Figma wireframes |
| **AI/ML** | 1 specialist | Dataset, labels, model selection | ML Research Document + feature engineering |

---

## Data & Ethics

### Data Sources (Public APIs Only)
- ✅ Ticketmaster Discovery API (event listings)
- ✅ Uit in Eindhoven public agenda (local events, legally scraped)
- ✅ football-data.org (PSV matches)
- ✅ Open-Meteo (weather)
- ✅ Nager.Date (public holidays)

### Known Limitations
- **Central-Eindhoven bias**: Ticketmaster API covers commercial venues; quieter zones default to "Calm"
- **Sparse Netherlands coverage**: Free tier has ~5 Eindhoven events per 100 days from Ticketmaster
- **Mitigated by**: Uit in Eindhoven scraping (~100+ local events/month) + transparency disclosure

### Compliance
- **GDPR**: No personal data collected or stored
- **EU AI Act**: Article 50 (limited-risk AI); transparency built-in
- **Non-intended uses**: Explicitly prohibited for policing, crowd control, insurance pricing

---

## Documentation

- **[api-contracts.md](docs/api-contracts.md)** – REST API data shapes
- **[ticketmaster_api_audit.md](moonshot-backend/docs/ticketmaster_api_audit.md)** – API findings & rate limits
- **[WEB_SCRAPING_LEGAL_RESEARCH.md](moonshot-backend/docs/WEB_SCRAPING_LEGAL_RESEARCH.md)** – Scraping legality & compliance
- **[boundary_coordinate_generation.md](moonshot-backend/docs/boundary_coordinate_generation.md)** – Zone GeoJSON polygons
- **[TICKETMASTER_SETUP.md](moonshot-backend/TICKETMASTER_SETUP.md)** – API key registration
- **[POSTGRESQL_SETUP.md](moonshot-backend/POSTGRESQL_SETUP.md)** – Database setup on Render.com

---

## Deployment

- **Backend**: Render.com (auto-deploy on git push to `main`)
- **Database**: PostgreSQL on Render.com (free tier)
- **Frontend**: Expo EAS Build (Phase 2+)

See [DEPLOYMENT.md](DEPLOYMENT.md) for detailed instructions.

---

## Execution Rules

⚠️ **Always stop background processes at end of work**:
- Kill all running terminals (backend server, scraper, etc.)
- Verify no processes remain (prevent port conflicts)
- Document the action before handing off

⚠️ **Never commit automatically**:
- Provide code changes and Git commands only
- User makes final `git commit` decision

---

## Contact & Questions

- **AI Specialist**: ML research, model strategy
- **Frontend IT Leads**: Env setup, Figma, components
- **Backend Team**: API schema, migrations, CI/CD
- **Weekly Standup**: Cross-direction coordination

---

**Status**: Phase 1 Complete | **Phase 2**: Model training, frontend refinement, expanded data sources (June – Aug 2026)

*Last Updated: June 8, 2026*
