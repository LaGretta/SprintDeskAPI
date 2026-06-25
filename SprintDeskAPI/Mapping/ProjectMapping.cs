using AutoMapper;
using SprintDeskAPI.DTOs;
using SprintDeskAPI.Models;

namespace SprintDeskAPI.Mapping;

public class ProjectMapping : Profile
{
    public ProjectMapping()
    {
        CreateMap<CreateProjectDto, Project>();
        CreateMap<UpdateProjectDto, Project>();
        CreateMap<Project, ProjectResponseDto>();
    }
}