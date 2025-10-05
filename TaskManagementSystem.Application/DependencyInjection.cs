using Microsoft.Extensions.DependencyInjection;
using TaskManagementSystem.Application.Interfaces.Services;
using TaskManagementSystem.Application.Services;

namespace TaskManagementSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register task services
        services.AddScoped<ITaskService, TaskService>();

        return services;
    }
}
