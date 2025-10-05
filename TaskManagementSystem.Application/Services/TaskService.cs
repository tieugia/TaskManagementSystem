using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Application.Interfaces.Repositories;
using TaskManagementSystem.Application.Interfaces.Services;
using TaskManagementSystem.Domain.Entities;

namespace TaskManagementSystem.Application.Services;

public sealed class TaskService(ITaskRepository taskRepository) : ITaskService
{
    public async Task<TaskDto> CreateTaskAsync(TaskCreateDto dto)
    {
        var task = new TaskItem
        {
            Title = dto.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description,
            DueDate = dto.DueDate,
            Priority = dto.Priority,
            IsCompleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        var addedTask = await taskRepository.AddAsync(task);
        return Map(addedTask);
    }

    public async Task UpdateTaskAsync(TaskUpdateDto taskUpdateDto)
    {
        var task = await taskRepository.GetByIdAsync(taskUpdateDto.Id);
        if (task == null) throw new KeyNotFoundException("Task not found");

        task.Title = taskUpdateDto.Title.Trim();
        task.Description = string.IsNullOrWhiteSpace(taskUpdateDto.Description) ? null : taskUpdateDto.Description;
        task.DueDate = taskUpdateDto.DueDate;
        task.Priority = taskUpdateDto.Priority;
        task.IsCompleted = taskUpdateDto.IsCompleted;
        task.UpdatedAtUtc = DateTime.UtcNow;

        task.RowVersion = taskUpdateDto.RowVersion;

        await taskRepository.UpdateAsync(task);
    }

    public async Task DeleteTaskAsync(Guid id)
    {
        var task = await taskRepository.GetByIdAsync(id);
        if (task == null) throw new KeyNotFoundException("Task not found");

        await taskRepository.DeleteAsync(task);
    }

    public async Task<TaskDto> GetTaskByIdAsync(Guid id)
    {
        var task = await taskRepository.GetByIdAsync(id);
        if (task == null) throw new KeyNotFoundException("Task not found");

        return Map(task);
    }

    public async Task<IEnumerable<TaskDto>> SearchAsync(TaskSearchQueryDto query)
    {
        var items = await taskRepository.SearchAsync(query);

        return items.Select(Map);
    }

    private static TaskDto Map(TaskItem task) =>
        new(task.Id, task.Title, task.Description, task.DueDate, task.IsCompleted, task.Priority, task.CreatedAtUtc, task.UpdatedAtUtc, task.RowVersion);
}
