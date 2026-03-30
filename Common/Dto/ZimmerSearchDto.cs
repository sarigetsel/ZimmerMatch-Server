using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Dto
{
    public class ZimmerSearchDto
    {
        public string? FreeText { get; set; }

        public decimal? MaxPrice { get; set; }
        public string? City { get; set; }
        public int? NumOfRooms { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public bool? HasPool { get; set; }
        public bool? HasJacuzzi { get; set; }
        public bool? HasSauna { get; set; }

    }
}
