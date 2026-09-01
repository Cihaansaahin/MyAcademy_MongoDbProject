using AutoMapper;
using Travel.Web.DTOs.CategoryDtos;
using Travel.Web.Entities;

namespace Travel.Web.Mappings
{
    public class CategoryMappings : Profile
    {
        public CategoryMappings()
        {
            CreateMap<CreateCategoryDto, Category>();
            CreateMap<UpdateCategoryDto, Category>();
            CreateMap<Category, ResultCategoryDto>().ReverseMap();
        }
    }
}