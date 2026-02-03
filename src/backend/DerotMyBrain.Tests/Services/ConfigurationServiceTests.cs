using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Services;
using DerotMyBrain.Core.Interfaces.Repositories;
using DerotMyBrain.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace DerotMyBrain.Tests.Services;

public class ConfigurationServiceTests : IDisposable
{
    private readonly string _tempConfigDir;
    private readonly Mock<ILogger<ConfigurationService>> _loggerMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<IConfigurationRepository> _configRepositoryMock;

    public ConfigurationServiceTests()
    {
        _tempConfigDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempConfigDir);
        
        _loggerMock = new Mock<ILogger<ConfigurationService>>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _configMock = new Mock<IConfiguration>();
        _configMock.Setup(c => c["DataDirectory"]).Returns(_tempConfigDir);
        _configRepositoryMock = new Mock<IConfigurationRepository>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempConfigDir))
        {
            Directory.Delete(_tempConfigDir, true);
        }
    }

    [Fact]
    public async Task InitializeAsync_CreatesDefaultConfiguration()
    {
        // Arrange
        _configRepositoryMock.Setup(r => r.GetAsync()).ReturnsAsync((AppConfiguration?)null);
        _configRepositoryMock.Setup(r => r.SaveAsync(It.IsAny<AppConfiguration>()))
            .ReturnsAsync((AppConfiguration config) => config);
        
        var service = new ConfigurationService(
            _configMock.Object, 
            _loggerMock.Object, 
            _httpClientFactoryMock.Object,
            _configRepositoryMock.Object);

        // Act
        await service.InitializeAsync();

        // Assert
        _configRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<AppConfiguration>()), Times.Once);
    }

    [Fact]
    public async Task GetLLMConfigurationAsync_ReturnsCorrectConfig()
    {
        // Arrange
        var testConfig = new AppConfiguration
        {
            Id = "global",
            LLM = new LLMConfiguration
            {
                Url = "127.0.0.1",
                Port = 11434,
                Provider = "ollama",
                DefaultModel = "llama3:8b",
                TimeoutSeconds = 30
            }
        };
        
        _configRepositoryMock.Setup(r => r.GetAsync()).ReturnsAsync(testConfig);
        
        var service = new ConfigurationService(
            _configMock.Object, 
            _loggerMock.Object, 
            _httpClientFactoryMock.Object,
            _configRepositoryMock.Object);

        // Act
        var llmConfig = await service.GetLLMConfigurationAsync();

        // Assert
        Assert.NotNull(llmConfig);
        Assert.Equal(11434, llmConfig.Port);
    }

    [Fact]
    public void LLMConfiguration_GetFullUrl_ReturnsCorrectEndpoint()
    {
        // Arrange
        var config = new LLMConfiguration
        {
            Url = "127.0.0.1",
            Port = 11434
        };

        // Act & Assert
        Assert.Equal("http://127.0.0.1:11434", config.GetFullUrl());
    }
}
