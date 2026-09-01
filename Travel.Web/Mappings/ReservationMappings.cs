using AutoMapper;
using Travel.Web.DTOs.ReservationDtos;
using Travel.Web.Entities;

namespace Travel.Web.Mappings
{
    public class ReservationMappings : Profile
    {
        public ReservationMappings()
        {
            CreateMap<CreateReservationDto, Reservation>();
            CreateMap<UpdateReservationDto, Reservation>();
            CreateMap<Reservation, ResultReservationDto>().ReverseMap();
        }
    }
}