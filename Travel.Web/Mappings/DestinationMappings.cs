using AutoMapper;
using Travel.Web.DTOs.DestinationDtos;
using Travel.Web.Entities;

namespace Travel.Web.Mappings
{
    public class DestinationMappings : Profile
    {
        public DestinationMappings()
        {
            CreateMap<CreateDestinationDto, Destination>().ReverseMap();
            CreateMap<UpdateDestinationDto, Destination>().ReverseMap();
            CreateMap<Destination, ResultDestinationDto>().ReverseMap();
            CreateMap<ResultDestinationDto, UpdateDestinationDto>().ReverseMap();
        }
    }
}