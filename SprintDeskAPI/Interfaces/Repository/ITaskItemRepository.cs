using SprintDeskAPI.Models;

namespace SprintDeskAPI.Interfaces.Repository;

public interface ITaskItemRepository
{
    Task<(List<TaskItem> Items, int TotalCount)> 
        GetTaskItemPagedAsync(int pageNumber, int pageSize);
    
    
    Task<(List<TaskItem> Items, int TotalCount)> 
        GetMyTasksAsync(int pageNumber, int pageSize, int userId);
    
    
    Task<(List<TaskItem> Items, int TotalCount)> 
        GetByProjectIdAsync(int pageNumber, int pageSize, int projectId);
    
    
    
    Task<TaskItem?>  GetTaskItemByIdAsync(int id);
    Task CreateTaskItemAsync(TaskItem taskItem);
    void DeleteTaskItem(TaskItem taskItem);
    Task SaveChangesAsync();
   
}
