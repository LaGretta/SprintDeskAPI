using SprintDeskAPI.DTOs;

namespace SprintDeskAPI.Interfaces.Service;

public interface IProjectService
{
    Task<PagedResponseDto<ProjectResponseDto>> GetPaged(int pageNumber, int pageSize);
    Task<ProjectResponseDto> GetById(int id);
    Task<ProjectResponseDto> CreateProject(CreateProjectDto createProjectDto);
    Task<ProjectResponseDto> UpdateProjectById(int id, UpdateProjectDto updateProjectDto );
    Task<ProjectResponseDto> CompleteAsync(int id);
    Task<ProjectResponseDto> ArchiveAsync(int id);
    
    Task DeleteProjectById(int id);
}