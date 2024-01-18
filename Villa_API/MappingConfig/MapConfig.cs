using AutoMapper;
using Villa_API.Models;
using Villa_API.Models.Dto;

namespace Villa_API.MappingConfig
{
    public class MapConfig : Profile
    {
        public MapConfig() 
        { 
            CreateMap<Villa, VillaDTO>().ReverseMap();
            CreateMap<Villa, VillaCreateDTO>().ReverseMap();
            CreateMap<Villa, VillaUpdateDTO>().ReverseMap();
        }
    }
}
