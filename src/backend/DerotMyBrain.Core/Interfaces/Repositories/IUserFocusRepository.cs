using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for UserFocus operations.
/// </summary>
public interface IUserFocusRepository
{
    /// <summary>
    /// Gets a user focus by its unique identifier.
    /// </summary>
    Task<UserFocus?> GetByIdAsync(string id);
    
    /// <summary>
    /// Gets a user focus by user ID and content hash.
    /// </summary>
    Task<UserFocus?> GetByHashAsync(string userId, string sourceHash);
    
    /// <summary>
    /// Gets all user focus objects for a user.
    /// </summary>
    Task<IEnumerable<UserFocus>> GetAllAsync(string userId);
    
    /// <summary>
    /// Creates a new user focus.
    /// </summary>
    Task<UserFocus> CreateAsync(UserFocus userFocus);
    
    /// <summary>
    /// Updates an existing user focus.
    /// </summary>
    Task UpdateAsync(UserFocus userFocus);
    
    /// <summary>
    /// Deletes a user focus by ID.
    /// </summary>
    Task DeleteAsync(string id);
    
    /// <summary>
    /// Checks if a content (by hash) is focused by the user.
    /// </summary>
    Task<bool> ExistsAsync(string userId, string sourceHash);
}
