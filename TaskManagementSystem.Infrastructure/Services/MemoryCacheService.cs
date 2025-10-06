using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using TaskManagementSystem.Application.Interfaces.Services;

namespace TaskManagementSystem.Infrastructure.Services;

public class MemoryCacheService(IMemoryCache memoryCache) : ICacheService
{
    private static readonly ConcurrentDictionary<string, bool> Keys = new();

    public void Set<T>(string key, T value, TimeSpan duration)
    {
        memoryCache.Set(key, value, duration);
        Keys.TryAdd(key, true);
    }

    public bool TryGet<T>(string key, out T? value)
    {
        return memoryCache.TryGetValue(key, out value);
    }

    public void Remove(string key)
    {
        memoryCache.Remove(key);
        Keys.TryRemove(key, out _);
    }

    public void RemoveByPrefix(string prefix)
    {
        var toRemove = Keys.Keys.Where(k => k.StartsWith(prefix)).ToList();
        foreach (var key in toRemove)
        {
            memoryCache.Remove(key);
            Keys.TryRemove(key, out _);
        }
    }
}