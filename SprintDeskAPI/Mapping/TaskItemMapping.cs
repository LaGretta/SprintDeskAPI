using AutoMapper;
using SprintDeskAPI.DTOs;
using SprintDeskAPI.Models;

namespace SprintDeskAPI.Mapping;

public class TaskItemMapping : Profile
{
    public TaskItemMapping()
    {
        CreateMap<CreateTaskDto, TaskItem>();
        CreateMap<UpdateTaskDto, TaskItem>();
        
        CreateMap<TaskItem, TaskResponseDto>()
            .ForMember(dest => dest.ProjectName,
                opt => opt
                    .MapFrom(src => src.Project.Name))
            
            
            .ForMember(dest => dest.AssignedUserFullName,
                opt => opt
                    .MapFrom(src => src.AssignedUser != null 
                    ? src.AssignedUser.FullName : null));
    }
}