namespace MoodRadar.API.Models.Domain;

/// <summary>
/// Hourly weather data snapshot cached from Open-Meteo API.
/// Keyed by hour (SnapshotHour is rounded to the nearest hour).
/// Used as an input feature for mood prediction.
/// </summary>
public class Weather
{
    /// <summary>
    /// Hour timestamp (UTC), rounded to the nearest hour.
    /// Used as the primary key since each hour is unique.
    /// Example: 2026-03-28T14:00:00Z
    /// </summary>
    public DateTime SnapshotHour { get; set; }
    
    /// <summary>
    /// Temperature in Celsius (2 meters above ground).
    /// Source: Open-Meteo temperature_2m
    /// </summary>
    public double TemperatureC { get; set; }
    
    /// <summary>
    /// Probability of precipitation (0-100%).
    /// Source: Open-Meteo precipitation_probability
    /// </summary>
    public int PrecipitationProbability { get; set; }
    
    /// <summary>
    /// Cloud cover percentage (0-100%).
    /// Source: Open-Meteo cloud_cover
    /// </summary>
    public int CloudCover { get; set; }
    
    /// <summary>
    /// When this row was inserted/last updated.
    /// Used to determine cache staleness.
    /// </summary>
    public DateTime CachedAt { get; set; }
}
