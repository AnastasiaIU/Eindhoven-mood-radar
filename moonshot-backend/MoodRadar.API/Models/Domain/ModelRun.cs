namespace MoodRadar.API.Models.Domain
{
    public class ModelRun
    {
        public int Id { get; set; }
        public DateTime RunAt { get; set; }
        public int ZonesUpdated { get; set; }
        public string Errors { get; set; } = string.Empty;
    }
}
