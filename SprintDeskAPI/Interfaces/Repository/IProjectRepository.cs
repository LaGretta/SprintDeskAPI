using SprintDeskAPI.Models;

namespace SprintDeskAPI.Interfaces.Repository;

public interface IProjectRepository
{
    Task<(List<Project> Items, int TotalCount)> GetProjectsPagedAsync(int pageNumber, int pageSize);
    Task<Project?> GetProjectByIdAsync(int id);
    Task CreateProjectAsync(Project project);
    
    void DeleteProjectAsync(Project project);
    Task SaveChangesAsync();
}
