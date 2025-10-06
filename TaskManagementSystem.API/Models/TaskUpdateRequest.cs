using System.ComponentModel.DataAnnotations;
using TaskManagementSystem.Common.Attributes;
using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.API.Models;
public sealed class TaskUpdateRequest
{
    [NotEmptyGuid] public Guid Id { get; init; }
    [StringLength(200)] public string Title { get; init; } = null!;
    [StringLength(2000)] public string? Description { get; init; }
    [DataType(DataType.DateTime)] public DateTime? DueDate { get; init; }
    [Required] public TaskPriority Priority { get; init; }
    public bool IsCompleted { get; init; }
    public byte[] RowVersion { get; init; } = [];
}
