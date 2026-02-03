using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
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
        
        List<QuestionDto> questions = new();
        var trimmedJson = questionsJson.Trim();

        if (trimmedJson.StartsWith("["))
        {
            questions = _jsonSerializer.Deserialize<List<QuestionDto>>(questionsJson) ?? new List<QuestionDto>();
        }
        else if (trimmedJson.StartsWith("{"))
        {
            // If it's an object, try to parse it and look for a list property (like "questions" or "items")
            try 
            {
                using var doc = System.Text.Json.JsonDocument.Parse(questionsJson);
                var root = doc.RootElement;
                
                // Look for the first property that is an array
                bool found = false;
                foreach (var property in root.EnumerateObject())
                {
                    if (property.Value.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        var arrayJson = property.Value.GetRawText();
                        questions = _jsonSerializer.Deserialize<List<QuestionDto>>(arrayJson) ?? new List<QuestionDto>();
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    _logger.LogWarning("LLM returned an object but no array property was found.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse wrapped JSON object from LLM");
                throw new System.Text.Json.JsonException("Failed to parse wrapped JSON object from LLM", ex);
            }
        }
            else
            {
                throw new System.Text.Json.JsonException("LLM returned non-JSON content");
            }
            
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

    public async Task<List<QuestionEvaluationResult>> EvaluateOpenAnswersBatchAsync(string sourceContext, List<AnswerEvaluationRequest> requests, string language = "en")
    {
        return await _llmService.EvaluateAnswersBatchAsync(sourceContext, requests, language);
    }
}
