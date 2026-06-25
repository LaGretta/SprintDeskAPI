using SprintDeskAPI.Models;

namespace SprintDeskAPI.Interfaces.Repository;

public interface ICommentRepository
{
    Task<Comment?> GetCommentByIdAsync(int id);
    Task<List<Comment>> GetCommentsByTaskIdAsync(int taskId);
    Task AddCommentAsync(Comment comment);
    Task DeleteCommentAsync(Comment comment);
    Task SaveChangesAsync();
}
