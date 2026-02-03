using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Dto
{
    public class AvailabilityDto
    {
        public int AvailabilityId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsBooked { get; set; }

        public int ZimmerId { get; set; }
        public string ZimmerName { get; set; }

    }
}
