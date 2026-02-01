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

    public async Task<QuizDto> GenerateQuizAsync(string content, QuizFormat format = QuizFormat.MCQ, int count = 5, string difficulty = "Medium", string language = "en")
    {
        _logger.LogInformation("Generating {Count} {Format} questions with {Difficulty} difficulty in {Language}", count, format, difficulty, language);
        
        try
        {
            var questionsJson = await _llmService.GenerateQuestionsAsync(content, count, difficulty, format, language);
            
            _logger.LogWarning("LLM returned JSON: {Json}", questionsJson);
            
            var questions = _jsonSerializer.Deserialize<List<QuestionDto>>(questionsJson) ?? new List<QuestionDto>();
            
            _logger.LogInformation("Successfully generated {Count} questions", questions.Count);

            return new QuizDto { Questions = questions };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating quiz");
            throw;
        }
    }

    public async Task<SemanticEvaluationResult> EvaluateOpenAnswerAsync(string question, string expected, string actual, string language = "en")
    {
        return await _llmService.EvaluateAnswerAsync(question, expected, actual, language);
    }
}
