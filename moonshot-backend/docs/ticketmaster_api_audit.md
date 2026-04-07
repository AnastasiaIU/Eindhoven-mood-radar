# Ticketmaster Discovery API Audit (Current Backend Implementation)

Updated from source code in `MoodRadar.API/Services/TicketmasterService.cs` and related models.

---

## 1. Integration Summary

The backend integrates with Ticketmaster Discovery API v2 using city-based polling for Eindhoven.

- Base URL: `https://app.ticketmaster.com/discovery/v2/`
- Endpoint used: `GET events.json`
- Auth: API key in query string (`apikey`)
- Time window: now to now + 24 hours (UTC)
- Paging: up to 20 pages, size 50 per page (max 1000 records per poll)

Configured through:

- `Ticketmaster:ApiKey` (configuration)
- `TICKETMASTER__APIKEY` (environment variable override)

---

## 2. Request Shape Used by Code

Each page request is built with these query params:

- `apikey` = configured key
- `city` = `Eindhoven`
- `size` = `50`
- `page` = `0..19`
- `startDateTime` = current UTC time (`yyyy-MM-ddTHH:mm:ssZ`)
- `endDateTime` = current UTC + 24h (`yyyy-MM-ddTHH:mm:ssZ`)
- `includeTBA` = `yes`
- `includeTBD` = `yes`

Implementation notes:

- Search is city-based, not radius/geopoint-based.
- Country filter (`countryCode`) is not added in current code.

---

## 3. Reliability Behavior

The poller has built-in resilience:

- Retry policy: exponential backoff, max 3 retries per page.
- Rate limiting: 300ms delay between page calls.
- 429 handling: explicit 30-second delay before rethrow.
- Rate-limit headers are parsed/logged when present:
  - `X-Rate-Limit`
  - `X-Rate-Limit-Remaining`
  - `X-Rate-Limit-Reset`

Failure fallback:

- If polling fails or is cancelled, service loads cached Ticketmaster events from database for the same 24h window.
- Fallback conversion is lossy and only reconstructs core fields.

---

## 4. Persistence Mapping (What Is Stored)

When events are fetched, they are mapped to `Domain.Event` as follows:

- `ExternalId` <- Ticketmaster `id`
- `Source` <- constant `"Ticketmaster"`
- `Title` <- Ticketmaster `name`
- `StartTime` <- `dates.start.dateTime` (fallback `DateTime.UtcNow` if missing)
- `EndTime` <- `dates.end.dateTime`
- `CachedAt` <- `DateTime.UtcNow`

Current implementation does **not** map or persist Ticketmaster fields below in `TicketmasterService`:

- `url`
- `description`
- `latitude` / `longitude`
- `classifications` (segment/genre)
- `priceRanges`
- `images`
- venue metadata

This means these fields remain `null` in API DTOs unless populated by another source/process.

---

## 5. Database Refresh Strategy

During a successful poll with at least one result:

1. Remove old records where `StartTime < now`.
2. Insert newly fetched mapped events.
3. Save changes.

If no new events are returned:

- Existing records are kept.
- Log warning indicates stale data may persist.

Important behavior:

- No de-duplication/upsert by `ExternalId` in current service code.
- There is no explicit transaction wrapper in this method.

---

## 6. API Surface Impact

`GET /api/events` reads from database and returns paginated events in next 24h.

- Query param `category` exists but is currently **not applied** in controller query.
- `source` is returned as stored in the database (`Ticketmaster` in current Ticketmaster mapping).

Observed response shape from controller DTO:

```json
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
```

---

## 7. Coverage Statement

From implementation alone, coverage quantity/quality cannot be guaranteed because it depends on external Ticketmaster catalog availability for Eindhoven.

What the code guarantees:

- It requests only Eindhoven city events.
- It fetches only the next 24h window.
- It supports pagination up to 1000 records per run.

What must be measured operationally (not inferable from code only):

- Actual event counts returned by Ticketmaster for Eindhoven.
- Category diversity in returned feed.
- Day-to-day data completeness.

---

## 8. Operational Checklist

For local/production troubleshooting:

1. Confirm `Ticketmaster:ApiKey` is configured.
2. Call `POST /api/events/refresh` (non-production only).
3. Check logs for page fetch count and rate-limit headers.
4. Verify `events` table rows and `Source = Ticketmaster`.
5. Verify `GET /api/events` payload values and pagination.

---

## 9. Known Gaps in Current Implementation

Implementation-verified gaps:

- No category persistence from Ticketmaster classifications.
- No lat/lon persistence despite model support.
- No URL/description persistence in Ticketmaster mapping.
- `category` filter parameter in `GET /api/events` is accepted but not used.
- City search uses only `city=Eindhoven`; no explicit country or geo-radius filter.

These are code-level gaps and should be treated as backlog items if required by frontend or ML consumers.
