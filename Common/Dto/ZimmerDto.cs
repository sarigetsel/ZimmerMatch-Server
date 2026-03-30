using Common.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        [Required]
        public string NameZimmer { get; set; }
        public string Description { get; set; }

        public string City { get; set; }
        public string Address { get; set; }
        [Range(-90, 90, ErrorMessage = "קו רוחב חייב להיות בין -90 ל-90")]
        public double Latitude { get; set; }
        [Range(-180, 180, ErrorMessage = "קו אורך חייב להיות בין -180 ל-180")]
        public double Longitude { get; set; }

        public int NumRooms { get; set; }
        public decimal PricePerNight { get; set; }

        public DateTime CreatedAt { get; set; }
        
        public Facility Facilities { get; set; }

        public List<IFormFile>? ImageFiles { get; set; } = new List<IFormFile>();

        public List<byte[]>? ArrImages { get; set; } = new List<byte[]>();

    }

}

