using AutoMapper;
using Travel.Web.DTOs.BannerDtos;
using Travel.Web.Entities;

namespace Travel.Web.Mappings
{
    public class BannerMappings  : Profile
    {
        public BannerMappings()
        {
            CreateMap<CreateBannerDto, Banner>();
            CreateMap<UpdateBannerDto, Banner>();
            CreateMap<Banner, ResultBannerDto>().ReverseMap();
        }
    }
}
