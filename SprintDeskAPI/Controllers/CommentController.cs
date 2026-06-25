using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SprintDeskAPI.DTOs;
using SprintDeskAPI.Interfaces.Repository;
using SprintDeskAPI.Interfaces.Service;
using SprintDeskAPI.Models;

namespace SprintDeskAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CommentController : ControllerBase
{
   private readonly ICommentService _commentService;

   public CommentController(ICommentService commentService)
   {
      _commentService = commentService;
   }
   
   [HttpGet("tasks/{taskId:int}/comments")]
   public async Task<IActionResult> GetByTaskIdAsync([FromRoute] int taskId)
   {
      var result = await _commentService.GetByTaskIdAsync(taskId);
      return Ok(result);
   }

   [HttpPost("tasks/{taskId:int}/comments")]
   public async Task<IActionResult> CreateAsync(
      [FromRoute] int taskId,
      [FromBody] CreateCommentDto dto)
   {
      var userId = GetUserId();
      var result = await _commentService.CreateAsync(taskId, userId, dto);
      return Ok(result);
   }
   
   [HttpDelete("comments/{id:int}")]
   public async Task<IActionResult> DeleteAsync([FromRoute] int id)
   {
      var userId = GetUserId();
      var role = GetUserRole();

      await _commentService.DeleteAsync(id, userId, role);

      return NoContent();
   }

   
   
   private int GetUserId()
   {
      var userIdValue =
         User.FindFirst(ClaimTypes.NameIdentifier)?.Value
         ?? User.FindFirst("id")?.Value
         ?? User.FindFirst("userId")?.Value;

      if (!int.TryParse(userIdValue, out var userId))
         throw new Exception("User id not found in token");

      return userId;
   }
   private UserRole GetUserRole()
   {
      var roleValue =
         User.FindFirst(ClaimTypes.Role)?.Value
         ?? User.FindFirst("role")?.Value;

      if (!Enum.TryParse<UserRole>(roleValue, out var role))
         throw new Exception("User role not found in token");

      return role;
   }
}