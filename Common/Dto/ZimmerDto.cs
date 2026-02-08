using Common.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
          


namespace Common.Dto
{
    public class ZimmerDto
    {
        

        public int ZimmerId { get; set; }
        public int OwnerId { get; set; }

        public string NameZimmer { get; set; }
        public string Description { get; set; }

        public string City { get; set; }
        public string Address { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public int NumRooms { get; set; }
        public decimal PricePerNight { get; set; }

        public DateTime CreatedAt { get; set; }
        
        public Facility Facilities { get; set; } 

        public List<IFormFile> ImageFiles { get; set; }

        public List<byte[]>? ArrImages { get; set; }


    }

}

