using AutoMapper;
using Common.Dto;
using Common.Enums;
using Repository.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public class MapperProfile : Profile
    {
        string path = Directory.GetCurrentDirectory() + "\\images\\";

        public MapperProfile()
        {

             CreateMap<Zimmer, ZimmerDto>()
             .ForMember(dest => dest.ArrImages,
              o => o.MapFrom(src => src.ImageUrls.Select(fileName => File.ReadAllBytes(Path.Combine(path, fileName))).ToList()));

            CreateMap<ZimmerDto, Zimmer>()
                .ForMember(dest => dest.ImageUrls,
                 o => o.MapFrom(src => src.ImageFiles != null?
                 src.ImageFiles.Select(f => f.FileName).ToList(): new List<string>()));


            CreateMap<User, UserDto>().ReverseMap();

            CreateMap<Availability, AvailabilityDto>()
               .ForMember(dest => dest.ZimmerName,
               opt => opt.MapFrom(src => src.Zimmers !=null ? src.Zimmers.NameZimmer:null))
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndTime));

            CreateMap<AvailabilityDto, Availability>()
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.Zimmers, opt => opt.Ignore());

            CreateMap<Booking, BookingDto>()
               .ForMember(dest => dest.UserName,
                          opt => opt.MapFrom(src => src.User != null ? src.User.Name : null))
               .ForMember(dest => dest.ZimmerName,
                          opt => opt.MapFrom(src => src.Zimmer != null ? src.Zimmer.NameZimmer : null))
               .ForMember(dest => dest.Status,
               opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<BookingDto, Booking>()
                  .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<BookingStatus>(src.Status)))
                  .ForMember(dest => dest.User, opt => opt.Ignore())
                  .ForMember(dest => dest.Zimmer, opt => opt.Ignore());

            CreateMap<BookingCreateDto, Booking>()
                  .ForMember(dest => dest.User, opt => opt.Ignore())   // מונע ניסיון ליצור משתמש חדש
                  .ForMember(dest => dest.Zimmer, opt => opt.Ignore());
        }
        public byte[] myconvert(string url)
        {
            string path = Environment.CurrentDirectory + "\\Images\\" + url;
            var arr = File.ReadAllBytes(path);
            return arr;
        }
        
    }

}
