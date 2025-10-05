using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.Application.DTOs;

public sealed record TaskSearchQueryDto(
    string? Keyword,
    bool? IsCompleted,
    TaskPriority? Priority,
    DateTime? DueFromUtc,
    DateTime? DueToUtc,
    int Page,
    int PageSize,
    byte[] RowVersion
);