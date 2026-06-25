using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SprintDeskAPI.DTOs;
using SprintDeskAPI.Interfaces.Service;

namespace SprintDeskAPI.Controllers;

[Authorize]
[ApiController]
[Route("api")]
public class TaskItemController : ControllerBase
{
    private readonly ITaskItemService _service;

    public TaskItemController(ITaskItemService service)
    {
        _service = service;
    }
    [HttpGet("tasks")]
    public async Task<IActionResult> GetPagedAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetPagesAsync(pageNumber, pageSize);

        return Ok(result);
    }
    [HttpGet("tasks/{id:int}")]
    public async Task<IActionResult> GetByIdAsync([FromRoute] int id)
    {
        var result = await _service.GetAsync(id);

        return Ok(result);
    }
    [HttpGet("tasks/my")]
    public async Task<IActionResult> GetMyTasksAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();

        var result = await _service.GetMyPagesByIdAsync(pageNumber, pageSize, userId);

        return Ok(result);
    }
    [HttpGet("projects/{projectId:int}/tasks")]
    public async Task<IActionResult> GetByProjectIdAsync(
        [FromRoute] int projectId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetByProjIdPagesAsync(pageNumber, pageSize, projectId);

        return Ok(result);
    }
    [Authorize(Roles = "Admin,Manager")]
    [HttpPost("tasks")]
    public async Task<IActionResult> CreateAsync([FromBody] CreateTaskDto dto)
    {
        var result = await _service.CreateAsync(dto);

        return Ok(result);
    }
    [Authorize(Roles = "Admin,Manager")]
    [HttpPut("tasks/{id:int}")]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute] int id,
        [FromBody] UpdateTaskDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);

        return Ok(result);
    }
    [HttpPatch("tasks/{id:int}/status")]
    public async Task<IActionResult> UpdateStatusAsync(
        [FromRoute] int id,
        [FromBody] UpdateTaskStatusDto dto)
    {
        var result = await _service.UpdateToStatusByIdAsync(id, dto);

        return Ok(result);
    }
    [Authorize(Roles = "Admin,Manager")]
    [HttpPatch("tasks/{id:int}/assign")]
    public async Task<IActionResult> AssignAsync(
        [FromRoute] int id,
        [FromBody] AssignTaskDto dto)
    {
        var result = await _service.UpdateToAssignByIdAsync(id, dto);

        return Ok(result);
    }
    [Authorize(Roles = "Admin,Manager")]
    [HttpDelete("tasks/{id:int}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] int id)
    {
        await _service.DeleteAsync(id);

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
}