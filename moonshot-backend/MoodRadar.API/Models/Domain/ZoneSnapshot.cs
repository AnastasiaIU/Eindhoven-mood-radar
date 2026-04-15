namespace MoodRadar.API.Models.Domain
{
    public class ZoneSnapshot
    {
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public int EventCount { get; set; }

        public int PsvMatchCount { get; set; }

        public string? WeatherSummary { get; set; }

        public double? PredictionScore { get; set; }

        public string? RawJson { get; set; }
    }
}
