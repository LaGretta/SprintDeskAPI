using AutoMapper;
using SprintDeskAPI.DTOs;
using SprintDeskAPI.Models;

namespace SprintDeskAPI.Mapping;

public class AuthMapping : Profile
{
    public AuthMapping()
    {
        CreateMap<RegisterDto, User>();
        CreateMap<User, AuthResponseDto>();
    }
}