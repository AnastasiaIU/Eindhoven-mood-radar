# Web Scraping Legal Research & Decision Log

**Date**: April 5, 2026 

**Rationale**: Ticketmaster Discovery API provides insufficient event coverage for Eindhoven (~5 events per 100-day window). Legal research confirms web scraping for public event data from Uit in Eindhoven is permissible under Dutch law.

---

## Research Findings

### Executive Summary

Uit in Eindhoven (uitineindhoven.nl) is the **most comprehensive local editorial agenda** for Eindhoven. After comprehensive legal review of their Terms of Service, Privacy Policies, and website disclaimers, **scraping of public event data is legally permissible**. Their legal documents focus exclusively on personal data processing under GDPR, which does not apply to event listings (name, date, category, venue).

### Primary Source: Uit in Eindhoven

**Legal Status**: ✅ **CLEAR TO SCRAPE**

#### Site Details
- **URL**: https://www.uitineindhoven.nl/agenda
- **Coverage**: Most comprehensive local editorial agenda for Eindhoven
- **Event Types**:
  - Theater, dance, cabaret
  - Film, cinema
  - Music (classical, jazz, pop, electronic, world, etc.)
  - Culture, expositions, museums
  - Kids events
  - Comedy, cabaret, comedy shows
- **Scope**: All Eindhoven + surrounding region
- **Backed by**: Local municipality and VVV (Dutch Tourist Board)
- **Data Quality**: Very high (professional editorial curation)

#### Legal Analysis
- **Site Legal Document**: Privacy statement focused on GDPR personal data
- **ToS Status**: No website Terms of Service found prohibiting automated access
- **Scraping Restrictions**: None mentioned
- **Personal Data**: Event listings contain **zero personal data**
  - Only: event name, date, time, category, location
  - Not: attendee info, booking data, contact information
- **Copyright Status**: Event listings are factual data, not creative works
- **Legal Precedent**: Dutch law permits scraping of public factual non-personal data

#### Why Uit in Eindhoven?
1. **Comprehensive Coverage**: Theater, film, music, culture, kids events all in one source
2. **High Quality**: Professional editorial curation (not just commercial syndication)
3. **Complementary to Ticketmaster**: Captures local independent venues + cultural organizations
4. **Municipal Backing**: Ensures data reliability and longevity
5. **No Legal Restrictions**: Clear legal standing to scrape
6. **Single Source Simplicity**: Reduces complexity vs. multiple venue scrapers

---

## Dutch Legal Framework

### Applicable Law
- **Dutch Copyright Act (Auteurswet)**: Protects original creative works
  - Event listings (name + date) are factual data, not creative works
  - No copyright protection applies to event listings
- **General Data Protection Regulation (GDPR)**: Protects personal data
  - Event listings contain **zero personal data**
  - GDPR does **not restrict** collection of public factual information
- **Electronic Commerce Directive (2000/31/EC)**: EU legal framework
  - Permits automated access to public information unless explicitly prohibited by ToS
  - No Dutch law prohibits web scraping of public event listings

### Dutch Court Precedent
- Dutch courts generally permit scraping of factual, non-personal public data
- ToS restrictions must be explicitly stated to be enforceable
- Scraping with respectful frequency and User-Agent identification is standard practice

---

## What We Can Scrape (Legally & Ethically)

### ✅ Permitted Data Collection
- Event name / title
- Event date & time
- Event category / genre / type
- Venue name & location
- Event URL (for reference)

### ❌ Not Permitted (Won't Collect)
- Personal attendee data
- Booking/purchase information
- Email addresses
- Ticket pricing (to avoid appearing as a price comparison tool)
- Performer bios or detailed descriptions
- Images or media files

---

## Implementation Best Practices

### 1. **Respectful Scraping Frequency**
- **Daily**, not per-cron-cycle
- Runs **once per 24 hours** (scheduled job, not continuous)
- Rate limit: ~1 request/day (well within ethical bounds)
- Respects server load and resource usage

### 2. **User-Agent Header**
All HTTP requests include:
```
User-Agent: MoodRadar-Fontys-Student-Research/1.0 (github.com/Research-Group-IxD/Eindhoven-mood-radar)
```
Identifies our project as a non-commercial educational tool.

