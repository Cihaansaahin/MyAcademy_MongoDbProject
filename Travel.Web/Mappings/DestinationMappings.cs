using AutoMapper;
using Travel.Web.DTOs.DestinationDtos;
using Travel.Web.Entities;

namespace Travel.Web.Mappings
{
    public class DestinationMappings : Profile
    {
        public DestinationMappings()
        {
            CreateMap<CreateDestinationDto, Destination>();
            CreateMap<UpdateDestinationDto, Destination>();
            CreateMap<Destination, ResultDestinationDto>().ReverseMap();
        }
    }
}