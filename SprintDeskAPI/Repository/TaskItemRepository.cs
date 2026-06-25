using Microsoft.EntityFrameworkCore;
using SprintDeskAPI.Data;
using SprintDeskAPI.Interfaces.Repository;
using SprintDeskAPI.Models;

namespace SprintDeskAPI.Repository;

public class TaskItemRepository : ITaskItemRepository
{
    private readonly AppDbContext _context;
    public TaskItemRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<TaskItem> Items, int TotalCount)> GetTaskItemPagedAsync(int pageNumber, int pageSize)
    {
        var find = _context.TaskItems
            .Include(t => t.Project)
            .Include(t => t.AssignedUser)
            .AsNoTracking();
        
        var totalCount = await find.CountAsync();
        
        var items =  await find
            .OrderByDescending(t => t.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, totalCount);
    }

    public async Task<(List<TaskItem> Items, int TotalCount)> GetMyTasksAsync(int pageNumber, int pageSize, int userId)
    {
        var find = _context.TaskItems
            .Include(t => t.Project)
            .Include(t => t.AssignedUser)
            .Where(n => n.AssignedUserId == userId).AsNoTracking();
        
        var totalCount = await find.CountAsync();
        
        var items =  await find
            
            .OrderByDescending(t => t.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, totalCount);
    }

    public async Task<(List<TaskItem> Items, int TotalCount)> GetByProjectIdAsync(int pageNumber, int pageSize, int projectId)
    {
        var find = _context.TaskItems
            .Include(t => t.Project)
            .Include(t => t.AssignedUser)
            .Where(n => n.ProjectId == projectId).AsNoTracking();
        
        var totalCount = await find.CountAsync();
        
        var items = await find
            
            .OrderByDescending(t => t.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, totalCount);
    }

    public async Task<TaskItem?> GetTaskItemByIdAsync(int id)
    {
        var find = await _context.TaskItems
            .Include(t => t.Project)
            .Include(t => t.AssignedUser)
            .FirstOrDefaultAsync(t => t.Id == id);
        return find;
    }

    public async Task CreateTaskItemAsync(TaskItem taskItem)
    {
        await _context.TaskItems.AddAsync(taskItem);
    }

    public void DeleteTaskItem(TaskItem taskItem)
    {
         _context.TaskItems.Remove(taskItem);
    }

    public async Task SaveChangesAsync()
    {
         await _context.SaveChangesAsync();
    }
}
