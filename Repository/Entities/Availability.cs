using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.Entities
{
    public class Availability // יומן זמינות
    {
        public int AvailabilityId { get; set; }
        public int ZimmerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsBooked { get; set; } //האם התקופה תפוסה

        [ForeignKey("ZimmerId")]
        public Zimmer Zimmers{ get; set; }
    }
}
