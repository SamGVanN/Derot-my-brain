using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Repositories;
using DerotMyBrain.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DerotMyBrain.Infrastructure.Repositories;

/// <summary>
/// SQLite implementation of the configuration repository.
/// </summary>
public class SqliteConfigurationRepository : IConfigurationRepository
{
    private readonly DerotDbContext _context;
    private readonly ILogger<SqliteConfigurationRepository> _logger;
    private const string GlobalConfigId = "global";

    public SqliteConfigurationRepository(
        DerotDbContext context,
        ILogger<SqliteConfigurationRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets the global application configuration from the database.
    /// </summary>
    public async Task<AppConfiguration?> GetAsync()
    {
        try
        {
            return await _context.AppConfigurations
                .FirstOrDefaultAsync(c => c.Id == GlobalConfigId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving configuration from database");
            throw;
        }
    }

    /// <summary>
    /// Saves or updates the global application configuration.
    /// </summary>
    public async Task<AppConfiguration> SaveAsync(AppConfiguration config)
    {
        try
        {
            // Ensure the ID is always "global"
            config.Id = GlobalConfigId;
            config.LastUpdated = DateTime.UtcNow;

            var existing = await GetAsync();
            
            if (existing == null)
            {
                _context.AppConfigurations.Add(config);
                _logger.LogInformation("Creating new configuration in database");
            }
            else
            {
                // Update existing configuration
                existing.LLM = config.LLM;
                existing.LastUpdated = config.LastUpdated;
                _context.AppConfigurations.Update(existing);
                _logger.LogInformation("Updating existing configuration in database");
            }

            await _context.SaveChangesAsync();
            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving configuration to database");
            throw;
        }
    }

    /// <summary>
    /// Deletes the global application configuration.
    /// Used for reset to default functionality.
    /// </summary>
    public async Task DeleteAsync()
    {
        try
        {
            var existing = await GetAsync();
            if (existing != null)
            {
                _context.AppConfigurations.Remove(existing);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted configuration from database");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting configuration from database");
            throw;
        }
    }
}
