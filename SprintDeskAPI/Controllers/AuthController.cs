using Microsoft.AspNetCore.Mvc;
using SprintDeskAPI.DTOs;
using SprintDeskAPI.Interfaces.Service;

namespace SprintDeskAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> RegisterAsync([FromBody] RegisterDto dto)
    {
        var result = await _authService.Register(dto);

        return Ok(result);
    }
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> LoginAsync([FromBody] LoginDto dto)
    {
        var result = await _authService.Login(dto);
        return Ok(result);
    }
}