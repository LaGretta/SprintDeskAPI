using AutoMapper;
using SprintDeskAPI.DTOs;
using SprintDeskAPI.Interfaces.Repository;
using SprintDeskAPI.Interfaces.Service;
using SprintDeskAPI.Models;

namespace SprintDeskAPI.Service;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IMapper _mapper;
    public CommentService(ICommentRepository commentRepository, IMapper mapper)
    {
        _commentRepository = commentRepository;
        _mapper = mapper;
    }

    public async Task<List<CommentResponseDto>> GetByTaskIdAsync(int taskId)
    {
         var comments = await _commentRepository.GetCommentsByTaskIdAsync(taskId);
         return _mapper.Map<List<CommentResponseDto>>(comments);
    }

    public async Task<CommentResponseDto> CreateAsync(int taskId, int userId, CreateCommentDto dto)
    { 
         var comment = _mapper.Map<Comment>(dto);
         
         comment.TaskItemId = taskId;
         comment.UserId = userId;
         comment.CreatedAt = DateTime.UtcNow;
        
         await _commentRepository.AddCommentAsync(comment);
         await _commentRepository.SaveChangesAsync();

         var savedComment = await _commentRepository.GetCommentByIdAsync(comment.Id);
         
         return _mapper.Map<CommentResponseDto>(savedComment ?? comment);
    }

    public async Task DeleteAsync(int id, int userId, UserRole role)
    {
         var comment = await _commentRepository.GetCommentByIdAsync(id);
         if(comment == null)
             throw new Exception("Comment not found");
         
         if (comment.UserId != userId && role != UserRole.Admin)
             throw new Exception("You cannot delete this comment");
             
         await _commentRepository.DeleteCommentAsync(comment);
         await _commentRepository.SaveChangesAsync();
    }
}
