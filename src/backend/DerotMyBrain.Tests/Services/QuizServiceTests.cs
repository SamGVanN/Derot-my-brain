using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Services;
using DerotMyBrain.Core.Interfaces.Utils;
using DerotMyBrain.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DerotMyBrain.Tests.Services;

public class QuizServiceTests
{
    private readonly Mock<ILlmService> _llmServiceMock;
    private readonly Mock<IJsonSerializer> _jsonSerializerMock;
    private readonly Mock<ILogger<QuizService>> _loggerMock;
    private readonly QuizService _service;

    public QuizServiceTests()
    {
        _llmServiceMock = new Mock<ILlmService>();
        _jsonSerializerMock = new Mock<IJsonSerializer>();
        _loggerMock = new Mock<ILogger<QuizService>>();

        _service = new QuizService(
            _llmServiceMock.Object,
            _jsonSerializerMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GenerateQuizAsync_WithEnglish_PassesCorrectLanguageToLlm()
    {
        // Arrange
        var content = "Test content about history";
        var format = QuizFormat.MCQ;
        var count = 5;
        var difficulty = "Medium";
        var language = "en";

        var mockJson = "[{\"id\":1,\"text\":\"Question?\",\"options\":[\"A\",\"B\"],\"correctOptionIndex\":0,\"explanation\":\"Explanation\",\"type\":\"MCQ\"}]";
        var mockQuestions = new List<QuestionDto>
        {
            new QuestionDto
            {
                Id = 1,
                Text = "Question?",
                Options = new List<string> { "A", "B" },
                CorrectOptionIndex = 0,
                Explanation = "Explanation",
                Type = "MCQ"
            }
        };

        _llmServiceMock
            .Setup(s => s.GenerateQuestionsAsync(content, count, difficulty, format, language))
            .ReturnsAsync(mockJson);

        _jsonSerializerMock
            .Setup(s => s.Deserialize<List<QuestionDto>>(mockJson))
            .Returns(mockQuestions);

        // Act
        var result = await _service.GenerateQuizAsync(content, format, count, difficulty, language);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Questions);
        _llmServiceMock.Verify(s => s.GenerateQuestionsAsync(content, count, difficulty, format, "en"), Times.Once);
    }

    [Fact]
    public async Task GenerateQuizAsync_WithFrench_PassesCorrectLanguageToLlm()
    {
        // Arrange
        var content = "Contenu de test sur l'histoire";
        var format = QuizFormat.OpenEnded;
        var count = 3;
        var difficulty = "Hard";
        var language = "fr";

        var mockJson = "[{\"id\":1,\"text\":\"Question?\",\"correctAnswer\":\"Answer\",\"explanation\":\"Explication\",\"type\":\"OpenEnded\"}]";
        var mockQuestions = new List<QuestionDto>
        {
            new QuestionDto
            {
                Id = 1,
                Text = "Question?",
                CorrectAnswer = "Answer",
                Explanation = "Explication",
                Type = "OpenEnded"
            }
        };

        _llmServiceMock
            .Setup(s => s.GenerateQuestionsAsync(content, count, difficulty, format, language))
            .ReturnsAsync(mockJson);

        _jsonSerializerMock
            .Setup(s => s.Deserialize<List<QuestionDto>>(mockJson))
            .Returns(mockQuestions);

        // Act
        var result = await _service.GenerateQuizAsync(content, format, count, difficulty, language);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Questions);
        _llmServiceMock.Verify(s => s.GenerateQuestionsAsync(content, count, difficulty, format, "fr"), Times.Once);
    }

    [Fact]
    public async Task GenerateQuizAsync_WithInvalidJson_ThrowsException()
    {
        // Arrange
        var content = "Test content";
        var mockJson = "invalid json";

        _llmServiceMock
            .Setup(s => s.GenerateQuestionsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<QuizFormat>(), It.IsAny<string>()))
            .ReturnsAsync(mockJson);

        _jsonSerializerMock
            .Setup(s => s.Deserialize<List<QuestionDto>>(mockJson))
            .Throws(new System.Text.Json.JsonException("Invalid JSON"));

        // Act & Assert
        await Assert.ThrowsAsync<System.Text.Json.JsonException>(
            () => _service.GenerateQuizAsync(content));
    }

    [Fact]
    public async Task EvaluateOpenAnswerAsync_WithEnglish_PassesCorrectLanguageToLlm()
    {
        // Arrange
        var question = "What is the capital of France?";
        var expected = "Paris";
        var actual = "Paris";
        var language = "en";

        var mockEvaluation = new SemanticEvaluationResult
        {
            Score = 1.0,
            Explanation = "Perfect match"
        };

        _llmServiceMock
            .Setup(s => s.EvaluateAnswerAsync(question, expected, actual, language))
            .ReturnsAsync(mockEvaluation);

        // Act
        var result = await _service.EvaluateOpenAnswerAsync(question, expected, actual, language);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1.0, result.Score);
        Assert.Equal("Perfect match", result.Explanation);
        _llmServiceMock.Verify(s => s.EvaluateAnswerAsync(question, expected, actual, "en"), Times.Once);
    }

    [Fact]
    public async Task EvaluateOpenAnswerAsync_WithFrench_PassesCorrectLanguageToLlm()
    {
        // Arrange
        var question = "Quelle est la capitale de la France?";
        var expected = "Paris";
        var actual = "Paris";
        var language = "fr";

        var mockEvaluation = new SemanticEvaluationResult
        {
            Score = 1.0,
            Explanation = "Correspondance parfaite"
        };

        _llmServiceMock
            .Setup(s => s.EvaluateAnswerAsync(question, expected, actual, language))
            .ReturnsAsync(mockEvaluation);

        // Act
        var result = await _service.EvaluateOpenAnswerAsync(question, expected, actual, language);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1.0, result.Score);
        Assert.Equal("Correspondance parfaite", result.Explanation);
        _llmServiceMock.Verify(s => s.EvaluateAnswerAsync(question, expected, actual, "fr"), Times.Once);
    }
}
