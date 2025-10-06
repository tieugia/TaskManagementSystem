using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Domain.Entities;

namespace TaskManagementSystem.Application.Mappers;

public static class EntityMappers
{
    public static TaskDto ToDto(this TaskItem task) =>
        new(task.Id, task.Title, task.Description, task.DueDate, task.IsCompleted, task.Priority, task.CreatedAtUtc, task.UpdatedAtUtc, task.RowVersion);
}
