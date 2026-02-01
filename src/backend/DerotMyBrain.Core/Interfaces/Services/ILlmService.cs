using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.Interfaces.Services;

public interface ILlmService
{
    /// <summary>
    /// Generates a quiz from the provided text content.
    /// </summary>
    /// <param name="content">The text content to generate questions from.</param>
    /// <param name="numQuestions">Number of questions to generate.</param>
    /// <param name="difficulty">Difficulty level (Easy, Medium, Hard).</param>
    /// <param name="format">Quiz format (MCQ or OpenEnded).</param>
    /// <param name="language">Language for questions (e.g., "en", "fr").</param>
    /// <returns>A JSON string representing the quiz questions.</returns>
    Task<string> GenerateQuestionsAsync(string content, int numQuestions = 5, string difficulty = "Medium", QuizFormat format = QuizFormat.MCQ, string language = "en");
    
    /// <summary>
    /// Evaluates a user answer using semantic comparison.
    /// </summary>
    /// <param name="question">The question text.</param>
    /// <param name="expectedAnswer">The expected/correct answer.</param>
    /// <param name="userAnswer">The user's submitted answer.</param>
    /// <param name="language">Language for evaluation feedback (e.g., "en", "fr").</param>
    /// <returns>Semantic evaluation result with score (0.0-1.0) and explanation.</returns>
    Task<SemanticEvaluationResult> EvaluateAnswerAsync(string question, string expectedAnswer, string userAnswer, string language = "en");
}
