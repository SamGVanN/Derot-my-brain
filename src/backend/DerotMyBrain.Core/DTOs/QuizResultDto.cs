namespace DerotMyBrain.Core.DTOs;

/// <summary>
/// DTO for quiz evaluation results returned to the client.
/// </summary>
public class QuizResultDto
{
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public double ScorePercentage { get; set; }
    public List<QuestionResultDto> Results { get; set; } = new();
}

/// <summary>
/// DTO for individual question evaluation result.
/// </summary>
public class QuestionResultDto
{
    public int QuestionId { get; set; }
    public bool IsCorrect { get; set; }
    public string? UserAnswer { get; set; }
    public string? CorrectAnswer { get; set; }
    public string Explanation { get; set; } = string.Empty;
    
    /// <summary>
    /// Semantic similarity score for open-ended questions (0.0 to 1.0).
    /// Null for MCQ questions.
    /// </summary>
    public double? SemanticScore { get; set; }
}
