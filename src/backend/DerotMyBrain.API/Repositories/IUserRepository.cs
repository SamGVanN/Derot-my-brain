using DerotMyBrain.API.Models;

namespace DerotMyBrain.API.Repositories;

/// <summary>
/// Interface for user repository operations.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets all users in the system.
    /// </summary>
    /// <returns>List of users.</returns>
    Task<IEnumerable<User>> GetAllAsync();

    /// <summary>
    /// Gets a user by their unique identifier.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>The user if found, null otherwise.</returns>
    Task<User?> GetByIdAsync(string userId);

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="user">The user to create.</param>
    /// <returns>The created user.</returns>
    Task<User> CreateAsync(User user);

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="user">The user to update.</param>
    /// <returns>The updated user.</returns>
    Task<User> UpdateAsync(User user);

    /// <summary>
    /// Deletes a user by their ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>True if deleted, false if not found.</returns>
    Task<bool> DeleteAsync(string userId);

    /// <summary>
    /// Checks if a user exists with the given name (case-insensitive).
    /// </summary>
    /// <param name="name">The name to check.</param>
    /// <returns>The user if found, null otherwise.</returns>
    Task<User?> GetByNameAsync(string name);
}
