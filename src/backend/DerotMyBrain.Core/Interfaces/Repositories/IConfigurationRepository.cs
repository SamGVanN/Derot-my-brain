using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.Interfaces.Repositories;

/// <summary>
/// Interface for application configuration repository operations.
/// </summary>
public interface IConfigurationRepository
{
    /// <summary>
    /// Gets the global application configuration.
    /// </summary>
    /// <returns>The configuration if found, null otherwise.</returns>
    Task<AppConfiguration?> GetAsync();

    /// <summary>
    /// Saves or updates the global application configuration.
    /// </summary>
    /// <param name="config">The configuration to save.</param>
    /// <returns>The saved configuration.</returns>
    Task<AppConfiguration> SaveAsync(AppConfiguration config);

    /// <summary>
    /// Deletes the global application configuration.
    /// Used for reset to default functionality.
    /// </summary>
    Task DeleteAsync();
}
