using AutoMapper;
using Travel.Web.DTOs.CommentDtos;
using Travel.Web.Entities;

namespace Travel.Web.Mappings
{
    public class CommentMappings : Profile
    {
        public CommentMappings()
        {
            CreateMap<CreateCommentDto, Comment>();
            CreateMap<Comment, ResultCommentDto>().ReverseMap();
        }
    }
}