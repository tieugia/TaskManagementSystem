using TaskManagementSystem.Presentation.Middleware;

namespace TaskManagementSystem.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentationServices(this IServiceCollection services)
    {
        // Register services
        services.AddControllersWithViews();

        return services;
    }

    public static IApplicationBuilder ConfigurePresentation(this WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        
        app.UseMiddleware<ErrorHandlingMiddleware>();
        
        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseAuthorization();

        app.MapStaticAssets();

        app.MapControllers();

        app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Tasks}/{action=Index}")
            .WithStaticAssets();

        return app;
    }
}
