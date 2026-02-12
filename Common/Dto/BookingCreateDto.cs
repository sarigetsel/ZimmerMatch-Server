using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Dto
{
    public class BookingCreateDto
    {
        public int UserId { get; set; }
        public int ZimmerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NumGuests { get; set; }
        public string? SpecialRequests { get; set; }
    }
}
