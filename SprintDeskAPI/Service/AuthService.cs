using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using SprintDeskAPI.DTOs;
using SprintDeskAPI.Interfaces.Repository;
using SprintDeskAPI.Interfaces.Service;
using SprintDeskAPI.Models;

namespace SprintDeskAPI.Service;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _repo;
    private readonly IConfiguration _config;
    private readonly IMapper _mapper;
    public AuthService(IAuthRepository repo, IConfiguration config, IMapper mapper)
    {
        _repo = repo;
        _config = config;
        _mapper = mapper;
    }

    public async Task<AuthResponseDto> Register(RegisterDto registerDto)
    {
         var find = await _repo.ExistUserByEmailAsync(registerDto.Email);
         if (find != null)
             throw new Exception("Email already exists");
         var user = _mapper.Map<User>(registerDto);
         
         user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);
         
         await _repo.Add(user);
         await _repo.SaveChangesAsync();
         
         var token = GenerateToken(user);

         return new AuthResponseDto
         {
             FullName = user.FullName,
             Email = user.Email,
             Role = user.Role,
             Token = token,
             CreatedAt = user.CreatedAt
         };
    }

    public async Task<AuthResponseDto> Login(LoginDto loginDto)
    {
         var find = await _repo.GetUserByEmailAsync(loginDto.Email);
         if (find == null)
             throw new Exception("Email or password is incorrect");
         
         var isValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, find.PasswordHash);
         if (!isValid)
             throw new Exception("Email or password is incorrect");
         
         var token = GenerateToken(find);
         
         return new AuthResponseDto
         {
             FullName = find.FullName,
             Email = find.Email,
             Role = find.Role,
             Token = token,
             CreatedAt = find.CreatedAt
         };
    }

    private string GenerateToken(User user)
    {
        var sec = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config ["Jwt:Key"]!));

        var cred = new SigningCredentials(sec, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
        };

        var token = new JwtSecurityToken
        (
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(10),
            signingCredentials: cred
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
