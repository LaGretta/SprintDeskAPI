using AutoMapper;
using FluentAssertions;
using Moq;
using SprintDeskAPI.DTOs;
using SprintDeskAPI.Interfaces.Repository;
using SprintDeskAPI.Interfaces.Service;
using SprintDeskAPI.Models;
using SprintDeskAPI.Service;

namespace SprintDeskAPI.Tests;

public class ProjectServiceTests
{
    private readonly Mock<IProjectRepository>  _projectRepository;
    private readonly Mock<IMapper>  _mapper;
    private readonly IProjectService _service;
    
    public ProjectServiceTests()
    {
        _projectRepository = new Mock<IProjectRepository>();
        _mapper = new Mock<IMapper>();
        
        _service = new ProjectService
            (
                _projectRepository.Object
                , _mapper.Object
            );
    }
    [Fact]
    public async Task GetByIdAsync_WhenProjectNotFound_ShouldThrowException()
    {
        var projectId = 1;

        _projectRepository
            .Setup(r => r.GetProjectByIdAsync(projectId))
            .ReturnsAsync((Project?)null);

        var action = async () => await _service.GetById(projectId);

        await action.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task GetById_WhenProjectExists_ShouldReturnProject()
    {
        var projectId = 1;

        var project = new Project
        {
            Id = projectId,
            Name = "Test Project",
            Description = "Test Description",
            Status = ProjectStatus.Active
        };

        var responseDto = new ProjectResponseDto
        {
            Id = projectId,
            Name = "Test Project",
            Description = "Test Description",
            Status = ProjectStatus.Active
        };

        _projectRepository
            .Setup(r => r.GetProjectByIdAsync(projectId))
            .ReturnsAsync(project);

        _mapper
            .Setup(m => m.Map<ProjectResponseDto>(project))
            .Returns(responseDto);
        
        var result = await _service.GetById(projectId);
        result.Should().NotBeNull();
        result.Id.Should().Be(projectId);
        result.Name.Should().Be("Test Project");
    }

    [Fact]
    public async Task CreateProject_WhenDtoIsValid_ShouldCreateProject()
    {
        // Arrange
        var dto = new CreateProjectDto
        {
            Name = "Test Name",
            Description = "Test Description"
        };

        var project = new Project
        {
            Id = 1,
            Name = "Test Name",
            Description = "Test Description",
            Status = ProjectStatus.Active
        };

        var responseDto = new ProjectResponseDto
        {
            Id = 1,
            Name = "Test Name",
            Description = "Test Description",
            Status = ProjectStatus.Active
        };

        _mapper
            .Setup(m => m.Map<Project>(dto))
            .Returns(project);

        _mapper
            .Setup(m => m.Map<ProjectResponseDto>(project))
            .Returns(responseDto);

        var result = await _service.CreateProject(dto);

        result.Should().NotBeNull();
        result.Name.Should().Be("Test Name");
        result.Description.Should().Be("Test Description");
        result.Status.Should().Be(ProjectStatus.Active);

        _projectRepository.Verify(r => r.CreateProjectAsync(project), Times.Once);
        _projectRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CompleteAsync_WhenProjectExists_ShouldSetStatusCompleted()
    { 
        var projectId = 1;

        var project = new Project
        {
            Id = projectId,
            Name = "Test Project",
            Description = "Test Description",
            Status = ProjectStatus.Active
        };

        var responseDto = new ProjectResponseDto
        {
            Id = projectId,
            Name = "Test Project",
            Description = "Test Description",
            Status = ProjectStatus.Completed
        };
        _projectRepository.Setup(n => n.GetProjectByIdAsync(projectId))
            .ReturnsAsync(project);
        
        _mapper.Setup(m => m.Map<ProjectResponseDto>(project))
            .Returns(responseDto);
        
        var result = await _service.CompleteAsync(projectId);
        
        result.Should().NotBeNull();
        result.Status.Should().Be(ProjectStatus.Completed);
        project.Status.Should().Be(ProjectStatus.Completed);
        
        _projectRepository.Verify(n => n.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteProjectById_WhenProjectExists_ShouldDeleteProject()
    {
        var projectId = 1;

        var project = new Project
        {
            Id = projectId,
            Name = "Test Project",
            Description = "Test Description",
            Status = ProjectStatus.Active
        };
        _projectRepository.Setup(n => n.GetProjectByIdAsync(projectId))
            .ReturnsAsync(project);
        
        await _service.DeleteProjectById(projectId);
        
        _projectRepository.Verify(n => n.DeleteProjectAsync(project), Times.Once);
        _projectRepository.Verify(n => n.SaveChangesAsync(), Times.Once);
    }
}
