using AutoMapper;
using Common.Dto;
using Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Service.Services
{
    public class MapperProfile : Profile
    {
        string path = Directory.GetCurrentDirectory() + "\\images\\";

        public MapperProfile()
        {
            CreateMap<Zimmer, ZimmerDto>()
                .ForMember(dest => dest.ArrImages,
                 o => o.MapFrom(src => src.ImageUrls.Select(file => File.ReadAllBytes(path + file)).ToList()));
            CreateMap<ZimmerDto, Zimmer>()
                .ForMember(dest => dest.ImageUrls,
                 o => o.MapFrom(src => src.ImageFiles.Select(f => f.FileName).ToList()));
        }
        public byte[] myconvert(string url)
        {
            string path = Environment.CurrentDirectory + "\\Images\\" + url;
            var arr = File.ReadAllBytes(path);
            return arr;
        }
        
    }

}
