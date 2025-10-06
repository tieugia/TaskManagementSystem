namespace TaskManagementSystem.Application.Interfaces.Services;
public interface ICacheService
{
    void Set<T>(string key, T value, TimeSpan duration);
    bool TryGet<T>(string key, out T? value);
    void Remove(string key);
    void RemoveByPrefix(string prefix);
}
