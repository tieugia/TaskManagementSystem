using Microsoft.EntityFrameworkCore;
using System.Net;

namespace TaskManagementSystem.Presentation.Middleware;

public class ErrorHandlingMiddleware(
    RequestDelegate next,
    ILogger<ErrorHandlingMiddleware> logger,
    IHostEnvironment env)
{
    private readonly IHostEnvironment _env = env;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "Resource not found.");
            await HandleNotFoundAsync(context, ex);
        }
        catch (DbUpdateConcurrencyException)
        {
            await HandleConflictAsync(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleNotFoundAsync(HttpContext context, KeyNotFoundException ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;

        var result = new { message = "Resource not found.", detail = ex.Message };
        return context.Response.WriteAsJsonAsync(result);
    }
    
    private static Task HandleConflictAsync(HttpContext context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Conflict;
        return context.Response.WriteAsync("Conflict: The resource was updated by another user.");
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var result = new
        {
            message = "An internal server error occurred.",
            detail = ex.Message
        };
        return context.Response.WriteAsJsonAsync(result);
    }
}
