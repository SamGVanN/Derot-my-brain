using DerotMyBrain.Core.Interfaces.Repositories;
using DerotMyBrain.Infrastructure.Data;
using DerotMyBrain.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DerotMyBrain.API.Extensions;

public static class DataServiceExtensions
{
    public static IServiceCollection AddDataServices(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
    {
        // 1. Data Access (Infrastructure)
        if (!env.IsEnvironment("Testing"))
        {
            string dataPath;
            if (env.IsDevelopment())
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

            services.AddDbContext<DerotDbContext>(options =>
                options.UseSqlite($"Data Source={dataPath}/derot-my-brain.db"));
            
            Log.Information("Configured SQLite database at {DataPath}", dataPath);
        }

        services.AddScoped<IUserRepository, SqliteUserRepository>();
        services.AddScoped<IActivityRepository, SqliteActivityRepository>();
        services.AddScoped<IDocumentRepository, SqliteDocumentRepository>();
        services.AddScoped<IBacklogRepository, SqliteBacklogRepository>();
        services.AddScoped<IConfigurationRepository, SqliteConfigurationRepository>();

        // Configuration setup for DataDirectory used elsewhere if needed
        // Note: The original Program.cs set this in Configuration, which is fine to leave or move.
        // But since we are inside ConfigureServices, we can modify IConfiguration if it was mutable, but typically we shouldn't.
        // The original code did: builder.Configuration["DataDirectory"] = ...
        // We can pass that logic back to program.cs or keep it here if we change signature. 
        // For simplicity, we just configure services here.

        return services;
    }
}
