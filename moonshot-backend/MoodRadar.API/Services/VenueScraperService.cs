namespace MoodRadar.API.Services;

using MoodRadar.API.Models.Domain;
using MoodRadar.API.Data;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using System.Net;

/// <summary>
/// Service for scraping event data from Uit in Eindhoven.
/// 
/// Legal Research: April 2026 - Web scraping of public non-personal event data
/// is permissible under Dutch law and venue ToS.
/// 
/// See: docs/WEB_SCRAPING_LEGAL_RESEARCH.md
/// 
/// Source: Uit in Eindhoven (uitineindhoven.nl)
/// - Comprehensive local editorial agenda for Eindhoven
/// - Covers: culture, theater, film, music, expositions, kids events
/// - Backed by local municipality and VVV
/// - High data quality with extensive coverage
/// </summary>
public interface IVenueScraperService
{
    /// <summary>
    /// Scrape Uit in Eindhoven agenda and return all events.
    /// Called once per day by MoodUpdateService.
    /// </summary>
    Task<List<Event>> ScrapeAllVenuesAsync(CancellationToken cancellationToken = default);
}

public class VenueScraperService : IVenueScraperService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VenueScraperService> _logger;
    private readonly IServiceProvider _serviceProvider;

    // User-Agent header for respectful scraping
    private const string UserAgent = "MoodRadar-Fontys-Student-Research/1.0 (github.com/Research-Group-IxD/Eindhoven-mood-radar)";

    // Uit in Eindhoven agenda URL
    private const string UitInEindhovenUrl = "https://www.uitineindhoven.nl/agenda";

    // Results per page (Uit in Eindhoven shows 24 per page)
    private const int ResultsPerPage = 24;

    public VenueScraperService(HttpClient httpClient, ILogger<VenueScraperService> logger, IServiceProvider serviceProvider)
    {
        _httpClient = httpClient;
        _logger = logger;
        _serviceProvider = serviceProvider;

        // Set default headers
        if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
        }
        _httpClient.Timeout = TimeSpan.FromSeconds(15);
    }

    /// <summary>
    /// Scrape Uit in Eindhoven agenda and return combined event list.
    /// </summary>
    public async Task<List<Event>> ScrapeAllVenuesAsync(CancellationToken cancellationToken = default)
    {
        var allEvents = new List<Event>();

        _logger.LogInformation("Starting daily scraping from Uit in Eindhoven (uitineindhoven.nl)");

        try
        {
            allEvents = await ScrapeUitInEindhovenAsync(cancellationToken);
            _logger.LogInformation("Uit in Eindhoven: scraped {Count} events", allEvents.Count);

            // Save to database
            if (allEvents.Any())
            {
                await SaveScrapedEventsAsync(allEvents);
            }

            _logger.LogInformation("Daily scraping complete: {Count} total events", allEvents.Count);
            return allEvents;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Critical error in venue scraping: {ex}");
            throw;
        }
    }

    /// <summary>
    /// Scrape Uit in Eindhoven agenda (uitineindhoven.nl/agenda)
    /// 
    /// Strategy:
    /// 1. Build URL with date range: today + next 24h
    /// 2. Parse first page to get total result count from result-count__suffix span
    /// 3. Loop through all pages (24 results per page)
    /// 4. Extract event links with class="link-overlay"
    /// 5. Extract event ID from href and title from sr-only span
    /// 
    /// Comprehensive local editorial agenda for Eindhoven covering:
    /// - Theater, dance, cabaret
    /// - Film, cinema
    /// - Music (classical, jazz, pop, electronic, etc.)
    /// - Culture, expositions, museums
    /// - Kids events
    /// - Comedy, cabaret
    /// </summary>
    private async Task<List<Event>> ScrapeUitInEindhovenAsync(CancellationToken cancellationToken = default)
    {
        var events = new List<Event>();
        var skippedOutsideEindhoven = 0;

        try
        {
            var eindhovenDistrictBoundaries = await GetEindhovenDistrictBoundariesAsync(cancellationToken);
            if (eindhovenDistrictBoundaries.Count == 0)
            {
                _logger.LogWarning("No district boundaries available; Eindhoven-only filter is disabled for this run.");
            }

            // Build date range: today + 1 day for full 24h coverage
            var today = DateTime.Now.Date;
            var tomorrow = today.AddDays(1);
            var dateRange = $"{today:yyyy-MM-dd}-{tomorrow:yyyy-MM-dd}";
            
            // Build full URL with all parameters (matches link from user)
            var baseUrl = $"{UitInEindhovenUrl}?calendar_period=today&calendar_range={dateRange}&search=&sort=calendar&order=desc";
            
            _logger.LogInformation("Scraping Uit in Eindhoven with date range: {DateRange}", dateRange);
            _logger.LogInformation("Base URL: {Url}", baseUrl);

            // Fetch first page to get total result count
            var html = await _httpClient.GetStringAsync(baseUrl, cancellationToken);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Extract total result count from: <span class="result-count__suffix">(1 t/m 24 van 40 resultaten)</span>
            var countNode = doc.DocumentNode.SelectSingleNode("//span[@class='result-count__suffix']");
            int totalResults = 0;
            
            if (countNode != null)
            {
                var countText = countNode.InnerText;
                // Parse "(1 t/m 24 van 40 resultaten)" to extract "40"
                var match = System.Text.RegularExpressions.Regex.Match(countText, @"van\s+(\d+)\s+resultaten");
                if (match.Success && int.TryParse(match.Groups[1].Value, out int count))
                {
                    totalResults = count;
                    _logger.LogInformation("Total events found: {Count}", totalResults);
                }
            }
            else
            {
                _logger.LogWarning("Could not find result count element. HTML structure may have changed.");
            }

            // Calculate total pages needed
            int totalPages = (totalResults + ResultsPerPage - 1) / ResultsPerPage; // Ceiling division
            _logger.LogInformation("Total pages to scrape: {Pages}", totalPages);

            // Scrape all pages
            for (int page = 1; page <= Math.Max(totalPages, 1); page++)
            {
                try
                {
                    // Build page URL
                    var pageUrl = page == 1 
                        ? baseUrl 
                        : baseUrl + $"&page={page}";
                    
                    _logger.LogInformation("Scraping page {Page} of {TotalPages}", page, totalPages);
                    
                    // Fetch page HTML
                    var pageHtml = await _httpClient.GetStringAsync(pageUrl, cancellationToken);
                    var pageDoc = new HtmlDocument();
                    pageDoc.LoadHtml(pageHtml);

                    // Extract events: <a href="/agenda/682927976/the-history-of-sound-2" class="link-overlay">
                    var eventLinks = pageDoc.DocumentNode.SelectNodes("//a[@class='link-overlay']");

                    if (eventLinks == null || eventLinks.Count == 0)
                    {
                        _logger.LogWarning("Page {Page}: No event links found", page);
                        break; // No more events
                    }

                    _logger.LogDebug("Page {Page}: Found {Count} event links", page, eventLinks.Count);

                    foreach (var link in eventLinks)
                    {
                        try
                        {
                            // Extract title from sr-only span: <span class="sr-only">The History of Sound</span>
                            var titleNode = link.SelectSingleNode(".//span[@class='sr-only']");
                            var title = titleNode?.InnerText?.Trim() ?? "Untitled Event";
                            
                            if (string.IsNullOrWhiteSpace(title) || title == "Untitled Event")
                            {
                                // Fallback: try to get any text content
                                title = link.InnerText?.Trim() ?? "Event";
                            }
                            
                            // Decode HTML entities (e.g. "Dead Man&#039;s Wire" -> "Dead Man's Wire")
                            title = WebUtility.HtmlDecode(title);

                            // Extract URL/event ID from href
                            var href = link.GetAttributeValue("href", "");
                            
                            if (string.IsNullOrEmpty(href))
                                continue;

                            // Make absolute URL
                            var eventUrl = "https://www.uitineindhoven.nl" + href;
                            
                            // Extract event ID from href: /agenda/682927976/the-history-of-sound-2 → 682927976
                            var eventId = ExtractEventIdFromHref(href);

                            var evt = new Event
                            {
                                ExternalId = $"uiteindhoven_{eventId}",
                                Source = "Uit in Eindhoven",
                                Title = title,
                                StartTime = DateTime.UtcNow.AddHours(24),
                                Url = eventUrl,
                                CachedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                            };
                            
                            // PHASE 2: Scrape detail page for actual start/end times, venue, location
                            try
                            {
                                await EnrichEventDetailAsync(evt, cancellationToken);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogDebug("Could not enrich event details for {Title}: {Message}", title, ex.Message);
                                // Continue with placeholder data if detail scraping fails
                            }

                            // Eindhoven-only filter: if coordinates are present, require the point to
                            // fall inside one of the Eindhoven district boundaries.
                            if (evt.Latitude.HasValue && evt.Longitude.HasValue)
                            {
                                var isInEindhoven = IsCoordinateInsideEindhoven(
                                    evt.Latitude.Value,
                                    evt.Longitude.Value,
                                    eindhovenDistrictBoundaries);

                                if (!isInEindhoven)
                                {
                                    skippedOutsideEindhoven++;
                                    _logger.LogInformation(
                                        "Skipping non-Eindhoven event: {Title} at ({Lat}, {Lon})",
                                        evt.Title,
                                        evt.Latitude.Value,
                                        evt.Longitude.Value);
                                    continue;
                                }
                            }
                            
                            events.Add(evt);
                            _logger.LogDebug("Parsed event: {Title} (ID: {EventId})", title, eventId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogDebug("Error parsing event link: {Message}", ex.Message);
                            continue;
                        }
                    }

                    // If this page had fewer items than per-page size, we're on the last page
                    if (eventLinks.Count < ResultsPerPage)
                    {
                        _logger.LogInformation("Reached last page with {Count} items", eventLinks.Count);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error scraping page {Page}", page);
                    if (page == 1)
                        throw; // Fail fast if first page fails
                    else
                        break; // Stop pagination on error
                }
            }

            _logger.LogInformation(
                "Successfully scraped {Count} Eindhoven events from Uit in Eindhoven ({Skipped} skipped outside Eindhoven)",
                events.Count,
                skippedOutsideEindhoven);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error scraping Uit in Eindhoven: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping Uit in Eindhoven: {Message}", ex.Message);
            throw;
        }

        return events;
    }

    /// <summary>
    /// Extract event ID from href like "/agenda/682927976/the-history-of-sound-2" → "682927976"
    /// </summary>
    private string ExtractEventIdFromHref(string href)
    {
        try
        {
            // Format: /agenda/{id}/slug
            var segments = href.Split('/', System.StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length >= 2 && segments[0] == "agenda")
            {
                return segments[1]; // Return the ID
            }
        }
        catch { }

        return Guid.NewGuid().ToString().Substring(0, 8);
    }

    /// <summary>
    /// PHASE 2: Enrich event with detail page data:
    /// - Actual start time (with time of day, not just date)
    /// - End time (if available)
    /// - Venue/location name → geocoding → neighborhood
    /// - Event category from detail page
    /// </summary>
    private async Task EnrichEventDetailAsync(Event evt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(evt.Url))
            return;

        try
        {
            var detailHtml = await _httpClient.GetStringAsync(evt.Url, cancellationToken);
            var detailDoc = new HtmlDocument();
            detailDoc.LoadHtml(detailHtml);

            // Try to extract start time: look for datetime patterns like "06-04-2026 19:30"
            var startTimeStr = ExtractStartTimeFromDetailPage(detailDoc);
            if (!string.IsNullOrEmpty(startTimeStr))
            {
                if (DateTime.TryParse(startTimeStr, System.Globalization.CultureInfo.GetCultureInfo("nl-NL"), 
                    System.Globalization.DateTimeStyles.AssumeUniversal, out var parsedStart))
                {
                    evt.StartTime = DateTime.SpecifyKind(parsedStart, DateTimeKind.Utc);
                }
            }

            // Try to extract venue/location name
            var venueStr = ExtractVenueFromDetailPage(detailDoc);
            if (!string.IsNullOrEmpty(venueStr))
            {
                // Map venue name to coordinates and neighborhood
                await MapVenueToNeighborhoodAsync(evt, venueStr, cancellationToken);
            }

            // Try to extract coordinates directly from Google Maps link (Latitude, Longitude)
            var coordinates = ExtractCoordinatesFromDetailPage(detailDoc);
            if (coordinates.lat.HasValue && coordinates.lon.HasValue)
            {
                evt.Latitude = coordinates.lat.Value;
                evt.Longitude = coordinates.lon.Value;
                _logger.LogDebug("Found coordinates from detail page: {Lat}, {Lon}", coordinates.lat, coordinates.lon);

                // Map coordinates to neighborhood
                var neighborhoodId = await GetNeighborhoodFromCoordinatesAsync(
                    coordinates.lat.Value,
                    coordinates.lon.Value,
                    cancellationToken);
                
                if (neighborhoodId.HasValue)
                {
                    evt.NeighborhoodId = neighborhoodId.Value;
                    _logger.LogDebug("Mapped coordinates to neighborhood {NeighborhoodId}", neighborhoodId);
                }
            }

            // Categories are not used in Mood Radar - removed for simplicity

            _logger.LogDebug("Enriched event {Title}: StartTime={StartTime}, Venue={Venue}", 
                evt.Title, evt.StartTime, venueStr);
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Error enriching event {Title}: {Message}", evt.Title, ex.Message);
        }
    }

    /// <summary>
    /// Extract start time from detail page HTML.
    /// Look for patterns like "06-04-2026 19:30" in various HTML structures.
    /// </summary>
    private string? ExtractStartTimeFromDetailPage(HtmlDocument doc)
    {
        try
        {
            // Try common time format patterns in time/datetime elements
            var timePatterns = new[] {
                "//time[@datetime]",
                "//*[contains(@class, 'time') or contains(@class, 'datetime') or contains(@class, 'start-time')]/text()",
                "//span[contains(text(), ':')][contains(text(), '-')][position()=1]",
                "//*[contains(@class, 'event-time')]/text()",
                "//dt[contains(text(), 'Datum') or contains(text(), 'Tijd')]/following-sibling::dd[1]/text()",
                "//*[@itemprop='startDate']/text()",
                "//meta[@itemprop='startDate']/@content"
            };

            foreach (var xpath in timePatterns)
            {
                var nodes = doc.DocumentNode.SelectNodes(xpath);
                if (nodes?.Count > 0)
                {
                    var timeStr = nodes[0].GetAttributeValue("datetime", string.Empty) ?? 
                                   nodes[0].GetAttributeValue("content", string.Empty) ??
                                   nodes[0].InnerText;
                    
                    timeStr = timeStr?.Trim();
                    if (!string.IsNullOrWhiteSpace(timeStr) && timeStr.Length > 5)
                    {
                        _logger.LogDebug("Found start time via {Xpath}: {TimeStr}", xpath, timeStr);
                        return timeStr;
                    }
                }
            }
            
            _logger.LogDebug("Could not extract start time from detail page - all patterns failed");
        }
        catch (Exception ex) 
        { 
            _logger.LogDebug("Error extracting start time: {Message}", ex.Message);
        }

        return null;
    }

    /// <summary>
    /// Extract coordinates from detail page.
    /// Uit in Eindhoven embeds coordinates in Google Maps link: destination=51.444583%2C5.457367
    /// URL-decodes %2C to comma: 51.444583,5.457367
    /// </summary>
    private (double? lat, double? lon) ExtractCoordinatesFromDetailPage(HtmlDocument doc)
    {
        try
        {
            // Look for Google Maps links with destination parameter
            var mapsPattern = "//a[contains(@href, 'google.com/maps')]/@href";
            var nodes = doc.DocumentNode.SelectNodes(mapsPattern);
            
            if (nodes?.Count > 0)
            {
                var href = nodes[0].GetAttributeValue("href", "");
                if (!string.IsNullOrEmpty(href))
                {
                    // Extract destination parameter: destination=51.444583%2C5.457367
                    var match = System.Text.RegularExpressions.Regex.Match(href, @"destination=([^&]+)");
                    if (match.Success)
                    {
                        var destination = System.Net.WebUtility.UrlDecode(match.Groups[1].Value);
                        var parts = destination.Split(',');
                        
                        if (parts.Length == 2 && 
                            double.TryParse(parts[0].Trim(), System.Globalization.CultureInfo.InvariantCulture, out var lat) &&
                            double.TryParse(parts[1].Trim(), System.Globalization.CultureInfo.InvariantCulture, out var lon))
                        {
                            _logger.LogDebug("Extracted coordinates from Google Maps link: {Lat}, {Lon}", lat, lon);
                            return (lat, lon);
                        }
                    }
                }
            }
            
            _logger.LogDebug("Could not extract coordinates from detail page");
        }
        catch (Exception ex) 
        { 
            _logger.LogDebug("Error extracting coordinates: {Message}", ex.Message);
        }

        return (null, null);
    }

    /// <summary>
    /// Extract venue/location name from detail page.
    /// Look for elements that typically contain venue information.
    /// Uit in Eindhoven uses: <a class="odp-contact-information__address__link">Natlab</a>
    /// </summary>
    private string? ExtractVenueFromDetailPage(HtmlDocument doc)
    {
        try
        {
            // Try common venue-related selectors (Dutch site patterns)
            var venuePatterns = new[] {
                // Uit in Eindhoven specific pattern
                "//a[@class='odp-contact-information__address__link']/text()",
                "//*[contains(@class, 'odp-contact-information__address__link')]/text()",
                
                // Generic patterns
                "//*[contains(@class, 'venue') or contains(@class, 'location') or contains(@class, 'locatie')]/text()",
                "//*[contains(@class, 'place') or contains(@class, 'place-name')]/text()",
                "//dt[contains(text(), 'Locatie') or contains(text(), 'Plaats') or contains(text(), 'Venue')]/following-sibling::dd[1]//text()",
                "//*[@itemprop='location']/text()",
                "//strong[contains(text(), 'Locatie')]/parent::*/following-sibling::*/text()",
                "//h2[contains(text(), 'Locatie')]/following-sibling::p[1]//text()",
                "//meta[@itemprop='location']/@content",
                "//*[@itemtype='https://schema.org/Place']//text()[normalize-space()]"
            };

            foreach (var xpath in venuePatterns)
            {
                var nodes = doc.DocumentNode.SelectNodes(xpath);
                if (nodes?.Count > 0)
                {
                    var venueText = nodes[0].GetAttributeValue("content", string.Empty) ?? 
                                    nodes[0].InnerText ?? 
                                    nodes[0].InnerHtml;
                    venueText = venueText?.Trim();
                    venueText = WebUtility.HtmlDecode(venueText);
                    
                    if (!string.IsNullOrWhiteSpace(venueText) && venueText.Length > 2 && venueText.Length < 200)
                    {
                        _logger.LogDebug("Found venue via {Xpath}: {VenueText}", xpath, venueText);
                        return venueText;
                    }
                }
            }
            
            _logger.LogDebug("Could not extract venue from detail page - all patterns failed");
        }
        catch (Exception ex) 
        { 
            _logger.LogDebug("Error extracting venue: {Message}", ex.Message);
        }

        return null;
    }

    /// <summary>
    /// Extract event category from detail page.
    /// </summary>
    private string? ExtractCategoryFromDetailPage(HtmlDocument doc)
    {
        try
        {
            // Try to find category tags or classifications (Dutch patterns)
            var categoryPatterns = new[] {
                "//*[contains(@class, 'category') or contains(@class, 'tag') or contains(@class, 'categorie')]/text()",
                "//*[@itemprop='eventType']/text()",
                "//dt[contains(text(), 'Genre') or contains(text(), 'Categorie') or contains(text(), 'Type')]/following-sibling::dd[1]//text()",
                "//span[contains(@class, 'badge') or contains(@class, 'label')]/text()",
                "//strong[contains(text(), 'Genre')]/parent::*/following-sibling::*/text()",
                "//meta[@itemprop='eventType']/@content",
                "//*[@itemtype='https://schema.org/Event']//*[@itemprop='eventType']/text()"
            };

            foreach (var xpath in categoryPatterns)
            {
                var nodes = doc.DocumentNode.SelectNodes(xpath);
                if (nodes?.Count > 0)
                {
                    var categoryText = nodes[0].GetAttributeValue("content", string.Empty) ?? 
                                      nodes[0].InnerText ?? 
                                      nodes[0].InnerHtml;
                    categoryText = categoryText?.Trim();
                    categoryText = WebUtility.HtmlDecode(categoryText);
                    
                    if (!string.IsNullOrWhiteSpace(categoryText) && categoryText.Length > 2 && categoryText.Length < 100)
                    {
                        _logger.LogDebug("Found category via {Xpath}: {CategoryText}", xpath, categoryText);
                        return categoryText;
                    }
                }
            }
            
            _logger.LogDebug("Could not extract category from detail page - all patterns failed");
        }
        catch (Exception ex) 
        { 
            _logger.LogDebug("Error extracting category: {Message}", ex.Message);
        }

        return null;
    }

    /// <summary>
    /// Map venue name to coordinates and neighborhood.
    /// For MVP: hard-coded common Eindhoven venues.
    /// Phase 3: Integrate with external geocoding API (Nominatim, Google Maps).
    /// </summary>
    private async Task MapVenueToNeighborhoodAsync(Event evt, string venueName, CancellationToken cancellationToken = default)
    {
        try
        {
            // Hard-coded common Eindhoven venues for MVP
            var venueCoordinates = GetHardCodedVenueCoordinates(venueName);
            
            if (venueCoordinates.HasValue)
            {
                evt.Latitude = venueCoordinates.Value.latitude;
                evt.Longitude = venueCoordinates.Value.longitude;

                // Map coordinates to neighborhood
                var neighborhoodId = await GetNeighborhoodFromCoordinatesAsync(
                    venueCoordinates.Value.latitude, 
                    venueCoordinates.Value.longitude,
                    cancellationToken);
                
                if (neighborhoodId.HasValue)
                {
                    evt.NeighborhoodId = neighborhoodId.Value;
                    _logger.LogDebug("Mapped venue {Venue} to neighborhood {NeighborhoodId}", venueName, neighborhoodId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Error mapping venue {Venue} to neighborhood: {Message}", venueName, ex.Message);
        }
    }

    /// <summary>
    /// Hard-coded coordinates for common Eindhoven venues.
    /// Returns (latitude, longitude) if venue is recognized, null otherwise.
    /// </summary>
    private (double latitude, double longitude)? GetHardCodedVenueCoordinates(string venueName)
    {
        var venue = venueName.ToLowerInvariant();

        // Strijp-S area venues
        if (venue.Contains("strijp") || venue.Contains("klokgebouw") || venue.Contains("effenaar"))
            return (51.4449, 5.4758);
        
        if (venue.Contains("muziekgebouw"))
            return (51.4438, 5.4765);

        // City Centre venues
        if (venue.Contains("theater aan het vrijthof") || venue.Contains("vrijthof"))
            return (51.4416, 5.4704);
        
        if (venue.Contains("parktheater"))
            return (51.4456, 5.4681);
        
        if (venue.Contains("vestzaal"))
            return (51.4424, 5.4723);

        // Woensel area
        if (venue.Contains("puncheon club"))
            return (51.4320, 5.4812);

        // Default: return null (try external geocoding in Phase 3)
        return null;
    }

    /// <summary>
    /// Map coordinates to neighborhood using GeoJSON boundary checking.
    /// Returns neighborhood ID if coordinates fall within a neighborhood boundary.
    /// </summary>
    private async Task<int?> GetNeighborhoodFromCoordinatesAsync(
        double latitude, 
        double longitude,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                // Get all neighborhoods with GeoJSON boundaries
                var neighborhoods = await dbContext.Neighborhoods
                    .Where(n => !string.IsNullOrEmpty(n.GeoJsonBoundary))
                    .ToListAsync();

                // Check each neighborhood's GeoJSON boundary (simplified point-in-polygon)
                foreach (var neighborhood in neighborhoods)
                {
                    if (IsPointInGeoJsonBoundary(latitude, longitude, neighborhood.GeoJsonBoundary))
                    {
                        return neighborhood.Id;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Error determining neighborhood from coordinates: {Message}", ex.Message);
        }

        return null;
    }

    /// <summary>
    /// Point-in-polygon test using ray casting algorithm for GeoJSON polygon.
    /// Returns true if (lat, lon) falls within the polygon boundary.
    /// IMPORTANT: GeoJSON format is [longitude, latitude], not [latitude, longitude]
    /// </summary>
    private bool IsPointInGeoJsonBoundary(double lat, double lon, string geoJsonBoundary)
    {
        try
        {
            if (string.IsNullOrEmpty(geoJsonBoundary) || geoJsonBoundary == "{}")
                return false;

            // Parse GeoJSON manually
            // Expected format: {"type":"Polygon","coordinates":[[[lon,lat],[lon,lat],...,]]}
            var json = geoJsonBoundary;
            
            // Extract coordinates array
            int coordStart = json.IndexOf("[[[");
            if (coordStart == -1) return false;
            
            int coordEnd = json.LastIndexOf("]]]");
            if (coordEnd == -1) return false;

            string coordString = json.Substring(coordStart + 3, coordEnd - coordStart - 3);
            
            // Parse coordinate pairs - GeoJSON uses [longitude, latitude] order
            var polygonPoints = new List<(double lat, double lon)>();
            var pairs = coordString.Split(new[] { "],[" }, System.StringSplitOptions.None);
            
            foreach (var pair in pairs)
            {
                var coords = pair.Replace("[", "").Replace("]", "").Split(',');
                if (coords.Length >= 2 && 
                    double.TryParse(coords[0].Trim(), System.Globalization.CultureInfo.InvariantCulture, out var pointLon) &&
                    double.TryParse(coords[1].Trim(), System.Globalization.CultureInfo.InvariantCulture, out var pointLat))
                {
                    // Store as (latitude, longitude) for consistency
                    polygonPoints.Add((pointLat, pointLon));
                }
            }

            if (polygonPoints.Count < 3)
                return false;

            // Ray casting algorithm - count how many times a horizontal ray from the point crosses polygon edges
            int crossings = 0;
            for (int i = 0; i < polygonPoints.Count - 1; i++)
            {
                double lat1 = polygonPoints[i].lat;
                double lon1 = polygonPoints[i].lon;
                double lat2 = polygonPoints[i + 1].lat;
                double lon2 = polygonPoints[i + 1].lon;

                // Check if the horizontal ray from (lat, lon) to the right crosses this edge
                if ((lat1 <= lat && lat < lat2) || (lat2 <= lat && lat < lat1))
                {
                    // Calculate longitude where the ray crosses this edge
                    double xinters = (lat - lat1) * (lon2 - lon1) / (lat2 - lat1) + lon1;
                    if (lon < xinters)
                        crossings++;
                }
            }

            bool inside = crossings % 2 == 1;
            if (inside)
            {
                _logger.LogDebug("Point ({Lat}, {Lon}) is inside polygon", lat.ToString("F6"), lon.ToString("F6"));
            }
            return inside;
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Error in point-in-polygon test: {Message}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Load district boundaries used for Eindhoven-only filtering.
    /// Returns empty when no boundaries are available.
    /// </summary>
    private async Task<List<string>> GetEindhovenDistrictBoundariesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                return await dbContext.Districts
                    .Where(d => !string.IsNullOrEmpty(d.GeoJsonBoundary) && d.GeoJsonBoundary != "{}")
                    .Select(d => d.GeoJsonBoundary)
                    .ToListAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Could not load district boundaries for Eindhoven filter: {Message}", ex.Message);
            return new List<string>();
        }
    }

    /// <summary>
    /// Checks whether a point falls inside at least one Eindhoven district boundary.
    /// Fail-open when boundaries are unavailable to avoid data loss.
    /// </summary>
    private bool IsCoordinateInsideEindhoven(double latitude, double longitude, IReadOnlyCollection<string> districtBoundaries)
    {
        if (districtBoundaries.Count == 0)
            return true;

        foreach (var boundary in districtBoundaries)
        {
            if (IsPointInGeoJsonBoundary(latitude, longitude, boundary))
            {
                return true;
            }
        }

        return false;
    }


    /// <summary>
    /// Save scraped events to database, replacing old Uit in Eindhoven events.
    /// 
    /// DEDUPLICATION STRATEGY:
    /// - Clears all existing "Uit in Eindhoven" events before inserting new ones
    /// - Resets the Event ID sequence to start from 0
    /// - This ensures no duplicates accumulate across multiple scrape runs
    /// - Latest event data is always fresh since we re-scrape the entire agenda
    /// </summary>
    private async Task SaveScrapedEventsAsync(List<Event> events)
    {
        try
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // DEDUPLICATION: Remove only Uit in Eindhoven events, preserve other sources
                // This prevents duplicates from repeated scrape runs while keeping aggregated data
                var existingUitEvents = dbContext.Events
                    .Where(e => e.Source == "Uit in Eindhoven")
                    .ToList();

                if (existingUitEvents.Any())
                {
                    dbContext.Events.RemoveRange(existingUitEvents);
                    await dbContext.SaveChangesAsync();
                    _logger.LogDebug("Removed {Count} existing Uit in Eindhoven events from database", existingUitEvents.Count);
                }

                // Check if Events table is now empty
                var remainingEventCount = await dbContext.Events
                    .AsNoTracking()
                    .CountAsync();

                // Determine next ID to use for the scraped events
                int startId = 1;
                
                if (remainingEventCount > 0)
                {
                    // Find max ID in remaining events (from other sources)
                    var remainingMaxId = await dbContext.Events
                        .AsNoTracking()
                        .MaxAsync(e => (int?)e.Id) ?? 0;
                    
                    startId = remainingMaxId + 1;
                }

                // Manually assign IDs to scraped events to ensure they start from correct ID
                for (int i = 0; i < events.Count; i++)
                {
                    events[i].Id = startId + i;
                }

                // Clear DbContext cache to ensure clean insert
                dbContext.ChangeTracker.Clear();

                // Add new Uit in Eindhoven events with assigned IDs
                dbContext.Events.AddRange(events);
                await dbContext.SaveChangesAsync();

                _logger.LogInformation("Saved {Count} scraped events from Uit in Eindhoven to database with IDs {StartId}-{EndId} (table was empty: {TableWasEmpty})", 
                    events.Count, startId, startId + events.Count - 1, remainingEventCount == 0);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error saving scraped events to database: {ex.Message}");
            throw;
        }
    }
}
