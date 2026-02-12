using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Repository.Entities
{
    public class Booking
    {
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int ZimmerId { get; set; }
        public DateTime BookingDate { get; set; } // מתי בוצעה ההזמנה 
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NumGuests { get; set; }
        public decimal TotalPrice { get; set; }
        public string? SpecialRequests { get; set; }
        public BookingStatus Status { get; set; }
        // Navigation properties
        [ForeignKey("UserId")]
        public User User { get; set; }
        [ForeignKey("ZimmerId")]
        public Zimmer Zimmer { get; set; }
    }
}
