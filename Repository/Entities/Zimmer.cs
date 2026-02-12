using System.ComponentModel.DataAnnotations.Schema;
using Common.Enums;

namespace Repository.Entities
{
    public class Zimmer
    {
        public int ZimmerId { get; set; }
        public int OwnerId { get; set; }
        public string NameZimmer { get; set; }
        public string Description { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public double Latitude { get; set; } // קו רוחב למפה
        public double Longitude { get; set; } // קו אורך למפה
        public int NumRooms { get; set; }
        public decimal PricePerNight { get; set; }
        public DateTime CreatedAt { get; set; } // תאריך הוספה למערכת
        public Facility Facilities { get; set; } = new();

        [ForeignKey("OwnerId")]
        public User Owner { get; set; }
        public List<String> ImageUrls { get; set; } = new();
        public List<Availability> Availabilities { get; set; } = new();
        public List<Booking> Bookings { get; set; }

    }
}
