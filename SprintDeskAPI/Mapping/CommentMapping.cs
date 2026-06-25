using AutoMapper;
using SprintDeskAPI.DTOs;
using SprintDeskAPI.Models;

namespace SprintDeskAPI.Mapping;

public class CommentMapping : Profile
{
    public CommentMapping()
    {
        CreateMap<Comment, CommentResponseDto>()
            .ForMember(dest => dest.TaskTitle,
                opt => opt.MapFrom(src => src.TaskItem.Title))
            .ForMember(dest => dest.UserFullName,
                opt => opt.MapFrom(src => src.User.FullName));
    }
}