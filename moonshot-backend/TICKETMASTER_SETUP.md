# Ticketmaster Discovery API Integration Setup Guide

## Quick Start

### 1. Get Your Ticketmaster API Key

1. Go to https://developer.ticketmaster.com/
2. Sign in or create a developer account (completely FREE, no credit card required)
3. Create a new app to receive your API key
4. API key is immediately usable - no additional authentication steps needed
5. Store it securely (do NOT commit to git)

### 2. Configure the Backend

#### Local Development

**DO NOT** commit API keys to `appsettings.json`. Instead:

1. Copy `.env.example` to `.env` (in `moonshot-backend/` directory):

   ```bash
   cp .env.example .env
   ```

2. Edit `.env` and add your API key:

   ```
   TICKETMASTER__APIKEY=your_ticketmaster_api_key_here
   ```

3. **Important**: `.env` is in `.gitignore` and will never be committed. This is the secure way to store secrets locally.

When the app starts, it reads `.env` and sets environment variables automatically. The ASP.NET Core configuration hierarchy applies:

- `appsettings.json` (defaults, committed to git)
- `appsettings.Development.json` (dev overrides, not committed to git)
- Environment variables from `.env` (secrets, NOT committed) ← **highest priority**

#### Production (Render.com)

Add environment variable in Render dashboard:

```
TICKETMASTER__APIKEY=your_production_api_key
```

The double underscore (`__`) maps to nested JSON structure in ASP.NET Core.

### 3. Build and Run

```bash
cd moonshot-backend/MoodRadar.API
dotnet build
dotnet run
```

### 4. Test the Integration

#### Access Locally

The backend runs at:
- Development: `http://localhost:5000` or `https://localhost:5001`
- Check logs for service startup messages

---

## API Details

### Rate Limits (Free Tier)

- **5,000 API calls per day**
- **5 requests per second**
- Ideal for 15-minute polling frequency used in Phase 1

### Coverage (Current Limitations)

⚠️ **IMPORTANT**: Discovery API has **poor coverage for Eindhoven** and tier-2 Dutch cities.

- **230K+ events** globally, but concentrated in major markets (US, UK, major EU cities)
- **Netherlands technically supported** (Market 211), but coverage is sparse
- **Eindhoven: Only 2 events returned** for 10-day window (March 27 - April 5, 2026)
- **Comparison**: ticketmaster.nl website shows ~10 Eindhoven events for same period, but API returns only 2
- **Root cause**: Discovery API only syndicates from specific partners (Ticketmaster, TicketWeb, Universe, FrontGate, Ticketmaster Sport, MoshTix) — many local Dutch promoters not included

#### API Data Source Limitation

The Discovery API **does not represent all Eindhoven events**. It mirrors only events from syndicated partners. Local venues, independent promoters, and smaller events are missing.

#### Recommended Mitigation (Phase 2+)

- **Add secondary data sources**: Local Dutch ticketing platforms (e.g., Eventim.nl, local promoters)
- **Supplement with sport-specific integrations**: PSV matches via football-data.org (already planned)
- **Document data quality in UI**: Transparency Panel must disclose "Event data sourced from Ticketmaster Discovery API; coverage bias toward larger venues"

### What Gets Returned

- Event name, date/time, venue location (latitude/longitude)
- Event categories (music, sports, arts, etc.)
- Ticket URLs and price ranges
- Event images and descriptions
- Multiple event sources (Ticketmaster, Universe, FrontGate, TMR)

---

## Monitoring & Logging

### Log Files

The service logs to console. In production (Render), view logs via:

- **Render Dashboard** → Service → Logs
- **Real-time**: `render logs --service moonshot-backend --follow`

### Rate Limit Monitoring

Watch for lines like:

```
DEBUG Ticketmaster RateLimit: 4950/5000 remaining, resets in 3600s
```

If you hit 429 (Too Many Requests), the service automatically backs off for 30 seconds before retrying.

