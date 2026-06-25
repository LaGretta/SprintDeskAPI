using SprintDeskAPI.Models;

namespace SprintDeskAPI.DTOs;

public class CreateCommentDto
{
    public string Text { get; set; } = string.Empty;
}

public class CommentResponseDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } 

    public int TaskItemId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
}