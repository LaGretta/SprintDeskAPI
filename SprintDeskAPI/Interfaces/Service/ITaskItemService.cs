using SprintDeskAPI.DTOs;

namespace SprintDeskAPI.Interfaces.Service;

public interface ITaskItemService
{
    Task<PagedResponseDto<TaskResponseDto>> GetPagesAsync(int pageNumber, int pageSize);
    Task<TaskResponseDto> GetAsync(int id);
    Task<PagedResponseDto<TaskResponseDto>> GetMyPagesByIdAsync(int pageNumber, int pageSize , int userId);
    Task<PagedResponseDto<TaskResponseDto>> GetByProjIdPagesAsync(int pageNumber, int pageSize , int projectId);

    Task<TaskResponseDto> CreateAsync(CreateTaskDto dto);
    Task<TaskResponseDto> UpdateAsync(int id, UpdateTaskDto dto);
    Task<TaskResponseDto> UpdateToStatusByIdAsync(int id ,UpdateTaskStatusDto dto);
    Task<TaskResponseDto> UpdateToAssignByIdAsync(int id  ,AssignTaskDto dto);
    
    Task DeleteAsync(int id);
}