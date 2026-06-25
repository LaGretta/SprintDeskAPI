using AutoMapper;
using SprintDeskAPI.DTOs;
using SprintDeskAPI.Interfaces.Repository;
using SprintDeskAPI.Interfaces.Service;
using SprintDeskAPI.Models;

namespace SprintDeskAPI.Service;

public class TaskItemService : ITaskItemService
{
    private readonly ITaskItemRepository _taskItemRepository;
    private readonly IMapper _mapper;
    private readonly IAuthRepository _authRepository;
    private readonly IProjectRepository _projectRepository;

    public TaskItemService(
        ITaskItemRepository taskItemRepository
        , IMapper mapper
        , IProjectRepository projectRepository
        , IAuthRepository authRepository)
    {
        _taskItemRepository = taskItemRepository;
        _mapper = mapper;
        _projectRepository = projectRepository;
        _authRepository = authRepository;
    }

    public async Task<PagedResponseDto<TaskResponseDto>> GetPagesAsync(int pageNumber, int pageSize)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);
        
         var paged  = await _taskItemRepository.GetTaskItemPagedAsync(pageNumber, pageSize);
         var map = _mapper.Map<List<TaskResponseDto>>(paged.Items);

         return new PagedResponseDto<TaskResponseDto>
         {
             Items = map,
             PageNumber = pageNumber,
             PageSize = pageSize,
             TotalItems = paged.TotalCount,
             TotalPages = (int)Math.Ceiling((double)paged.TotalCount / pageSize)
         };
    }

    public async Task<TaskResponseDto> GetAsync(int id)
    {
         var find = await  _taskItemRepository.GetTaskItemByIdAsync(id);
         if (find == null)
             throw new Exception("TaskItem Not Found");
         return _mapper.Map<TaskResponseDto>(find);
    }

    public async Task<PagedResponseDto<TaskResponseDto>> GetMyPagesByIdAsync(int pageNumber, int pageSize, int userId)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);
        
        var paged  = await _taskItemRepository.GetMyTasksAsync(pageNumber, pageSize , userId);
        var map = _mapper.Map<List<TaskResponseDto>>(paged.Items);

        return new PagedResponseDto<TaskResponseDto>
        {
            Items = map,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = paged.TotalCount,
            TotalPages = (int)Math.Ceiling((double)paged.TotalCount / pageSize)
        };
    }

    public async Task<PagedResponseDto<TaskResponseDto>> GetByProjIdPagesAsync(int pageNumber, int pageSize, int projectId)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);
        
        var paged  = await _taskItemRepository.GetByProjectIdAsync(pageNumber, pageSize , projectId);
        var map = _mapper.Map<List<TaskResponseDto>>(paged.Items);

        return new PagedResponseDto<TaskResponseDto>
        {
            Items = map,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = paged.TotalCount,
            TotalPages = (int)Math.Ceiling((double)paged.TotalCount / pageSize)
        };
    }

    public async Task<TaskResponseDto> CreateAsync(CreateTaskDto dto)
    {
        var findproj = await _projectRepository.GetProjectByIdAsync(dto.ProjectId);
        if (findproj == null)
            throw new Exception("Project Not Found");
        
        if(dto.AssignedUserId == null)
            throw new Exception("Assigned User Not Found");

        if (dto.DueDate == null && dto.DueDate < DateTime.Now)
            throw new Exception("Due Date cannot be int the past");
        
        var taskItem = _mapper.Map<TaskItem>(dto);

        taskItem.Status = TaskItemStatus.ToDo;
        taskItem.CreatedAt = DateTime.UtcNow;
        taskItem.UpdatedAt = DateTime.UtcNow;
    
        await _taskItemRepository.CreateTaskItemAsync(taskItem);
        await _taskItemRepository.SaveChangesAsync();
        
        return _mapper.Map<TaskResponseDto>(taskItem);
    }

    public async Task<TaskResponseDto> UpdateAsync(int id, UpdateTaskDto dto)
    {
        var taskItem = await _taskItemRepository.GetTaskItemByIdAsync(id);

        if (taskItem == null)
            throw new Exception("Task Item Not Found");

        _mapper.Map(dto, taskItem);

        taskItem.UpdatedAt = DateTime.UtcNow;

        await _taskItemRepository.SaveChangesAsync();

        return _mapper.Map<TaskResponseDto>(taskItem);
    }

    public async Task<TaskResponseDto> UpdateToStatusByIdAsync(int id ,UpdateTaskStatusDto dto)
    {
        var taskItem = await _taskItemRepository.GetTaskItemByIdAsync(id);

        if (taskItem == null)
            throw new Exception("Task Item Not Found");

        taskItem.Status = dto.Status;
        taskItem.UpdatedAt = DateTime.UtcNow;
        
        await _taskItemRepository.SaveChangesAsync();
        return _mapper.Map<TaskResponseDto>(taskItem);
    }

    public async Task<TaskResponseDto> UpdateToAssignByIdAsync(int id, AssignTaskDto dto)
    {
        var taskItem = await _taskItemRepository.GetTaskItemByIdAsync(id);

        if (taskItem == null)
            throw new Exception("Task Item Not Found");

        taskItem.AssignedUserId = dto.AssignedUserId;
        taskItem.UpdatedAt = DateTime.UtcNow;

        await _taskItemRepository.SaveChangesAsync();

        return _mapper.Map<TaskResponseDto>(taskItem);
    }

    public async Task DeleteAsync(int id)
    {
         var find = await _taskItemRepository.GetTaskItemByIdAsync(id);
         
         if (find == null)
            throw new Exception("Task Item Not Found");
         
         _taskItemRepository.DeleteTaskItem(find);
         await _taskItemRepository.SaveChangesAsync();
    }
}