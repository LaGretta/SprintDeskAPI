using Microsoft.EntityFrameworkCore;
using SprintDeskAPI.Data;
using SprintDeskAPI.Interfaces.Repository;
using SprintDeskAPI.Models;

namespace SprintDeskAPI.Repository;

public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _context;
    public ProjectRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Project> Items, int TotalCount)> GetProjectsPagedAsync(int pageNumber, int pageSize)
    {
        var find = _context.Projects.AsNoTracking();
        
        var count = await find.CountAsync();
        
        var items = await find
            .OrderByDescending(n => n.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, count);
    }

    public async Task<Project?> GetProjectByIdAsync(int id)
    {
        var find = await _context.Projects.FindAsync(id);
        return find;
    }

    public async Task CreateProjectAsync(Project project)
    {
         await  _context.Projects.AddAsync(project);
    }

    public void DeleteProjectAsync(Project project)
    {
         _context.Projects.Remove(project);
    }

    public async Task SaveChangesAsync()
    {
         await _context.SaveChangesAsync();
    }
}