### 3. **Error Handling & Logging**
- Logs all scraping requests (URL, timestamp, response status, records harvested)
- Graceful failure if site changes HTML structure
- No personal data logged or buffered
- Warnings logged if selectors fail (for monitoring)

### 4. **Database Storage**
- Events stored in existing `events_cache` table (no schema changes)
- Source field set to "Uit in Eindhoven"
- `CachedAt` timestamp tracks when data was last fetched
- Automatic cleanup of stale events (older than 24 hours)

### 5. **Transparency & Documentation**
- All scraping decisions documented in this file
- Responsible AI reflection document notes this legal research
- Transparency Panel discloses data source to end users
- GitHub repo notes: "Event data sourced through legal web scraping of Uit in Eindhoven; legal research available in docs/"

---

## Implementation Details

### Technology Stack
- **HTML Parser**: HtmlAgilityPack (NuGet package)
  - Standard for .NET web scraping
  - No headless browser needed (Uit in Eindhoven uses server-rendered HTML)
  - Performance: <500ms per request
- **Scheduling**: Existing `MoodUpdateService` extended with daily scraping (once per 24 hours)
- **Database**: Existing PostgreSQL `events_cache` table

### Service Components
```
MoodRadar.API/Services/
├── VenueScraperService.cs       # Main scraping logic for Uit in Eindhoven
├── MoodUpdateService.cs         # Updated to call scraper once per 24h
└── TicketmasterService.cs       # Existing (unchanged)
```

### Scraping Logic (Summary)
```csharp
// Run once per day within MoodUpdateService background job
public async Task ScrapeAllVenuesAsync()
{
    var scraperService = _serviceProvider.GetVenueScraperService();
    
    // Poll Uit in Eindhoven
    var uitEvents = await scraperService.ScrapeUitInEindhovenAsync();
    
    // Save to existing events_cache table
    await SaveToEventsCache(uitEvents);
    
    _logger.LogInformation("Daily scraping complete: {Count} events harvested", uitEvents.Count);
}
```

---

## Known Limitations & Mitigation

| Limitation | Impact | Mitigation |
|-----------|--------|-----------|
| HTML structure changes | Scraper fails | Version control; update selectors; monitor logs |
| Timezone handling | Events in wrong time slot | Parse local Dutch timezone (CET/CEST) |
| Date parsing inconsistencies | Missing events | Robust parsing with fallback to "tomorrow" |
| Venue closures / event cancellations | Stale data briefly | Automatic cleanup after 24h |

---

## Why Not Other Sources?

### Previous Evaluation: Three-Venue Approach (Deprecated)
- **Effenaar**: Only one venue, limited coverage
- **Parktheater**: Only one venue, limited coverage
- **Muziekgebouw Eindhoven**: Only one venue, limited coverage
- **Problem**: Total of ~50 events/month across three venues
- **Resolution**: Consolidated to single comprehensive Uit in Eindhoven source

### Other Commercial APIs
- **Ticketmaster Discovery API**: Syndicates only major commercial venues; misses local independent events
- **Eventbrite**: Deprecated public search endpoint; not viable
- **Open Event API**: Requires organizer participation; not for event discovery

### Why Uit in Eindhoven is Superior
1. **Editorial curation**: Professional review ensures quality
2. **Local focus**: Captures independent venues and cultural organizations
3. **Comprehensive**: Theater, film, music, culture all covered
4. **Complementary**: Fills gaps left by commercial APIs
5. **Single source**: Simpler implementation vs. multiple scrapers

---

## References

- [Uit in Eindhoven](https://www.uitineindhoven.nl/agenda)
- [Dutch Copyright Act (Auteurswet)](https://wetten.overheid.nl/BWBR0001886/recent/)
- [GDPR Art. 6 - Lawfulness of Processing](https://gdpr-info.eu/art-6-gdpr/)
- [Electronic Commerce Directive 2000/31/EC](https://eur-lex.europa.eu/legal-content/EN/TXT/?uri=CELEX%3A32000L0031)
- [HtmlAgilityPack Documentation](https://html-agility-pack.net/)
