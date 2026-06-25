using SprintDeskAPI.DTOs;

namespace SprintDeskAPI.Interfaces.Service;

public interface IAuthService
{
    Task<AuthResponseDto> Register(RegisterDto registerDto);
    Task<AuthResponseDto> Login(LoginDto loginDto);
}