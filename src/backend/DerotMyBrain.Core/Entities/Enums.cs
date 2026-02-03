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

/// <summary>
/// Defines the format of a quiz question.
/// </summary>
public enum QuizFormat
{
    /// <summary>
    /// Multiple Choice Question.
    /// </summary>
    MCQ = 0,
    
    /// <summary>
    /// Open-ended question.
    /// </summary>
    OpenEnded = 1
}

/// <summary>
/// Defines the status of content extraction for document sources.
/// </summary>
public enum ContentExtractionStatus
{
    /// <summary>
    /// Document uploaded, extraction not started yet.
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Content extraction is currently in progress.
    /// </summary>
    Processing = 1,
    
    /// <summary>
    /// Content extraction completed successfully.
    /// </summary>
    Completed = 2,
    
    /// <summary>
    /// Content extraction failed with an error.
    /// </summary>
    Failed = 3
}
