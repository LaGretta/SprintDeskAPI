using AutoMapper;
using FluentAssertions;
using Moq;
using SprintDeskAPI.DTOs;
using SprintDeskAPI.Interfaces.Repository;
using SprintDeskAPI.Interfaces.Service;
using SprintDeskAPI.Models;
using SprintDeskAPI.Service;

namespace SprintDeskAPI.Tests;

public class TaskItemServiceTests
{
    private readonly Mock<ITaskItemRepository> _taskItemRepository;
    private readonly Mock<IProjectRepository> _projectRepository;
    private readonly Mock<IAuthRepository> _authRepository;
    private readonly Mock<IMapper> _mapper;

    private readonly ITaskItemService _service;

    public TaskItemServiceTests()
    {
        _taskItemRepository = new Mock<ITaskItemRepository>();
        _projectRepository = new Mock<IProjectRepository>();
        _authRepository = new Mock<IAuthRepository>();
        _mapper = new Mock<IMapper>();

        _service = new TaskItemService(
            _taskItemRepository.Object,
            _mapper.Object,
            _projectRepository.Object,
            _authRepository.Object
        );
    }
    [Fact]
    public async Task GetAsync_WhenTaskItemNotFound_ShouldThrowException()
    {
        var taskId = 1;

        _taskItemRepository.Setup(n => n.GetTaskItemByIdAsync(taskId))
            .ReturnsAsync((TaskItem?)null);
        
        var result = async () => await _service.GetAsync(taskId);

        await result.Should().ThrowAsync<Exception>();
    }
    [Fact]
    public async Task CreateAsync_WhenProjectNotFound_ShouldThrowException()
    {
        // Arrange
        var dto = new CreateTaskDto
        {
            Title = "Test Task",
            Description = "Test Description",
            ProjectId = 1,
            AssignedUserId = null,
            Priority = TaskPriority.Medium,
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        _projectRepository
            .Setup(r => r.GetProjectByIdAsync(dto.ProjectId))
            .ReturnsAsync((Project?)null);

        var action = async () => await _service.CreateAsync(dto);

        await action.Should().ThrowAsync<Exception>();
    }
    [Fact]
    public async Task UpdateToStatusByIdAsync_WhenTaskItemExists_ShouldUpdateStatus()
    {
        var taskId = 1;

        var dto = new UpdateTaskStatusDto
        {
            Status = TaskItemStatus.Done
        };
        var taskItem = new TaskItem
        {
            Id = taskId,
            Title = "Test Task",
            Description = "Test Description",
            Status = TaskItemStatus.ToDo,
            Priority = TaskPriority.Medium
        };

        var responseDto = new TaskResponseDto
        {
            Id = taskId,
            Title = "Test Task",
            Description = "Test Description",
            Status = TaskItemStatus.Done,
            Priority = TaskPriority.Medium
        };

        _taskItemRepository
            .Setup(r => r.GetTaskItemByIdAsync(taskId))
            .ReturnsAsync(taskItem);

        _mapper
            .Setup(m => m.Map<TaskResponseDto>(taskItem))
            .Returns(responseDto);

        var result = await _service.UpdateToStatusByIdAsync(taskId, dto);

        taskItem.Status.Should().Be(TaskItemStatus.Done);
        result.Status.Should().Be(TaskItemStatus.Done);

        _taskItemRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
    [Fact]
    public async Task DeleteAsync_WhenTaskItemExists_ShouldDeleteTaskItem()
    {
        var taskId = 1;

        var taskItem = new TaskItem
        {
            Id = taskId,
            Title = "Test Task",
            Description = "Test Description",
            Status = TaskItemStatus.ToDo,
            Priority = TaskPriority.Medium
        };

        _taskItemRepository
            .Setup(r => r.GetTaskItemByIdAsync(taskId))
            .ReturnsAsync(taskItem);

        await _service.DeleteAsync(taskId);

        _taskItemRepository.Verify(r => r.DeleteTaskItem(taskItem), Times.Once);
        _taskItemRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}