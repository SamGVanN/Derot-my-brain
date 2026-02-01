using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Services;
using DerotMyBrain.Core.Interfaces.Utils;
using Microsoft.Extensions.Logging;

namespace DerotMyBrain.Core.Services;

public class QuizService : IQuizService
{
    private readonly ILlmService _llmService;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILogger<QuizService> _logger;

    public QuizService(
        ILlmService llmService,
        IJsonSerializer jsonSerializer,
        ILogger<QuizService> logger)
    {
        _llmService = llmService;
        _jsonSerializer = jsonSerializer;
        _logger = logger;
    }

    public async Task<QuizDto> GenerateQuizAsync(string content, QuizFormat format = QuizFormat.MCQ, int count = 5, string difficulty = "Medium")
    {
        _logger.LogInformation("Generating {Count} {Format} questions with {Difficulty} difficulty", count, format, difficulty);
        
        var questionsJson = await _llmService.GenerateQuestionsAsync(content, count, difficulty);
        var questions = _jsonSerializer.Deserialize<List<QuestionDto>>(questionsJson) ?? new List<QuestionDto>();

        return new QuizDto { Questions = questions };
    }

    public async Task<bool> EvaluateOpenAnswerAsync(string question, string expected, string actual)
    {
        return await _llmService.EvaluateAnswerAsync(question, expected, actual);
    }
}
