using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Infrastructure.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using Xunit;

namespace DerotMyBrain.Tests.Infrastructure.Clients;

public class WikipediaClientTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<ILogger<WikipediaClient>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly WikipediaClient _client;

    public WikipediaClientTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _loggerMock = new Mock<ILogger<WikipediaClient>>();
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(c => c["Wikipedia:ContactEmail"]).Returns("test@example.com");

        _client = new WikipediaClient(_httpClient, _loggerMock.Object, _configurationMock.Object);
    }

    [Fact]
    public async Task GetArticleRichContentAsync_ValidTitle_ReturnsContent()
    {
        // Arrange
        var title = "Quantum_mechanics";
        var lang = "en";
        var jsonResponse = @"{
            ""query"": {
                ""pages"": {
                    ""12345"": {
                        ""pageid"": 12345,
                        ""ns"": 0,
                        ""title"": ""Quantum mechanics"",
                        ""extract"": ""Quantum mechanics is a fundamental theory in physics.""
                    }
                }
            }
        }";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString().Contains("titles=Quantum_mechanics")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        // Act
        var result = await _client.GetArticleRichContentAsync(title, lang);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Quantum mechanics", result.Title);
        Assert.Equal("Quantum mechanics is a fundamental theory in physics.", result.TextContent);
        Assert.Equal("Wikipedia", result.SourceType);
    }

    [Fact]
    public async Task GetArticleRichContentAsync_InvalidTitle_ThrowsException()
    {
        // Arrange
        var title = "NonExistentPage12345";
        var lang = "en";
        var jsonResponse = @"{
            ""query"": {
                ""pages"": {
                    ""-1"": {
                        ""ns"": 0,
                        ""title"": ""NonExistentPage12345"",
                        ""missing"": """"
                    }
                }
            }
        }";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _client.GetArticleRichContentAsync(title, lang));
    }
}
