using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Services;
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

    public ConfigurationServiceTests()
    {
        _tempConfigDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempConfigDir);
        
        _loggerMock = new Mock<ILogger<ConfigurationService>>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _configMock = new Mock<IConfiguration>();
        _configMock.Setup(c => c["DataDirectory"]).Returns(_tempConfigDir);
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
        var service = new ConfigurationService(_configMock.Object, _loggerMock.Object, _httpClientFactoryMock.Object);

        // Act
        await service.InitializeAsync();

        // Assert
        var config = await service.GetConfigurationAsync();
        Assert.NotNull(config);
        Assert.Equal("global", config.Id);
        Assert.Equal("ollama", config.LLM.Provider);
        Assert.Equal(11434, config.LLM.Port);
    }

    [Fact]
    public async Task GetLLMConfigurationAsync_ReturnsCorrectConfig()
    {
        // Arrange
        var service = new ConfigurationService(_configMock.Object, _loggerMock.Object, _httpClientFactoryMock.Object);
        await service.InitializeAsync();

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
