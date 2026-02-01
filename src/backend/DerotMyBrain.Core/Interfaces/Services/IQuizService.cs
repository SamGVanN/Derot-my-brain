using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.Interfaces.Services;

public interface IQuizService
{
    Task<QuizDto> GenerateQuizAsync(string content, QuizFormat format = QuizFormat.MCQ, int count = 5, string difficulty = "Medium");
    Task<bool> EvaluateOpenAnswerAsync(string question, string expected, string actual);
}