### Common Issues

**Q**: "Ticketmaster API key loaded: NULL"  
**A**: Ensure `.env` exists in `moonshot-backend/` with `TICKETMASTER__APIKEY=...`

**Q**: "HTTP error during Ticketmaster polling" or 404 errors  
**A**: Check API key is valid and formatted correctly

If you see `429 Too Many Requests`, the service will automatically back off.

---

## API Endpoints

### Ticketmaster Controller

See [api-contracts.md](docs/api-contracts.md) for the full endpoint contract and response schema.

#### Quick Test

```bash
POST /api/events/refresh
```

Returns cached events with source, category, and coordinates.

---

## Troubleshooting

### Issue: `401 Unauthorized`

**Cause**: Invalid or missing API token  
**Solution**:

1. Verify token in `.env` (format: `TICKETMASTER__APIKEY=...`)
2. Check token is still valid on Ticketmaster Developer Dashboard
3. Regenerate token if needed

### Issue: `no events found`

**Cause**: Ticketmaster Discovery API has poor coverage for tier-2 Dutch cities. It only syndicates from specific ticketing partners (Ticketmaster, TicketWeb, Universe, FrontGate, Ticketmaster Sport, MoshTix).  

**Verified**: Website shows ~10 events within 10 days; API returns ~2. This is not a code bug—it's an API data limitation.

**Solutions**:

1. **Phase 1 (Now)**: Accept limited data as MVP validation. Document in Transparency Panel.
2. **Phase 2**: Add supplementary data sources:
   - Local Dutch platforms (Eventim.nl, ThreeTickets, De Ticketshop)
   - Direct venue/promoter APIs
   - PSV match data (football-data.org) already planned
3. **Alternative**: Upgrade to Ticketmaster Discovery Feed (paid tier, more comprehensive, no call limits)

### Issue: Very slow polling

**Cause**: Rate limiting or network latency  
**Solution**:

1. Check logs for rate-limit warnings (429 responses trigger 30s backoff)
2. Current implementation makes 1 API call per poll (city-based search)

### Issue: `Unable to parse rate limit headers`

**Cause**: Ticketmaster API response format changed or unexpected response  
**Solution**:

1. Check Ticketmaster API status page: https://status.ticketmaster.com/
2. Log full response: Add debug logging in rate-limit parsing code

---

## Data Flow

```
TicketmasterService.PollEindhovenEventsAsync()
  ↓
FetchPageAsync(page) → GET /discovery/v2/events.json
  ├─ city=Eindhoven
  └─ startDateTime/endDateTime (24-hour window)
  ↓
  ├─ Parse JSON response → TicketmasterModels
  ├─ Convert venue coordinates: string → double
  ├─ Log rate-limit headers
  └─ Return TicketmasterEvent[]
  ↓
EventsController.RefreshEventsAsync()
  ↓
In-memory cache updated (singleton, persists across requests)
  ↓
GET /api/events → Return paginated cached events
  ↓
Zone mapping: Geolocation → zone_id (Phase 2)
  ↓
Mood generation: Events per zone → mood label (Phase 2 ML)

⚠️ **Coverage Note**: API returns ~5 events for Eindhoven (100-day window)
   This is a known limitation; supplementary sources needed for production
```

---

### Known Data Limitations

- Discovery API coverage for Eindhoven is sparse (~5 events for 100-day window)
- This is **not a bug**—it's a documented limitation of the API tier for tier-2 cities
- Must disclose in Transparency Panel and project documentation
- Phase 2 should evaluate adding secondary data sources for more comprehensive coverage

---

## References

- **Service Code**: `Services/TicketmasterService.cs`
- **Controller**: `Controllers/EventsController.cs`
- **Models**: `Models/Ticketmaster.cs`, `Models/Event.cs`
- **Configuration**: `appsettings.json`, `appsettings.Development.json`
- **API Documentation**: `docs/api-contracts.md`
