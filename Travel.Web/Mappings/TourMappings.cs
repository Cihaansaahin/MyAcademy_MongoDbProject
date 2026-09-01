using AutoMapper;
using Travel.Web.DTOs.TourDtos;
using Travel.Web.Entities;

namespace Travel.Web.Mappings
{
    public class TourMappings : Profile
    {
        public TourMappings()
        {
            CreateMap<CreateTourDto, Tour>();
            CreateMap<UpdateTourDto, Tour>();
            CreateMap<Tour, ResultTourDto>().ReverseMap();
        }
    }
}