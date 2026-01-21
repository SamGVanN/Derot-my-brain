// Trigger reload
namespace DerotMyBrain.API.Models;

/// <summary>
/// Represents a user of the application.
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier for the user.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// User's display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when the user account was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Date and time of the user's last connection.
    /// </summary>
    public DateTime LastConnectionAt { get; set; }
    
    /// <summary>
    /// Navigation property to user preferences (1-to-1 relationship).
    /// </summary>
    public UserPreferences? Preferences { get; set; }
    
    /// <summary>
    /// Navigation property to user activities (1-to-many relationship).
    /// </summary>
    public ICollection<UserActivity> Activities { get; set; } = new List<UserActivity>();

    /// <summary>
    /// Navigation property to user tracked topics (1-to-many relationship).
    /// </summary>
    public List<TrackedTopic> TrackedTopics { get; set; } = new();
}

/// <summary>
/// Container for a list of users (used for JSON serialization).
/// </summary>
public class UserList
{
    public List<User> Users { get; set; } = [];
}

