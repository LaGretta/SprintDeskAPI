using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SprintDeskAPI.DTOs;
using SprintDeskAPI.Interfaces.Service;
using SprintDeskAPI.Models;

namespace SprintDeskAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProjectController : ControllerBase
{
    private readonly IProjectService _service;

    public ProjectController(IProjectService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetPagedAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetPaged(pageNumber, pageSize);
        return Ok(result);
    }
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync([FromRoute] int id)
    {
        var result = await _service.GetById(id);
        return Ok(result);
    }
    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    public async Task<IActionResult> CreateProjectAsync([FromBody] CreateProjectDto dto)
    {
        var result = await _service.CreateProject(dto);

        return Ok(result);
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProjectByIdAsync(
        [FromRoute] int id,
        [FromBody] UpdateProjectDto dto)
    {
        var result = await _service.UpdateProjectById(id, dto);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPatch("{id:int}/complete")]
    public async Task<IActionResult> CompleteAsync([FromRoute] int id)
    {
        var result = await _service.CompleteAsync(id);

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:int}/archive")]
    public async Task<IActionResult> ArchiveAsync([FromRoute] int id)
    {
        var result = await _service.ArchiveAsync(id);

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProjectByIdAsync([FromRoute] int id)
    {
        await _service.DeleteProjectById(id);

        return NoContent();
    }
}