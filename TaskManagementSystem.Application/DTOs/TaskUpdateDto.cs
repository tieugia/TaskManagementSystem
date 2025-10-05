using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.Application.DTOs;

public sealed record TaskUpdateDto(
    Guid Id,
    string Title,
    string? Description,
    DateTime? DueDate,
    TaskPriority Priority,
    bool IsCompleted,
    byte[] RowVersion
);
