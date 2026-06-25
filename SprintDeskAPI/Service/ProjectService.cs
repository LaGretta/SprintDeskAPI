using AutoMapper;
using SprintDeskAPI.DTOs;
using SprintDeskAPI.Interfaces.Repository;
using SprintDeskAPI.Interfaces.Service;
using SprintDeskAPI.Models;

namespace SprintDeskAPI.Service;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IMapper _mapper;
    public ProjectService(IProjectRepository projectRepository, IMapper mapper)
    {
        _projectRepository = projectRepository;
        _mapper = mapper;
    }

    public async Task<PagedResponseDto<ProjectResponseDto>> GetPaged(int pageNumber, int pageSize)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);
        
         var paged  = await _projectRepository.GetProjectsPagedAsync(pageNumber, pageSize);
         var map =  _mapper.Map<List<ProjectResponseDto>>(paged.Items);

         return new PagedResponseDto<ProjectResponseDto>
         {
             Items = map,
             PageNumber = pageNumber,
             PageSize = pageSize,
             TotalItems = paged.TotalCount,
             TotalPages = (int)Math.Ceiling((double)paged.TotalCount / pageSize)
         };
    }

    public async Task<ProjectResponseDto> GetById(int id)
    {
         var find = await _projectRepository.GetProjectByIdAsync(id);
         
         if(find == null)
             throw new Exception("Project not found");
         
         return _mapper.Map<ProjectResponseDto>(find);
    }

    public async Task<ProjectResponseDto> CreateProject(CreateProjectDto createProjectDto)
    {
         var map = _mapper.Map<Project>(createProjectDto);

         map.Status = ProjectStatus.Active;
         map.CreatedAt = DateTime.UtcNow;
         map.UpdatedAt = DateTime.UtcNow;
         
         await _projectRepository.CreateProjectAsync(map);
         await _projectRepository.SaveChangesAsync();
         
         return _mapper.Map<ProjectResponseDto>(map);
    }

    public async Task<ProjectResponseDto> UpdateProjectById(int id, UpdateProjectDto updateProjectDto)
    {
         var find = await _projectRepository.GetProjectByIdAsync(id);
        
         if(find == null)
             throw new Exception("Project not found");
         
         _mapper.Map(updateProjectDto, find);   
         
         find.Id = id;
         find.UpdatedAt = DateTime.UtcNow;
         
         await _projectRepository.SaveChangesAsync();
         return _mapper.Map<ProjectResponseDto>(find);
    }

    public async Task<ProjectResponseDto> CompleteAsync(int id)
    {
         var find = await _projectRepository.GetProjectByIdAsync(id);
         
         if(find == null)
             throw new Exception("Project not found");
         
         find.Status = ProjectStatus.Completed;
         find.UpdatedAt = DateTime.UtcNow;
         
         await _projectRepository.SaveChangesAsync();
         return _mapper.Map<ProjectResponseDto>(find);
    }

    public async Task<ProjectResponseDto> ArchiveAsync(int id)
    {
        var find = await _projectRepository.GetProjectByIdAsync(id);
         
        if(find == null)
            throw new Exception("Project not found");
         
        find.Status = ProjectStatus.Archived;
        find.UpdatedAt = DateTime.UtcNow;
         
        await _projectRepository.SaveChangesAsync();
        return _mapper.Map<ProjectResponseDto>(find);
    }

    public async Task DeleteProjectById(int id)
    {
        var find  = await _projectRepository.GetProjectByIdAsync(id);
        
        if(find == null)
            throw new Exception("Project not found");
        
        _projectRepository.DeleteProjectAsync(find);
        await _projectRepository.SaveChangesAsync();
    }
}