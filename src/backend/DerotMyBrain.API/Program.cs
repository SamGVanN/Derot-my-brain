using Serilog;
using DerotMyBrain.Infrastructure.Data;
using DerotMyBrain.Core.Interfaces.Services;
using DerotMyBrain.Core.Services;
using DerotMyBrain.Infrastructure.Services;
using DerotMyBrain.API.Extensions;
using Microsoft.EntityFrameworkCore; // Added for Migrate()

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(
        path: "Logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

// Build the application
var builder = WebApplication.CreateBuilder(args);

// Use Serilog
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();

// 5. Documentation
builder.Services.AddDocumentationServices();

// --- Security Configuration ---

// --- Clean Architecture Registration ---

// 1. Data Access (Infrastructure)
builder.Services.AddDataServices(builder.Configuration, builder.Environment);

// 2. Identity & Security
builder.Services.AddIdentityServices(builder.Configuration);

// 3. Infrastructure (External Services)
builder.Services.AddInfrastructureServices(builder.Configuration);

// 4. Application (Core Domain)
builder.Services.AddApplicationServices(builder.Configuration);


var app = builder.Build();

// Initialize application
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<DerotDbContext>();
        
        // Auto-migration / creation
        context.Database.Migrate();
        
        // Seed Data
        await DerotMyBrain.Infrastructure.Data.DbInitializer.InitializeAsync(context, services.GetRequiredService<ICategoryService>());

        var initService = services.GetRequiredService<IInitializationService>();
        await initService.InitializeAsync();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(policy => policy
    .WithOrigins("http://localhost:5173") 
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseRateLimiter(); // Add Rate Limiting Middleware
app.UseAuthentication(); // Add Auth Middleware
app.UseAuthorization(); // Add Authorization Middleware

// Security Headers Middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    await next();
});

app.UseHttpsRedirection();

Log.Information("Application starting up");

app.MapControllers();

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
