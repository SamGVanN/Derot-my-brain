using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.DTOs;

public class AnswerEvaluationRequest
{
    public int QuestionId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string ExpectedAnswer { get; set; } = string.Empty;
    public string UserAnswer { get; set; } = string.Empty;
}

public class BatchEvaluationResult
{
    public List<QuestionEvaluationResult> Evaluations { get; set; } = new();
}

public class QuestionEvaluationResult : SemanticEvaluationResult
{
    public int QuestionId { get; set; }
}
