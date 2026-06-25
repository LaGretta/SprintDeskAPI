using SprintDeskAPI.Data;
using SprintDeskAPI.Interfaces.Repository;
using SprintDeskAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace SprintDeskAPI.Repository;

public class AuthRepository : IAuthRepository
{
    private readonly AppDbContext _context;
    public AuthRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var find = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        return find;
    }

    public async Task<User?> ExistUserByEmailAsync(string email)
    {
         var find = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
         return find;
    }

    public async Task Add(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
