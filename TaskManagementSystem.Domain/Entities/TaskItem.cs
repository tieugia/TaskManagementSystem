using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.Domain.Entities;

public sealed class TaskItem: BaseEntity
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}