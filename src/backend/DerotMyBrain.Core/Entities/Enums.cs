namespace DerotMyBrain.Core.Entities;

/// <summary>
/// Defines the type of activity session.
/// </summary>
public enum ActivityType
{
    /// <summary>
    /// Exploring wikipedia article/choosing next Source for Read Activity.
    /// </summary>
    Explore = 0,

    /// <summary>
    /// Reading content.
    /// </summary>
    Read = 1,

    /// <summary>
    /// Answering a quiz generated from content.
    /// </summary>
    Quiz = 2,

    /// <summary>
    /// A structured study session (placeholder for future use).
    /// </summary>
    Study = 3
}

/// <summary>
/// Defines the supported sources of content.
/// </summary>
    /// <summary>
    /// Custom user-provided text.
    /// </summary>
    public enum SourceType
    {
        /// <summary>
        /// Wikipedia article.
        /// </summary>
        Wikipedia = 1,
    
        /// <summary>
        /// Uploaded PDF or text document.
        /// </summary>
        Document = 2,
    
        /// <summary>
        /// Custom user-provided text.
        /// </summary>
        Custom = 3,

        /// <summary>
        /// Generic web link (uses OnlineResource).
        /// </summary>
        WebLink = 4
    }
