namespace MoodRadar.API.Services;

using MoodRadar.API.Models;
using System.Text.Json;

/// <summary>
/// Service for polling Open-Meteo weather API and caching hourly forecasts.
/// 
/// API Documentation: https://open-meteo.com/en/docs
/// Eindhoven Coordinates: 51.4416°N, 5.4699°E
/// 
/// Free tier: Unlimited calls, no API key required, no authentication.
/// Response: Hourly temperature, precipitation probability, and cloud cover.
/// </summary>
public interface IWeatherService
{
    /// <summary>
    /// Fetch latest hourly weather forecast from Open-Meteo for Eindhoven.
    /// Returns forecasts for the next day (24 hours) with 1-hour resolution.
    /// 
    /// Called by: Background service / cron job (every 15 minutes recommended).
    /// </summary>
    Task<List<Weather>> FetchEindhovenWeatherAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get weather for a specific hour (or nearest hour in cache).
    /// Returns null if hour is not in cache or cache is outdated.
    /// </summary>
    Weather? GetWeatherByHour(DateTime snapshotHour);

    /// <summary>
    /// Get all cached weather records.
    /// </summary>
    List<Weather> GetAllCachedWeather();

    /// <summary>
    /// Clear old cached records (older than 1 days).
    /// </summary>
    Task ClearOldCacheAsync(int olderThanDays = 1);
}

