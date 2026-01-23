using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DerotMyBrain.Infrastructure.Data;

namespace DerotMyBrain.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory for integration tests.
/// Configures InMemory database and test environment.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = "IntegrationTestDb_" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set environment to Testing to skip initialization in Program.cs
        builder.UseEnvironment("Testing");
        
        // Use ConfigureTestServices to ensure this runs AFTER Program.cs configuration
        builder.ConfigureTestServices(services =>
        {
            // Remove ALL DbContext-related service descriptors
            var descriptorsToRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<DerotDbContext>) ||
                           d.ServiceType == typeof(DbContextOptions) ||
                           d.ServiceType == typeof(DerotDbContext))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }
            
            // Add InMemory database for testing with a stable name
            services.AddDbContext<DerotDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });
    }
}
