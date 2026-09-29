using AutoMapper;
using Travel.Web.DTOs.ReservationDtos;
using Travel.Web.Entities;

namespace Travel.Web.Mappings
{
    public class ReservationMappings : Profile
    {
        public ReservationMappings()
        {
            CreateMap<CreateReservationDto, Reservation>().ReverseMap();
            CreateMap<UpdateReservationDto, Reservation>().ReverseMap();
            CreateMap<Reservation, ResultReservationDto>().ReverseMap();
            CreateMap<ResultReservationDto, UpdateReservationDto>().ReverseMap();
        }
    }
}