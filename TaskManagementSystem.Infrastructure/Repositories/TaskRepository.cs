using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Application.Interfaces.Repositories;
using TaskManagementSystem.Domain.Entities;
using TaskManagementSystem.Infrastructure.Persistences;

namespace TaskManagementSystem.Infrastructure.Repositories;

public sealed class TaskRepository(TaskManagementDbContext context)
    : GenericRepository<TaskItem>(context), ITaskRepository
{
    public async Task<IReadOnlyList<TaskItem>> SearchAsync(TaskSearchQueryDto taskSearchQueryDto)
    {
        IQueryable<TaskItem> taskQuery = _context.Tasks.AsNoTracking();

        // Filtering
        if (!string.IsNullOrWhiteSpace(taskSearchQueryDto.Keyword))
        {
            var keyword = taskSearchQueryDto.Keyword.Trim();
            taskQuery = taskQuery.Where(t => EF.Functions.Like(t.Title, $"%{keyword}%")
                          || EF.Functions.Like(t.Description ?? "", $"%{keyword}%"));
        }
        if (taskSearchQueryDto.IsCompleted.HasValue) 
            taskQuery = taskQuery.Where(t => t.IsCompleted == taskSearchQueryDto.IsCompleted);
        
        if (taskSearchQueryDto.Priority.HasValue)
            taskQuery = taskQuery.Where(t => t.Priority == taskSearchQueryDto.Priority);
        
        if (taskSearchQueryDto.DueFromUtc.HasValue)
            taskQuery = taskQuery.Where(t => t.DueDate >= taskSearchQueryDto.DueFromUtc);
        
        if (taskSearchQueryDto.DueToUtc.HasValue)
            taskQuery = taskQuery.Where(t => t.DueDate <= taskSearchQueryDto.DueToUtc);

        var items = await taskQuery.OrderByDescending(t => t.UpdatedAtUtc)
                           .ThenByDescending(t => t.Priority)
                           .Skip((taskSearchQueryDto.Page - 1) * taskSearchQueryDto.PageSize)
                           .Take(taskSearchQueryDto.PageSize)
                           .ToListAsync();
        return items;
    }
}
