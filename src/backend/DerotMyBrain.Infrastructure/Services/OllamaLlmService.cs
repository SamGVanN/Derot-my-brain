using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DerotMyBrain.Core.Interfaces.Services;
using DerotMyBrain.Core.Entities; // For AppConfiguration if needed, but usually passed in constructor
using Microsoft.Extensions.Logging;

namespace DerotMyBrain.Infrastructure.Services;

public class OllamaLlmService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<OllamaLlmService> _logger;

    public OllamaLlmService(HttpClient httpClient, IConfigurationService configurationService, ILogger<OllamaLlmService> logger)
    {
        _httpClient = httpClient;
        _configurationService = configurationService;
        _logger = logger;
    }

    public async Task<string> GenerateQuestionsAsync(string content, int numQuestions = 5, string difficulty = "Medium", QuizFormat format = QuizFormat.MCQ, string language = "en")
    {
        var config = await _configurationService.GetLLMConfigurationAsync();
        var baseUrl = config.GetFullUrl();
        var model = config.DefaultModel;

        string formatInstructions = format == QuizFormat.MCQ 
            ? @"Return ONLY a valid JSON array of objects. No markdown formatting, no code blocks, just raw JSON.
Format:
[
  {
    ""id"": 1,
    ""text"": ""Question text here?"",
    ""options"": [""Option A"", ""Option B"", ""Option C"", ""Option D""],
    ""correctOptionIndex"": 0,
    ""explanation"": ""Why A is correct."",
    ""type"": ""MCQ""
  }
]"
            : @"Return ONLY a valid JSON array of objects. No markdown formatting, no code blocks, just raw JSON.
Format:
[
  {
    ""id"": 1,
    ""text"": ""Question text here?"",
    ""correctAnswer"": ""Reference answer for the user to compare with."",
    ""explanation"": ""Context or additional info."",
    ""type"": ""OpenEnded""
  }
]";

        var languageInstruction = language.ToLower() switch
        {
            "fr" => "IMPORTANT: Generate all questions, options, answers, and explanations in FRENCH.",
            "es" => "IMPORTANT: Generate all questions, options, answers, and explanations in SPANISH.",
            "de" => "IMPORTANT: Generate all questions, options, answers, and explanations in GERMAN.",
            _ => "IMPORTANT: Generate all questions, options, answers, and explanations in ENGLISH."
        };

        var prompt = $@"
You are a quiz generator. Generate {numQuestions} {format} questions based on the text below.
Difficulty: {difficulty}.
{languageInstruction}
{formatInstructions}

Text:
{content.Substring(0, Math.Min(content.Length, 12000))} 
";

        var requestBody = new
        {
            model = model,
            prompt = prompt,
            stream = false,
            format = "json" // Ollama JSON mode
        };

        var json = JsonSerializer.Serialize(requestBody);
        var response = await _httpClient.PostAsync($"{baseUrl}/api/generate", new StringContent(json, Encoding.UTF8, "application/json"));
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"LLM request failed: {response.StatusCode} - {error}");
        }

        var responseString = await response.Content.ReadAsStringAsync();
        var responseJson = JsonNode.Parse(responseString);
        var generatedText = responseJson?["response"]?.ToString();

        if (string.IsNullOrEmpty(generatedText)) return "[]";

        // Clean up the response - remove markdown code blocks and extract JSON
        var cleanedJson = CleanJsonResponse(generatedText);
        
        return cleanedJson;
    }

    private string CleanJsonResponse(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
            return "[]";

        // Remove markdown code blocks
        var cleaned = response.Trim();
        
        // Remove ```json and ``` markers
        if (cleaned.StartsWith("```json"))
        {
            cleaned = cleaned.Substring(7);
        }
        else if (cleaned.StartsWith("```"))
        {
            cleaned = cleaned.Substring(3);
        }
        
        if (cleaned.EndsWith("```"))
        {
            cleaned = cleaned.Substring(0, cleaned.Length - 3);
        }
        
        cleaned = cleaned.Trim();
        
        // Try to find JSON array in the response
        var arrayStart = cleaned.IndexOf('[');
        var arrayEnd = cleaned.LastIndexOf(']');
        
        if (arrayStart >= 0 && arrayEnd > arrayStart)
        {
            cleaned = cleaned.Substring(arrayStart, arrayEnd - arrayStart + 1);
        }
        
        return cleaned;
    }

    public async Task<SemanticEvaluationResult> EvaluateAnswerAsync(string question, string expectedAnswer, string userAnswer, string language = "en")
    {
        // Handle empty or null answers
        if (string.IsNullOrWhiteSpace(userAnswer))
        {
            var noAnswerMessage = language.ToLower() switch
            {
                "fr" => "Aucune réponse fournie.",
                "es" => "No se proporcionó respuesta.",
                "de" => "Keine Antwort angegeben.",
                _ => "No answer provided."
            };
            
            return new SemanticEvaluationResult
            {
                Score = 0.0,
                Explanation = noAnswerMessage
            };
        }

        var config = await _configurationService.GetLLMConfigurationAsync();
        var baseUrl = config.GetFullUrl();
        var model = config.DefaultModel;

        var languageInstruction = language.ToLower() switch
        {
            "fr" => "IMPORTANT: Provide the explanation in FRENCH.",
            "es" => "IMPORTANT: Provide the explanation in SPANISH.",
            "de" => "IMPORTANT: Provide the explanation in GERMAN.",
            _ => "IMPORTANT: Provide the explanation in ENGLISH."
        };

        var prompt = $@"
You are an answer evaluator for a quiz application.
{languageInstruction}

Compare the user's answer to the expected answer and provide a semantic similarity score.

Give a score between 0.0 and 1.0:
- 1.0 = Perfect match (exact or semantically equivalent)
- 0.7-0.9 = Good answer with minor differences
- 0.4-0.6 = Partially correct
- 0.0-0.3 = Incorrect or unrelated

Be tolerant of:
- Synonyms and paraphrasing
- Minor spelling errors
- Different word order if meaning is preserved

Return ONLY a valid JSON object with this exact format:
{{
  ""score"": 0.0,
  ""explanation"": ""Brief explanation of the evaluation""
}}

Question: {question}

Expected Answer:
{expectedAnswer}

User's Answer:
{userAnswer}
";

        var requestBody = new
        {
            model = model,
            prompt = prompt,
            stream = false,
            format = "json"
        };

        try
        {
            var json = JsonSerializer.Serialize(requestBody);
            var response = await _httpClient.PostAsync($"{baseUrl}/api/generate", new StringContent(json, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("LLM evaluation request failed: {StatusCode} - {Error}", response.StatusCode, error);
                
                // Fallback to simple comparison
                return FallbackEvaluation(expectedAnswer, userAnswer);
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var responseJson = JsonNode.Parse(responseString);
            var generatedText = responseJson?["response"]?.ToString();

            if (string.IsNullOrEmpty(generatedText))
            {
                _logger.LogWarning("Empty response from LLM evaluation");
                return FallbackEvaluation(expectedAnswer, userAnswer);
            }

            // Clean up the response - remove markdown code blocks
            var cleanedJson = CleanJsonResponse(generatedText);

            // Parse the evaluation result
            var evaluation = JsonSerializer.Deserialize<SemanticEvaluationResult>(cleanedJson);
            if (evaluation == null)
            {
                _logger.LogWarning("Failed to deserialize LLM evaluation result");
                return FallbackEvaluation(expectedAnswer, userAnswer);
            }

            // Clamp score to valid range
            evaluation.Score = Math.Clamp(evaluation.Score, 0.0, 1.0);
            
            return evaluation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during LLM answer evaluation");
            return FallbackEvaluation(expectedAnswer, userAnswer);
        }
    }

    private SemanticEvaluationResult FallbackEvaluation(string expectedAnswer, string userAnswer)
    {
        // Simple fallback: case-insensitive string comparison
        var isMatch = string.Equals(expectedAnswer?.Trim(), userAnswer?.Trim(), StringComparison.OrdinalIgnoreCase);
        
        return new SemanticEvaluationResult
        {
            Score = isMatch ? 1.0 : 0.0,
            Explanation = isMatch 
                ? "Exact match (LLM evaluation unavailable)" 
                : "Answer does not match expected response (LLM evaluation unavailable)"
        };
    }
}
