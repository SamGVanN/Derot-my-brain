using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DerotMyBrain.Core.Interfaces.Services;
using DerotMyBrain.Core.DTOs;
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
            format = "json", // Ollama JSON mode
            options = new { num_ctx = 16384 }
        };

        var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions(JsonSerializerDefaults.Web));
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
        
        // Find the actual boundaries of the JSON content (object or array)
        var firstBrace = cleaned.IndexOf('{');
        var firstBracket = cleaned.IndexOf('[');
        
        int start;
        if (firstBrace >= 0 && firstBracket >= 0) start = Math.Min(firstBrace, firstBracket);
        else if (firstBrace >= 0) start = firstBrace;
        else if (firstBracket >= 0) start = firstBracket;
        else return "[]";

        var lastBrace = cleaned.LastIndexOf('}');
        var lastBracket = cleaned.LastIndexOf(']');
        
        int end = Math.Max(lastBrace, lastBracket);
        
        if (end > start)
        {
            cleaned = cleaned.Substring(start, end - start + 1);
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

        var (languageInstruction, languageName) = language.ToLower() switch
        {
            "fr" => ("IMPORTANT: Use ONLY FRENCH for all output text, including the justification/explanation.", "FRENCH"),
            "es" => ("IMPORTANT: Use ONLY SPANISH for all output text, including the justification/explanation.", "SPANISH"),
            "de" => ("IMPORTANT: Use ONLY GERMAN for all output text, including the justification/explanation.", "GERMAN"),
            _ => ("IMPORTANT: Use ONLY ENGLISH for all output text, including the justification/explanation.", "ENGLISH")
        };

        var prompt = $@"
You are a RIGOROUS and UNBIASED evaluator.
{languageInstruction}

### TASK:
Evaluate if the STUDENT_RESPONSE is accurate compared to the OFFICIAL_GROUND_TRUTH.

### SCORING SCALE:
- 1.0: Perfect. Identical meaning.
- 0.7 - 0.9: Mostly correct.
- 0.4 - 0.6: Partially correct.
- 0.0: Incorrect, Nonsense (like ""zeazze"", ""abcd""), or Missing.

### EVALUATION PROTOCOL:
1. Compare 'STUDENT_RESPONSE' to 'OFFICIAL_GROUND_TRUTH'.
2. NEVER assume the student is correct.
3. If 'STUDENT_RESPONSE' is nonsense or irrelevant, you MUST give a score of 0.0.
4. Assign a score from 0.0 to 1.0.
5. Provide a justification in {languageName}.

### OUTPUT FORMAT:
Return ONLY a valid JSON object:
{{
  ""score"": <0.0 to 1.0>,
  ""explanation"": ""<justification in {languageName}>""
}}

### DATA:
Question: {question}

OFFICIAL_GROUND_TRUTH:
{expectedAnswer}

STUDENT_RESPONSE (GRADE THIS):
{userAnswer}

{languageInstruction}
Final Instruction: Be extremely strict. Nonsense answers get 0.0. Justify clearly in {languageName}.
";

        var requestBody = new
        {
            model = model,
            prompt = prompt,
            stream = false,
            format = "json",
            options = new { num_ctx = 16384 }
        };

        try
        {
            var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions(JsonSerializerDefaults.Web));
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

            // Log the raw LLM response for debugging
            _logger.LogInformation("LLM Evaluation Response: {Response}", generatedText);

            // Clean up the response - remove markdown code blocks
            var cleanedJson = CleanJsonResponse(generatedText);
            
            _logger.LogInformation("Cleaned JSON: {CleanedJson}", cleanedJson);

            // Parse the evaluation result
            var evaluation = JsonSerializer.Deserialize<SemanticEvaluationResult>(cleanedJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (evaluation == null)
            {
                _logger.LogWarning("Failed to deserialize LLM evaluation result");
                return FallbackEvaluation(expectedAnswer, userAnswer);
            }

            // Log the evaluation result
            _logger.LogInformation("Evaluation Result - Score: {Score}, Explanation: {Explanation}", evaluation.Score, evaluation.Explanation);

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

    public async Task<List<QuestionEvaluationResult>> EvaluateAnswersBatchAsync(string sourceContext, List<AnswerEvaluationRequest> requests, string language = "en")
    {
        if (requests == null || !requests.Any())
            return new List<QuestionEvaluationResult>();

        var config = await _configurationService.GetLLMConfigurationAsync();
        var baseUrl = config.GetFullUrl();
        var model = config.DefaultModel;

        var (languageInstruction, languageName) = language.ToLower() switch
        {
            "fr" => ("IMPORTANT: Use ONLY FRENCH for all output text, including all justifications/explanations.", "FRENCH"),
            "es" => ("IMPORTANT: Use ONLY SPANISH for all output text, including all justifications/explanations.", "SPANISH"),
            "de" => ("IMPORTANT: Use ONLY GERMAN for all output text, including all justifications/explanations.", "GERMAN"),
            _ => ("IMPORTANT: Use ONLY ENGLISH for all output text, including all justifications/explanations.", "ENGLISH")
        };

        var requestsJson = JsonSerializer.Serialize(requests);

        // We use very explicit labels to avoid model confusion between UserAnswer and ExpectedAnswer
        var prompt = $@"
You are a RIGOROUS and UNBIASED evaluator.
{languageInstruction}

### TASK:
Evaluate if the STUDENT_RESPONSE is accurate compared to the OFFICIAL_GROUND_TRUTH based on the source context.

### SOURCE CONTEXT:
---
{sourceContext.Substring(0, Math.Min(sourceContext.Length, 8000))}
---

### EVALUATION PROTOCOL:
You will receive a list of items to evaluate. For each item:
1. Identify the 'STUDENT_RESPONSE' (field: UserAnswer) and the 'OFFICIAL_GROUND_TRUTH' (field: ExpectedAnswer).
2. NEVER assume the student is correct.
3. Compare the 'STUDENT_RESPONSE' to the 'OFFICIAL_GROUND_TRUTH' and the 'SOURCE CONTEXT'.
4. If 'STUDENT_RESPONSE' is nonsense (random characters like ""zeazze"", ""abcd""), irrelevant, or empty, you MUST give a score of 0.0.
5. Assign a score from 0.0 to 1.0.
6. Provide a justification in {languageName}.

### SCORING SCALE:
- 1.0: Perfect. Identical meaning.
- 0.7 - 0.9: Mostly correct.
- 0.4 - 0.6: Partially correct.
- 0.0: Incorrect, Nonsense, or Missing.

### OUTPUT FORMAT:
Return ONLY a valid JSON object:
{{
  ""evaluations"": [
    {{
      ""questionId"": <id>,
      ""score"": <0.0 to 1.0>,
      ""explanation"": ""<justification in {languageName}>""
    }}
  ]
}}

### EVALUATION DATA (Grade 'UserAnswer' against 'ExpectedAnswer'):
{requestsJson}

{languageInstruction}
Final Instruction: Be extremely strict. Nonsense answers get 0.0. Justify clearly in {languageName}.
";

        var requestBody = new
        {
            model = model,
            prompt = prompt,
            stream = false,
            format = "json",
            options = new { num_ctx = 16384 }
        };

        try
        {
            var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            var response = await _httpClient.PostAsync($"{baseUrl}/api/generate", new StringContent(json, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("LLM batch evaluation request failed: {StatusCode} - {Error}", response.StatusCode, error);
                return FallbackBatchEvaluation(requests);
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var responseJson = JsonNode.Parse(responseString);
            var generatedText = responseJson?["response"]?.ToString();

            if (string.IsNullOrEmpty(generatedText))
            {
                _logger.LogWarning("Empty response from LLM batch evaluation");
                return FallbackBatchEvaluation(requests);
            }

            _logger.LogInformation("LLM Batch Evaluation Response: {Response}", generatedText);

            var cleanedJson = CleanJsonResponse(generatedText);
            
            List<QuestionEvaluationResult> evaluations = new();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            try 
            {
                if (cleanedJson.TrimStart().StartsWith("["))
                {
                    evaluations = JsonSerializer.Deserialize<List<QuestionEvaluationResult>>(cleanedJson, options) ?? new();
                }
                else
                {
                    var batchResult = JsonSerializer.Deserialize<BatchEvaluationResult>(cleanedJson, options);
                    evaluations = batchResult?.Evaluations ?? new();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize LLM batch evaluation result. Raw cleaned JSON: {Json}", cleanedJson);
                return FallbackBatchEvaluation(requests);
            }

            if (!evaluations.Any())
            {
                _logger.LogWarning("No evaluations found in LLM response");
                return FallbackBatchEvaluation(requests);
            }

            foreach (var eval in evaluations)
            {
                eval.Score = Math.Clamp(eval.Score, 0.0, 1.0);
            }
            
            return evaluations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during LLM batch answer evaluation");
            return FallbackBatchEvaluation(requests);
        }
    }

    private List<QuestionEvaluationResult> FallbackBatchEvaluation(List<AnswerEvaluationRequest> requests)
    {
        return requests.Select(r => 
        {
            var isMatch = string.Equals(r.ExpectedAnswer?.Trim(), r.UserAnswer?.Trim(), StringComparison.OrdinalIgnoreCase);
            return new QuestionEvaluationResult
            {
                QuestionId = r.QuestionId,
                Score = isMatch ? 1.0 : 0.0,
                Explanation = isMatch 
                    ? "Exact match (LLM evaluation unavailable)" 
                    : "Answer does not match expected response (LLM evaluation unavailable)"
            };
        }).ToList();
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
