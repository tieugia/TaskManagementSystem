using TaskManagementSystem.API;
using TaskManagementSystem.Application;
using TaskManagementSystem.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Add services to the container.
services.AddApiServices();
services.AddApplicationServices();
services.AddInfrastructureServices();

var app = builder.Build();

app.Configure();

app.Run();

public partial class Program;
