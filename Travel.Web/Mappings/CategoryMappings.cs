using AutoMapper;
using Travel.Web.DTOs.CategoryDtos;
using Travel.Web.Entities;

namespace Travel.Web.Mappings
{
    public class CategoryMappings : Profile
    {
        public CategoryMappings()
        {
            CreateMap<CreateCategoryDto, Category>().ReverseMap();
            CreateMap<UpdateCategoryDto, Category>().ReverseMap();
            CreateMap<Category, ResultCategoryDto>().ReverseMap();
            CreateMap<ResultCategoryDto, UpdateCategoryDto>().ReverseMap();
        }
    }
}