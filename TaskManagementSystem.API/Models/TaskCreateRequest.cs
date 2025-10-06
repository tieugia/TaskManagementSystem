using System.ComponentModel.DataAnnotations;
using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.API.Models;
public sealed class TaskCreateRequest
{
    [StringLength(200)] public string Title { get; init; } = null!;
    [StringLength(2000)] public string? Description { get; init; }
    public DateTime? DueDate { get; init; }
    [Required] public TaskPriority Priority { get; init; }
}
