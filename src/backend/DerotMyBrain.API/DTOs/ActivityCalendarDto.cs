namespace DerotMyBrain.API.DTOs;

/// <summary>
/// Data transfer object for activity calendar (GitLab-style heatmap).
/// </summary>
public class ActivityCalendarDto
{
    /// <summary>
    /// Date of the activities.
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Number of activities on this date.
    /// </summary>
    public int Count { get; set; }
}
