using TaskManagementSystem.Application.Constants;
using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Application.Interfaces.Repositories;
using TaskManagementSystem.Application.Interfaces.Services;
using TaskManagementSystem.Application.Mappers;
using TaskManagementSystem.Domain.Entities;

namespace TaskManagementSystem.Application.Services;

public sealed class TaskService(
    ITaskRepository taskRepository,
    ICacheService cacheService) : ITaskService
{
    public async Task<TaskDto> CreateTaskAsync(TaskCreateDto dto)
    {
        var task = new TaskItem
        {
            Title = dto.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            DueDate = dto.DueDate,
            Priority = dto.Priority,
            IsCompleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        var addedTask = await taskRepository.AddAsync(task);

        cacheService.RemoveByPrefix("tasks:search:");

        return addedTask.ToDto();
    }

    public async Task UpdateTaskAsync(TaskUpdateDto taskUpdateDto)
    {
        var task = await taskRepository.GetByIdAsync(taskUpdateDto.Id);
        if (task == null)
            throw new KeyNotFoundException("Task not found");

        task.Title = taskUpdateDto.Title.Trim();
        task.Description = string.IsNullOrWhiteSpace(taskUpdateDto.Description)
            ? null
            : taskUpdateDto.Description.Trim();
        task.DueDate = taskUpdateDto.DueDate;
        task.Priority = taskUpdateDto.Priority;
        task.IsCompleted = taskUpdateDto.IsCompleted;
        task.UpdatedAtUtc = DateTime.UtcNow;
        task.RowVersion = taskUpdateDto.RowVersion;

        await taskRepository.UpdateAsync(task);

        cacheService.Remove(CacheKeys.Task(task.Id));
        cacheService.RemoveByPrefix("tasks:search:");
    }

    public async Task DeleteTaskAsync(Guid id)
    {
        var task = await taskRepository.GetByIdAsync(id);
        if (task == null)
            throw new KeyNotFoundException("Task not found");

        await taskRepository.DeleteAsync(task);

        cacheService.Remove(CacheKeys.Task(id));
        cacheService.RemoveByPrefix("tasks:search:");
    }

    public async Task<TaskDto> GetTaskByIdAsync(Guid id)
    {
        var cacheKey = CacheKeys.Task(id);
        if (cacheService.TryGet(cacheKey, out TaskDto? cached) && cached is not null)
            return cached;

        var task = await taskRepository.GetByIdAsync(id);
        if (task == null)
            throw new KeyNotFoundException("Task not found");

        var dto = task.ToDto();
        cacheService.Set(cacheKey, dto, TimeSpan.FromHours(1));
        return dto;
    }

    public async Task<IEnumerable<TaskDto>> SearchAsync(TaskSearchQueryDto query)
    {
        var cacheKey = CacheKeys.TaskSearch(query);
        if (cacheService.TryGet(cacheKey, out IEnumerable<TaskDto>? cachedList) && cachedList is not null)
            return cachedList;

        var items = await taskRepository.SearchAsync(query);
        var result = items.Select(item => item.ToDto()).ToList();

        cacheService.Set(cacheKey, result, TimeSpan.FromHours(1));
        return result;
    }
}