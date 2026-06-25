using SprintDeskAPI.Models;

namespace SprintDeskAPI.Interfaces.Repository;

public interface IAuthRepository
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> ExistUserByEmailAsync(string email);
    Task Add(User user);
    Task SaveChangesAsync();
}