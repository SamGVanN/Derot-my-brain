using System.ComponentModel.DataAnnotations;

namespace DerotMyBrain.Core.Entities;

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
    /// Navigation property to user sessions (1-to-many relationship).
    /// </summary>
    public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();

    /// <summary>
    /// Navigation property to user activities (1-to-many relationship).
    /// </summary>
    public ICollection<UserActivity> Activities { get; set; } = new List<UserActivity>();

    /// <summary>
    /// Navigation property for all content sources linked to the user.
    /// </summary>
    public ICollection<Source> Sources { get; set; } = new List<Source>();
}
