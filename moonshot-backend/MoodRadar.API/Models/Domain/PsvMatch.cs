using System.ComponentModel.DataAnnotations;

namespace MoodRadar.API.Models
{
    public class PsvMatch
    {
        [Key]
        public int Id { get; set; }
        public DateTime MatchDate { get; set; }
        public string HomeAway { get; set; }
        public string Status { get; set; }
        public DateTime KickOffTime { get; set; }
        public string Opponent { get; set; }
    }
}

