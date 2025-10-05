using TaskManagementSystem.Application.DTOs;

namespace TaskManagementSystem.Application.Interfaces.Services;

public interface ITaskService
{
    Task<TaskDto> CreateTaskAsync(TaskCreateDto dto);
    Task<TaskDto> GetTaskByIdAsync(Guid id);
    Task DeleteTaskAsync(Guid id);
    Task UpdateTaskAsync(TaskUpdateDto dto);
    Task<IEnumerable<TaskDto>> SearchAsync(TaskSearchQueryDto query);
}
