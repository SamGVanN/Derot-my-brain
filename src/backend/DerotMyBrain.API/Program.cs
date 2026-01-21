using Serilog;
using Microsoft.EntityFrameworkCore;

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
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuration
builder.Configuration["DataDirectory"] = Path.Combine(Directory.GetCurrentDirectory(), "Data");

// Service Registration (DI)
// builder.Services.AddScoped(typeof(DerotMyBrain.API.Repositories.IJsonRepository<>), typeof(DerotMyBrain.API.Repositories.JsonRepository<>)); // REMOVED
builder.Services.AddScoped<DerotMyBrain.API.Services.ICategoryService, DerotMyBrain.API.Services.CategoryService>();
builder.Services.AddScoped<DerotMyBrain.API.Services.IUserService, DerotMyBrain.API.Services.UserService>();

// Register initialization services as singletons
builder.Services.AddSingleton<DerotMyBrain.API.Services.ISeedDataService, DerotMyBrain.API.Services.SeedDataService>();
builder.Services.AddSingleton<DerotMyBrain.API.Services.IConfigurationService, DerotMyBrain.API.Services.ConfigurationService>();
builder.Services.AddSingleton<DerotMyBrain.API.Services.IInitializationService, DerotMyBrain.API.Services.InitializationService>();
builder.Services.AddHttpClient();

// Add DbContext for SQLite
// Only configure SQLite for non-test environments
// Test environment will configure DbContext via CustomWebApplicationFactory
if (!builder.Environment.IsEnvironment("Testing"))
{
    // Database path strategy:
    // - Development: Use project's Data folder (gitignored, survives rebuilds)
    // - Production: Use portable path relative to executable (user can move entire app folder)
    string dataPath;
    if (builder.Environment.IsDevelopment())
    {
        // Development: Project's Data folder
        var projectRoot = Directory.GetCurrentDirectory();
        dataPath = Path.Combine(projectRoot, "Data");
    }
    else
    {
        // Production: Portable - relative to executable
        var executablePath = AppDomain.CurrentDomain.BaseDirectory;
        dataPath = Path.Combine(executablePath, "data");
    }

    Directory.CreateDirectory(dataPath); // Ensure directory exists

    builder.Services.AddDbContext<DerotMyBrain.API.Data.DerotDbContext>(options =>
        options.UseSqlite($"Data Source={dataPath}/derot-my-brain.db"));
    
    Log.Information("Configured SQLite database at {DataPath}", dataPath);
}
else
{
    Log.Information("Running in Testing environment - DbContext will be configured by test infrastructure");
}

// Add Repositories
builder.Services.AddScoped<DerotMyBrain.API.Repositories.IActivityRepository, DerotMyBrain.API.Repositories.SqliteActivityRepository>();
builder.Services.AddScoped<DerotMyBrain.API.Repositories.IUserRepository, DerotMyBrain.API.Repositories.SqliteUserRepository>();
builder.Services.AddScoped<DerotMyBrain.API.Repositories.ITrackedTopicRepository, DerotMyBrain.API.Repositories.SqliteTrackedTopicRepository>();

// Add Services
builder.Services.AddScoped<DerotMyBrain.API.Services.IActivityService, DerotMyBrain.API.Services.ActivityService>();
builder.Services.AddScoped<DerotMyBrain.API.Services.ITrackedTopicService, DerotMyBrain.API.Services.TrackedTopicService>();


var app = builder.Build();

// Initialize application (seed data and configuration)
// Skip initialization in test environment
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<DerotMyBrain.API.Data.DerotDbContext>();
        
        // Auto-migration / creation
        context.Database.EnsureCreated();
        
        // Seed Data
        await DerotMyBrain.API.Data.DbInitializer.InitializeAsync(context, services.GetRequiredService<DerotMyBrain.API.Services.ICategoryService>());

        var initService = services.GetRequiredService<DerotMyBrain.API.Services.IInitializationService>();
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
    .WithOrigins("http://localhost:5173") // Vite default port
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseHttpsRedirection();

// Log startup
Log.Information("Application starting up");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapControllers();

// Run the application with error handling
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

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// Make Program class accessible for integration tests
public partial class Program { }

