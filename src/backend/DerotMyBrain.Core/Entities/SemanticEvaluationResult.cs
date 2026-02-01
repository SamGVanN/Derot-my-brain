namespace DerotMyBrain.Core.Entities;

/// <summary>
/// Result of semantic evaluation for an open-ended answer.
/// </summary>
public class SemanticEvaluationResult
{
    /// <summary>
    /// Semantic similarity score between 0.0 (completely wrong) and 1.0 (perfect match).
    /// </summary>
    public double Score { get; set; }
    
    /// <summary>
    /// Explanation of the evaluation from the LLM.
    /// </summary>
    public string Explanation { get; set; } = string.Empty;
}
