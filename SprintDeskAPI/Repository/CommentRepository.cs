using SprintDeskAPI.Data;
using SprintDeskAPI.Interfaces.Repository;
using SprintDeskAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace SprintDeskAPI.Repository;

public class CommentRepository : ICommentRepository
{
    private readonly AppDbContext _context;
    public CommentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Comment?> GetCommentByIdAsync(int id)
    {
        var find = await _context.Comments
            .Include(c => c.TaskItem)
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id);
        return find;
    }

    public async Task<List<Comment>> GetCommentsByTaskIdAsync(int taskId)
    {
        return await _context.Comments
            .Include(c => c.TaskItem)
            .Include(c => c.User)
            .Where(c => c.TaskItemId == taskId)
            .OrderBy(c => c.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddCommentAsync(Comment comment)
    {
        await  _context.Comments.AddAsync(comment);
    }

    public async Task DeleteCommentAsync(Comment comment)
    {
         _context.Comments.Remove(comment);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
