using AutoMapper;
using FluentAssertions;
using Moq;
using SprintDeskAPI.DTOs;
using SprintDeskAPI.Interfaces.Repository;
using SprintDeskAPI.Interfaces.Service;
using SprintDeskAPI.Models;
using SprintDeskAPI.Service;

namespace SprintDeskAPI.Tests;

public class CommentServiceTests
{
    private readonly Mock<ICommentRepository> _commentRepository;
    private readonly Mock<IMapper> _mapper;
    private readonly ICommentService _service;

    public CommentServiceTests()
    {
        _commentRepository = new Mock<ICommentRepository>();
        _mapper = new Mock<IMapper>();

        _service = new CommentService(
            _commentRepository.Object
            , _mapper.Object
        );
    }
    [Fact]
    public async Task CreateAsync_WhenDtoIsValid_ShouldCreateComment()
    {
        var taskId = 1;
        var userId = 10;

        var dto = new CreateCommentDto
        {
            Text = "Test comment"
        };
        var comment = new Comment
        {
            Id = 1,
            Text = "Test comment"
        };
        var responseDto = new CommentResponseDto
        {
            Id = 1,
            Text = "Test comment",
            TaskItemId = taskId,
            UserId = userId
        };

        _mapper
            .Setup(m => m.Map<Comment>(dto))
            .Returns(comment);

        _mapper
            .Setup(m => m.Map<CommentResponseDto>(comment))
            .Returns(responseDto);

        var result = await _service.CreateAsync(taskId, userId, dto);

        result.Should().NotBeNull();
        result.Text.Should().Be("Test comment");
        result.TaskItemId.Should().Be(taskId);
        result.UserId.Should().Be(userId);

        comment.TaskItemId.Should().Be(taskId);
        comment.UserId.Should().Be(userId);

        _commentRepository.Verify(r => r.AddCommentAsync(comment), Times.Once);
        _commentRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenCommentNotFound_ShouldThrowException()
    {
        var commentId = 1;
        var userId = 10;
        var role = UserRole.Developer;
        
        _commentRepository.Setup(n => n.GetCommentByIdAsync(commentId))
            .ReturnsAsync((Comment?)null);
        
        var action = async  () => await _service.DeleteAsync(commentId, userId, role);
        
        await action.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task DeleteAsync_WhenUserIsNotOwnerAndNotAdmin_ShouldThrowException()
    {
        var commentId = 1;
        var commentOwnerId = 10;
        var currentUserId = 99;

        var comment = new Comment
        {
            Id = commentId,
            Text = "Test comment",
            UserId = commentOwnerId
        };

        _commentRepository
            .Setup(r => r.GetCommentByIdAsync(commentId))
            .ReturnsAsync(comment);

        var action = async () => await _service.DeleteAsync(
            commentId,
            currentUserId,
            UserRole.Developer
        );

        await action.Should().ThrowAsync<Exception>();

        _commentRepository.Verify(r => r.DeleteCommentAsync(comment), Times.Never);
        _commentRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
    [Fact]
    public async Task DeleteAsync_WhenUserIsAdmin_ShouldDeleteComment()
    {
        var commentId = 1;
        var commentOwnerId = 10;
        var adminUserId = 99;

        var comment = new Comment
        {
            Id = commentId,
            Text = "Test comment",
            UserId = commentOwnerId
        };

        _commentRepository
            .Setup(r => r.GetCommentByIdAsync(commentId))
            .ReturnsAsync(comment);

        await _service.DeleteAsync(commentId, adminUserId, UserRole.Admin);

        _commentRepository.Verify(r => r.DeleteCommentAsync(comment), Times.Once);
        _commentRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}