using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.Application.DTOs;
public sealed record TaskCreateDto(
    string Title,
    string? Description,
    DateTime? DueDate,
    TaskPriority Priority
);
