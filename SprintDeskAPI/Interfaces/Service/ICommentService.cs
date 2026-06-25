using SprintDeskAPI.DTOs;
using SprintDeskAPI.Models;

namespace SprintDeskAPI.Interfaces.Service;

public interface ICommentService
{
    Task<List<CommentResponseDto>> GetByTaskIdAsync(int taskId);
    Task<CommentResponseDto> CreateAsync(int taskId, int userId, CreateCommentDto dto);
    Task DeleteAsync(int id, int userId, UserRole role);
}