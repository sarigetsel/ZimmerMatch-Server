using Common.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [Range(-90, 90, ErrorMessage = "קו רוחב חייב להיות בין -90 ל-90")]
        public double Latitude { get; set; } // קו רוחב למפה
        [Range(-180, 180, ErrorMessage = "קו אורך חייב להיות בין -180 ל-180")]
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
