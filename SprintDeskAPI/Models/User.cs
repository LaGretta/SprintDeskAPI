namespace SprintDeskAPI.Models;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Developer;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Comment> Comments { get; set; } = new();
    public List<TaskItem> TaskItems { get; set; } = new();
}

public enum UserRole
{
    Admin = 1,
    Manager = 2,
    Developer = 3
}