public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WeatherService> _logger;

    // In-memory cache
    private List<Weather> _cachedWeather = new();

    // Eindhoven coordinates
    private const double EindhovenLatitude = 51.4416;
    private const double EindhovenLongitude = 5.4699;

    // API constants
    private const string BaseUrl = "https://api.open-meteo.com/v1/forecast";

    public WeatherService(HttpClient httpClient, ILogger<WeatherService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Fetch hourly weather for Eindhoven from Open-Meteo.
    /// Parses JSON response and converts to Weather list.
    /// </summary>
    public async Task<List<Weather>> FetchEindhovenWeatherAsync(CancellationToken cancellationToken = default)
    {
        var weatherList = new List<Weather>();

        try
        {
            _logger.LogInformation("Starting Open-Meteo weather fetch for Eindhoven ({Lat}, {Lon})", 
                EindhovenLatitude, EindhovenLongitude);

            // Build the query string
            // Use InvariantCulture to ensure decimal separators are periods (not commas on some locales)
            var queryParams = $"?latitude={EindhovenLatitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                            $"&longitude={EindhovenLongitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                            $"&hourly=temperature_2m,precipitation_probability,cloud_cover" +
                            $"&timezone=Europe/Amsterdam" +
                            $"&forecast_days=1";

            var url = BaseUrl + queryParams;
            _logger.LogDebug("Calling Open-Meteo API: {Url}", url);
            
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Open-Meteo API returned status {StatusCode}: {Content}", 
                    response.StatusCode, errorContent);
                return weatherList;
            }

            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("Received response of {Length} characters", jsonContent.Length);
            
            var jsonDoc = JsonDocument.Parse(jsonContent);
            var root = jsonDoc.RootElement;

            // Parse hourly data
            if (root.TryGetProperty("hourly", out var hourlyData))
            {
                _logger.LogDebug("Found 'hourly' field in response");
                
                if (hourlyData.TryGetProperty("time", out var timeArray) &&
                    hourlyData.TryGetProperty("temperature_2m", out var tempArray) &&
                    hourlyData.TryGetProperty("precipitation_probability", out var precipArray) &&
                    hourlyData.TryGetProperty("cloud_cover", out var cloudArray))
                {
                    _logger.LogDebug("All required hourly fields found");
                    
                    // Convert JsonElement arrays to lists for indexed access
                    var timeList = timeArray.EnumerateArray().ToList();
                    var tempList = tempArray.EnumerateArray().ToList();
                    var precipList = precipArray.EnumerateArray().ToList();
                    var cloudList = cloudArray.EnumerateArray().ToList();

                    _logger.LogDebug("Array counts - Time: {TimeCount}, Temp: {TempCount}, Precip: {PrecipCount}, Cloud: {CloudCount}",
                        timeList.Count, tempList.Count, precipList.Count, cloudList.Count);

                    if (timeList.Count != tempList.Count || timeList.Count != precipList.Count || timeList.Count != cloudList.Count)
                    {
                        _logger.LogWarning("Array length mismatch! Time: {TimeCount}, Temp: {TempCount}, Precip: {PrecipCount}, Cloud: {CloudCount}",
                            timeList.Count, tempList.Count, precipList.Count, cloudList.Count);
                    }

                    for (int index = 0; index < timeList.Count; index++)
                    {
                        try
                        {
                            if (!DateTime.TryParse(timeList[index].GetString(), out var snapshotHour))
                            {
                                _logger.LogWarning("Failed to parse timestamp at index {Index}: {TimeValue}", 
                                    index, timeList[index].GetString());
                                continue;
                            }

                            // Ensure we're using UTC
                            snapshotHour = snapshotHour.ToUniversalTime();

                            var tempValue = tempList[index].GetDouble();
                            var precipValue = precipList[index].GetInt32();
                            var cloudValue = cloudList[index].GetInt32();

                            var weather = new Weather
                            {
                                SnapshotHour = snapshotHour,
                                TemperatureC = tempValue,
                                PrecipitationProbability = precipValue,
                                CloudCover = cloudValue,
                                CachedAt = DateTime.UtcNow
                            };

                            weatherList.Add(weather);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error parsing weather record at index {Index}", index);
                        }
                    }

                    _logger.LogInformation("Successfully parsed {Count} hourly weather records from Open-Meteo", 
                        weatherList.Count);

                    // Update in-memory cache
                    _cachedWeather = weatherList;
                }
                else
                {
                    _logger.LogError("Missing expected fields in Open-Meteo hourly response. " +
                        "Has time: {HasTime}, Has temp: {HasTemp}, Has precip: {HasPrecip}, Has cloud: {HasCloud}",
                        hourlyData.TryGetProperty("time", out _),
                        hourlyData.TryGetProperty("temperature_2m", out _),
                        hourlyData.TryGetProperty("precipitation_probability", out _),
                        hourlyData.TryGetProperty("cloud_cover", out _));
                }
            }
            else
            {
                _logger.LogError("No 'hourly' field in Open-Meteo response. Root properties: {Properties}", 
                    string.Join(", ", root.EnumerateObject().Select(p => p.Name)));
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while calling Open-Meteo API");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error from Open-Meteo response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching weather from Open-Meteo");
        }

        _logger.LogInformation("FetchEindhovenWeatherAsync completed. Returning {Count} records", weatherList.Count);
        return weatherList;
    }

    /// <summary>
    /// Get weather data for a specific hour from cache.
    /// Matches on the hour component (ignores minutes/seconds).
    /// </summary>
    public Weather? GetWeatherByHour(DateTime snapshotHour)
    {
        // Normalize to UTC and round to nearest hour
        var normalizedHour = snapshotHour.ToUniversalTime();
        normalizedHour = normalizedHour.AddMinutes(-normalizedHour.Minute)
                                      .AddSeconds(-normalizedHour.Second)
                                      .AddMilliseconds(-normalizedHour.Millisecond);

        return _cachedWeather.FirstOrDefault(w => w.SnapshotHour == normalizedHour);
    }

    /// <summary>
    /// Get all currently cached weather records.
    /// </summary>
    public List<Weather> GetAllCachedWeather()
    {
        return _cachedWeather.OrderBy(w => w.SnapshotHour).ToList();
    }

    /// <summary>
    /// Remove weather records older than specified days.
    /// Called by maintenance tasks to keep cache bounded.
    /// </summary>
    public Task ClearOldCacheAsync(int olderThanDays = 7)
    {
        var cutoffTime = DateTime.UtcNow.AddDays(-olderThanDays);
        var removedCount = _cachedWeather.RemoveAll(w => w.SnapshotHour < cutoffTime);
        
        if (removedCount > 0)
        {
            _logger.LogInformation("Cleared {Count} old weather cache records (older than {Days} days)", 
                removedCount, olderThanDays);
        }

        return Task.CompletedTask;
    }
}
