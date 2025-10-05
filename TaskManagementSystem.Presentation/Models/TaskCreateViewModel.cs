using System.ComponentModel.DataAnnotations;
using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.Presentation.Models;
public sealed class TaskCreateViewModel
{
    [StringLength(200)] public string Title { get; init; } = null!;
    [StringLength(2000)] public string? Description { get; init; }
    [DataType(DataType.DateTime)] public DateTime? DueDate { get; init; }
    [Required] public TaskPriority Priority { get; init; }
}
