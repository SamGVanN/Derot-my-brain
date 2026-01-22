using Serilog;
using Microsoft.EntityFrameworkCore;
using DerotMyBrain.Infrastructure.Data;
using DerotMyBrain.Core.Interfaces.Repositories;
using DerotMyBrain.Infrastructure.Repositories;
using DerotMyBrain.Core.Interfaces.Services;
using DerotMyBrain.Core.Services;
using DerotMyBrain.Infrastructure.Services;

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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuration
builder.Configuration["DataDirectory"] = Path.Combine(Directory.GetCurrentDirectory(), "Data");

// --- Clean Architecture Registration ---

// 1. Data Access (Infrastructure)
if (!builder.Environment.IsEnvironment("Testing"))
{
    string dataPath;
    if (builder.Environment.IsDevelopment())
    {
        var projectRoot = Directory.GetCurrentDirectory();
        dataPath = Path.Combine(projectRoot, "Data");
    }
    else
    {
        var executablePath = AppDomain.CurrentDomain.BaseDirectory;
        dataPath = Path.Combine(executablePath, "data");
    }

    Directory.CreateDirectory(dataPath);

    builder.Services.AddDbContext<DerotDbContext>(options =>
        options.UseSqlite($"Data Source={dataPath}/derot-my-brain.db"));
    
    Log.Information("Configured SQLite database at {DataPath}", dataPath);
}

builder.Services.AddScoped<IUserRepository, SqliteUserRepository>();
builder.Services.AddScoped<IActivityRepository, SqliteActivityRepository>();
builder.Services.AddScoped<ITrackedTopicRepository, SqliteTrackedTopicRepository>();

// 2. External Services (Infrastructure)
builder.Services.AddHttpClient();
builder.Services.AddScoped<ICategoryService, DerotMyBrain.Infrastructure.Services.CategoryService>();

// Register Content Sources as a collection
builder.Services.AddScoped<IContentSource, WikipediaContentSource>();
builder.Services.AddScoped<IContentSource, FileContentSource>();

// Register LLM Service
builder.Services.AddScoped<ILlmService, OllamaLlmService>();

// 3. Domain Services (Core)
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<ITrackedTopicService, TrackedTopicService>();

// Legacy / Helper Services (Keep API bound for now if they are purely API concern, e.g. SeedData)
// Need to verify if SeedDataService exists in API. Yes it does.
// Assuming SeedDataService needs to be refactored or stays in API calling Core services.
// I'll assume we keep it for now.
builder.Services.AddSingleton<DerotMyBrain.API.Services.ISeedDataService, DerotMyBrain.API.Services.SeedDataService>();
builder.Services.AddSingleton<DerotMyBrain.API.Services.IConfigurationService, DerotMyBrain.API.Services.ConfigurationService>();
builder.Services.AddSingleton<DerotMyBrain.API.Services.IInitializationService, DerotMyBrain.API.Services.InitializationService>();


var app = builder.Build();

// Initialize application
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<DerotDbContext>();
        
        // Auto-migration / creation
        context.Database.EnsureCreated();
        
        // Seed Data
        await DerotMyBrain.API.Data.DbInitializer.InitializeAsync(context, services.GetRequiredService<ICategoryService>());

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
    .WithOrigins("http://localhost:5173") 
    .AllowAnyMethod()
    .AllowAnyHeader());

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
