using AutoMapper;
using Travel.Web.DTOs.TourDtos;
using Travel.Web.Entities;

namespace Travel.Web.Mappings
{
    public class TourMappings : Profile
    {
        public TourMappings()
        {
            CreateMap<CreateTourDto, Tour>().ReverseMap();
            CreateMap<UpdateTourDto, Tour>().ReverseMap();
            CreateMap<Tour, ResultTourDto>().ReverseMap();
            CreateMap<ResultTourDto, UpdateTourDto>().ReverseMap();
        }
    }
}