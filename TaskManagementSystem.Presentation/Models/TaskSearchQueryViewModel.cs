using System.ComponentModel.DataAnnotations;
using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.Presentation.Models;
public sealed class TaskSearchQueryViewModel
{
    [StringLength(200)] public string? Keyword { get; init; }
    public bool? IsCompleted { get; init; }
    public TaskPriority? Priority { get; init; }
    [DataType(DataType.DateTime)] public DateTime? DueFromUtc { get; init; }
    [DataType(DataType.DateTime)] public DateTime? DueToUtc { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Page must be >= 1")]
    public int Page { get; init; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be 1..100")]
    public int PageSize { get; init; } = 20;

    public byte[] RowVersion { get; init; } = [];
}
