namespace MoodRadar.API.Models.Domain
{
    public class EventCache
    {
        public int Id { get; set; }
        public string Source { get; set; } // Ticketmaster
        public string ExternalId { get; set; }
        public string Title { get; set; }
        public DateTime StartTime { get; set; }
        public int? ZoneId { get; set; }
        public string Category { get; set; }
    }
}
