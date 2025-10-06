using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagementSystem.Application.Interfaces.Repositories;
using TaskManagementSystem.Application.Interfaces.Services;
using TaskManagementSystem.Infrastructure.Persistences;
using TaskManagementSystem.Infrastructure.Repositories;
using TaskManagementSystem.Infrastructure.Services;

namespace TaskManagementSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddDbContext<TaskManagementDbContext>(opt => opt.UseInMemoryDatabase("CustomerDb_Dev"));

        // Register caching services
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();

        // Register task repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<ITaskRepository, TaskRepository>();

        return services;
    }
}
