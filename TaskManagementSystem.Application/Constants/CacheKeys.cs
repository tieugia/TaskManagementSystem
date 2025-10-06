using TaskManagementSystem.Application.DTOs;

namespace TaskManagementSystem.Application.Constants;

public static class CacheKeys
{
    public static string Task(Guid id) => $"tasks:item:{id}";
    public static string TaskSearch(TaskSearchQueryDto q)
        => $"tasks:search:{q.Keyword}:{q.IsCompleted}:{q.Priority}:{q.Page}:{q.PageSize}:{q.DueFromUtc}:{q.DueToUtc}";
}

