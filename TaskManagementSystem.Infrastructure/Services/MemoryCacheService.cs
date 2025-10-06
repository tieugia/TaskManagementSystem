using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using TaskManagementSystem.Application.Interfaces.Services;

namespace TaskManagementSystem.Infrastructure.Services;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private static readonly ConcurrentDictionary<string, bool> _keys = new();

    public MemoryCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public void Set<T>(string key, T value, TimeSpan duration)
    {
        _memoryCache.Set(key, value, duration);
        _keys.TryAdd(key, true);
    }

    public bool TryGet<T>(string key, out T? value)
    {
        return _memoryCache.TryGetValue(key, out value);
    }

    public void Remove(string key)
    {
        _memoryCache.Remove(key);
        _keys.TryRemove(key, out _);
    }

    public void RemoveByPrefix(string prefix)
    {
        var toRemove = _keys.Keys.Where(k => k.StartsWith(prefix)).ToList();
        foreach (var key in toRemove)
        {
            _memoryCache.Remove(key);
            _keys.TryRemove(key, out _);
        }
    }
}