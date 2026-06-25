using SprintDeskAPI.Models;

namespace SprintDeskAPI.DTOs;

public class CreateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public DateTime? DueDate { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public int ProjectId { get; set; }
    public int? AssignedUserId { get; set; }
}
public class UpdateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
}

public class UpdateTaskStatusDto
{
    public TaskItemStatus Status { get; set; }
}

public class AssignTaskDto
{
    public int? AssignedUserId { get; set; }
}

public class TaskResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? AssignedUserId { get; set; }
    public string? AssignedUserFullName { get; set; }
    

    public TaskItemStatus Status { get; set; } 
    public TaskPriority Priority { get; set; } 

    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; } 
    public DateTime UpdatedAt { get; set; } 
}