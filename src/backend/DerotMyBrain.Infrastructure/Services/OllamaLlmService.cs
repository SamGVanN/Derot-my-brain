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

    public async Task<string> GenerateQuestionsAsync(string content, int numQuestions = 5, string difficulty = "Medium", QuizFormat format = QuizFormat.MCQ)
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

        var prompt = $@"
You are a quiz generator. Generate {numQuestions} {format} questions based on the text below.
Difficulty: {difficulty}.
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

        return generatedText;
    }

    public async Task<bool> EvaluateAnswerAsync(string question, string answer, string userAnswer)
    {
        // Simple string comparison for now, or ask LLM if it's open-ended
        return string.Equals(answer, userAnswer, StringComparison.OrdinalIgnoreCase);
    }
}
