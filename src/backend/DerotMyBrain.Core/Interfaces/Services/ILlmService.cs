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
    /// <returns>A JSON string representing the quiz questions.</returns>
    Task<string> GenerateQuestionsAsync(string content, int numQuestions = 5, string difficulty = "Medium");
    
    /// <summary>
    /// Evaluates a user answer (optional, if we want server-side evaluation later).
    /// </summary>
    Task<bool> EvaluateAnswerAsync(string question, string answer, string userAnswer);
}
