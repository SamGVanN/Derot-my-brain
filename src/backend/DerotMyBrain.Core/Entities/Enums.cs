namespace DerotMyBrain.Core.Entities;

/// <summary>
/// Defines the type of activity session.
/// </summary>
public enum ActivityType
{
    /// <summary>
    /// Reading/Exploring content.
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
    Custom = 3
}
