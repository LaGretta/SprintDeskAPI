using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using SprintDeskAPI.DTOs;
using SprintDeskAPI.Interfaces.Repository;
using SprintDeskAPI.Interfaces.Service;
using SprintDeskAPI.Models;
using SprintDeskAPI.Service;

namespace SprintDeskAPI.Tests;

public class AuthServiceTests
{
    private readonly Mock<IAuthRepository> _authRepository;
    private readonly Mock<IMapper> _mapper;
    private readonly IConfiguration _configuration;
    private readonly IAuthService _service;

    public AuthServiceTests()
    {
        _authRepository = new Mock<IAuthRepository>();
        _mapper = new Mock<IMapper>();

        var settings = new Dictionary<string, string?>
        {
            { "Jwt:Key", "THIS_IS_SUPER_SECRET_TEST_KEY_FOR_SPRINT_DESK_API_123456789" },
            { "Jwt:Issuer", "SprintDeskAPI" },
            { "Jwt:Audience", "SprintDeskClient" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        _service = new AuthService(
            _authRepository.Object,
            _configuration,
            _mapper.Object
        );
    }
    [Fact]
    public async Task Register_WhenEmailAlreadyExists_ShouldThrowException()
    {
        var dto = new RegisterDto
        {
            FullName = "Test User",
            Email = "test@gmail.com",
            Password = "123456"
        };

        var existingUser = new User
        {
            Id = 1,
            FullName = "Test User",
            Email = "test@gmail.com",
            PasswordHash = "hash",
            Role = UserRole.Developer
        };

        _authRepository
            .Setup(r => r.ExistUserByEmailAsync(dto.Email))
            .ReturnsAsync(existingUser);

        var action = async () => await _service.Register(dto);

        await action.Should().ThrowAsync<Exception>();

        _authRepository.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
        _authRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
    [Fact]
    public async Task Login_WhenEmailNotFound_ShouldThrowException()
    {
        var dto = new LoginDto
        {
            Email = "notfound@gmail.com",
            Password = "123456"
        };

        _authRepository
            .Setup(r => r.GetUserByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null);

        var action = async () => await _service.Login(dto);

        await action.Should().ThrowAsync<Exception>();
    }
    [Fact]
    public async Task Login_WhenPasswordIsWrong_ShouldThrowException()
    {
        var dto = new LoginDto
        {
            Email = "test@gmail.com",
            Password = "wrong-password"
        };

        var user = new User
        {
            Id = 1,
            FullName = "Test User",
            Email = "test@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct-password"),
            Role = UserRole.Developer
        };
        _authRepository
            .Setup(r => r.GetUserByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        var action = async () => await _service.Login(dto);

        await action.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task Login_WhenCredentialsAreValid_ShouldReturnToken()
    {
        var password = "123456";

        var dto = new LoginDto
        {
            Email = "test@gmail.com",
            Password = password
        };

        var user = new User
        {
            Id = 1,
            FullName = "Test User",
            Email = "test@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = UserRole.Developer
        };

        _authRepository
            .Setup(r => r.GetUserByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        var result = await _service.Login(dto);

        result.Should().NotBeNull();
        result.Email.Should().Be("test@gmail.com");
        result.Token.Should().NotBeNullOrWhiteSpace();
    }
}