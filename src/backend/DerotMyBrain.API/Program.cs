using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(
        path: "Logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
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
    builder.Services.AddScoped(typeof(DerotMyBrain.API.Repositories.IJsonRepository<>), typeof(DerotMyBrain.API.Repositories.JsonRepository<>));
    builder.Services.AddScoped<DerotMyBrain.API.Services.ICategoryService, DerotMyBrain.API.Services.CategoryService>();
    builder.Services.AddScoped<DerotMyBrain.API.Services.IUserService, DerotMyBrain.API.Services.UserService>();

    // Register initialization services as singletons
    builder.Services.AddSingleton<DerotMyBrain.API.Services.ISeedDataService, DerotMyBrain.API.Services.SeedDataService>();
    builder.Services.AddSingleton<DerotMyBrain.API.Services.IConfigurationService, DerotMyBrain.API.Services.ConfigurationService>();
    builder.Services.AddSingleton<DerotMyBrain.API.Services.IInitializationService, DerotMyBrain.API.Services.InitializationService>();
    builder.Services.AddHttpClient();

    var app = builder.Build();

    // Initialize application (seed data and configuration)
    using (var scope = app.Services.CreateScope())
    {
        var initService = scope.ServiceProvider.GetRequiredService<DerotMyBrain.API.Services.IInitializationService>();
        await initService.InitializeAsync();
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
