using System.ComponentModel.DataAnnotations;
using TaskManagementSystem.Common.Attributes;
using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.Presentation.Models;
public sealed class TaskUpdateViewModel
{
    [NotEmptyGuid] public Guid Id { get; init; }
    [StringLength(200)] public string Title { get; init; } = null!;
    [StringLength(2000)] public string? Description { get; init; }
    public DateTime? DueDate { get; init; }
    [Required] public TaskPriority Priority { get; init; }
    public bool IsCompleted { get; init; }
    public byte[] RowVersion { get; init; } = [];
}
