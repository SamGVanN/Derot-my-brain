namespace DerotMyBrain.Core.DTOs;

/// <summary>
/// DTO for submitting quiz answers from the client.
/// </summary>
public class QuizSubmissionDto
{
    public List<AnswerSubmissionDto> Answers { get; set; } = new();
    public int DurationSeconds { get; set; }
}

/// <summary>
/// DTO for a single answer submission.
/// </summary>
public class AnswerSubmissionDto
{
    public int QuestionId { get; set; }
    
    /// <summary>
    /// For MCQ: the selected option text (e.g., "Option A")
    /// </summary>
    public string? SelectedOption { get; set; }
    
    /// <summary>
    /// For Open-ended: the user's text answer
    /// </summary>
    public string? TextAnswer { get; set; }
}
