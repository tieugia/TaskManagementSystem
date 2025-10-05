using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.Application.DTOs;

public sealed record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    DateTime? DueDate,
    bool IsCompleted,
    TaskPriority Priority,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    byte[] RowVersion
);

