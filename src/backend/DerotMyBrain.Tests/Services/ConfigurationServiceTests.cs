using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Moq.Protected;
using DerotMyBrain.API.Services;
using DerotMyBrain.API.Models;

namespace DerotMyBrain.Tests.Services
{
    public class ConfigurationServiceTests : IDisposable
    {
        private readonly string _testConfigDirectory;
        private readonly ConfigurationService _configService;
        private readonly Mock<ILogger<ConfigurationService>> _mockLogger;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;

        public ConfigurationServiceTests()
        {
            // Create a unique test directory for each test run
            _testConfigDirectory = Path.Combine(Path.GetTempPath(), "DerotTests", Guid.NewGuid().ToString(), "config");
            
            // Setup configuration to use test directory
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "DataDirectory", Path.GetDirectoryName(_testConfigDirectory)! }
                })
                .Build();

            _mockLogger = new Mock<ILogger<ConfigurationService>>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            // Setup HttpClient factory to return a client with our mocked handler
            var client = new HttpClient(_mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);

            _configService = new ConfigurationService(config, _mockLogger.Object, _mockHttpClientFactory.Object);
        }

        [Fact]
        public async Task InitializeAsync_CreatesDefaultConfiguration_WhenNotExists()
        {
            // Act
            await _configService.InitializeAsync();

            // Assert
            var configFilePath = Path.Combine(_testConfigDirectory, "app-config.json");
            File.Exists(configFilePath).Should().BeTrue();

            var config = await _configService.GetConfigurationAsync();
            config.Should().NotBeNull();
            config.Id.Should().Be("global");
            config.LLM.Should().NotBeNull();
            config.LLM.Url.Should().Be("http://localhost:11434");
            config.LLM.Port.Should().Be(11434);
            config.LLM.Provider.Should().Be("ollama");
            config.LLM.DefaultModel.Should().Be("llama3:8b");
            config.LLM.TimeoutSeconds.Should().Be(30);
        }

        [Fact]
        public async Task InitializeAsync_IsIdempotent_DoesNotOverwriteExisting()
        {
            // Arrange - Initialize once
            await _configService.InitializeAsync();
            var firstConfig = await _configService.GetConfigurationAsync();
            var firstTimestamp = firstConfig.LastUpdated;

            // Wait a bit to ensure timestamp would be different
            await Task.Delay(100);

            // Act - Initialize again
            await _configService.InitializeAsync();
            var secondConfig = await _configService.GetConfigurationAsync();

            // Assert - Timestamp should be the same (not overwritten)
            secondConfig.LastUpdated.Should().Be(firstTimestamp);
        }

        [Fact]
        public async Task GetConfigurationAsync_ReturnsDefaultConfig_WhenFileNotExists()
        {
            // Act
            var config = await _configService.GetConfigurationAsync();

            // Assert
            config.Should().NotBeNull();
            config.Id.Should().Be("global");
            config.LLM.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateConfigurationAsync_UpdatesConfig_WhenValid()
        {
            // Arrange
            await _configService.InitializeAsync();
            var config = await _configService.GetConfigurationAsync();
            
            config.LLM.Url = "http://localhost:8080";
            config.LLM.Port = 8080;
            config.LLM.DefaultModel = "mistral:7b";

            // Act
            var updatedConfig = await _configService.UpdateConfigurationAsync(config);

            // Assert
            updatedConfig.Should().NotBeNull();
            updatedConfig.LLM.Url.Should().Be("http://localhost:8080");
            updatedConfig.LLM.Port.Should().Be(8080);
            updatedConfig.LLM.DefaultModel.Should().Be("mistral:7b");

            // Verify it was persisted
            var retrievedConfig = await _configService.GetConfigurationAsync();
            retrievedConfig.LLM.Url.Should().Be("http://localhost:8080");
        }

        [Fact]
        public async Task UpdateConfigurationAsync_UpdatesTimestamp()
        {
            // Arrange
            await _configService.InitializeAsync();
            var config = await _configService.GetConfigurationAsync();
            var originalTimestamp = config.LastUpdated;

            await Task.Delay(100); // Ensure time difference

            // Act
            var updatedConfig = await _configService.UpdateConfigurationAsync(config);

            // Assert
            updatedConfig.LastUpdated.Should().BeAfter(originalTimestamp);
        }

        [Fact]
        public async Task UpdateConfigurationAsync_ThrowsException_WhenConfigIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _configService.UpdateConfigurationAsync(null!)
            );
        }

        [Fact]
        public async Task UpdateConfigurationAsync_ThrowsException_WhenIdIsEmpty()
        {
            // Arrange
            var config = new AppConfiguration
            {
                Id = "",
                LLM = new LLMConfiguration { Url = "http://localhost:11434", Port = 11434, Provider = "ollama", DefaultModel = "llama3:8b", TimeoutSeconds = 30 }
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _configService.UpdateConfigurationAsync(config)
            );
        }

        [Fact]
        public async Task GetLLMConfigurationAsync_ReturnsLLMConfig()
        {
            // Arrange
            await _configService.InitializeAsync();

            // Act
            var llmConfig = await _configService.GetLLMConfigurationAsync();

            // Assert
            llmConfig.Should().NotBeNull();
            llmConfig.Url.Should().Be("http://localhost:11434");
            llmConfig.Port.Should().Be(11434);
            llmConfig.Provider.Should().Be("ollama");
        }

        [Fact]
        public async Task UpdateLLMConfigurationAsync_UpdatesOnlyLLMConfig()
        {
            // Arrange
            await _configService.InitializeAsync();
            var originalConfig = await _configService.GetConfigurationAsync();
            var originalId = originalConfig.Id;

            var newLLMConfig = new LLMConfiguration
            {
                Url = "http://localhost:9999",
                Port = 9999,
                Provider = "anythingllm",
                DefaultModel = "qwen2.5:7b",
                TimeoutSeconds = 60
            };

            // Act
            var updatedLLMConfig = await _configService.UpdateLLMConfigurationAsync(newLLMConfig);

            // Assert
            updatedLLMConfig.Should().NotBeNull();
            updatedLLMConfig.Url.Should().Be("http://localhost:9999");
            updatedLLMConfig.Port.Should().Be(9999);

            // Verify full config still has correct ID
            var fullConfig = await _configService.GetConfigurationAsync();
            fullConfig.Id.Should().Be(originalId);
            fullConfig.LLM.Url.Should().Be("http://localhost:9999");
        }

        [Fact]
        public async Task UpdateLLMConfigurationAsync_ThrowsException_WhenUrlIsEmpty()
        {
            // Arrange
            var llmConfig = new LLMConfiguration
            {
                Url = "",
                Port = 11434,
                Provider = "ollama",
                DefaultModel = "llama3:8b",
                TimeoutSeconds = 30
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _configService.UpdateLLMConfigurationAsync(llmConfig)
            );
        }

        [Fact]
        public async Task UpdateLLMConfigurationAsync_ThrowsException_WhenPortIsInvalid()
        {
            // Arrange - Port too low
            var llmConfig1 = new LLMConfiguration
            {
                Url = "http://localhost",
                Port = 0,
                Provider = "ollama",
                DefaultModel = "llama3:8b",
                TimeoutSeconds = 30
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _configService.UpdateLLMConfigurationAsync(llmConfig1)
            );

            // Arrange - Port too high
            var llmConfig2 = new LLMConfiguration
            {
                Url = "http://localhost",
                Port = 70000,
                Provider = "ollama",
                DefaultModel = "llama3:8b",
                TimeoutSeconds = 30
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _configService.UpdateLLMConfigurationAsync(llmConfig2)
            );
        }

        [Fact]
        public async Task UpdateLLMConfigurationAsync_ThrowsException_WhenProviderIsEmpty()
        {
            // Arrange
            var llmConfig = new LLMConfiguration
            {
                Url = "http://localhost:11434",
                Port = 11434,
                Provider = "",
                DefaultModel = "llama3:8b",
                TimeoutSeconds = 30
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _configService.UpdateLLMConfigurationAsync(llmConfig)
            );
        }

        [Fact]
        public async Task UpdateLLMConfigurationAsync_ThrowsException_WhenTimeoutIsInvalid()
        {
            // Arrange
            var llmConfig = new LLMConfiguration
            {
                Url = "http://localhost:11434",
                Port = 11434,
                Provider = "ollama",
                DefaultModel = "llama3:8b",
                TimeoutSeconds = -5
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _configService.UpdateLLMConfigurationAsync(llmConfig)
            );
        }

        [Fact]
        public async Task TestLLMConnectionAsync_ReturnsTrue_WhenConnectionSuccessful()
        {
            // Arrange
            var llmConfig = new LLMConfiguration
            {
                Url = "http://localhost:11434",
                Port = 11434
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Act
            var result = await _configService.TestLLMConnectionAsync(llmConfig);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task TestLLMConnectionAsync_ReturnsFalse_WhenConnectionFailed()
        {
            // Arrange
            var llmConfig = new LLMConfiguration
            {
                Url = "http://localhost:11434",
                Port = 11434
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Connection failed"));

            // Act
            var result = await _configService.TestLLMConnectionAsync(llmConfig);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task TestLLMConnectionAsync_UsesCorrectUrl()
        {
            // Arrange
            var llmConfig = new LLMConfiguration
            {
                Url = "http://localhost:11434",
                Port = 11434
            };

            HttpRequestMessage? capturedRequest = null;

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                 .Callback<HttpRequestMessage, CancellationToken>((r, c) => capturedRequest = r)
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Act
            await _configService.TestLLMConnectionAsync(llmConfig);

            // Assert
            capturedRequest.Should().NotBeNull();
            capturedRequest!.RequestUri!.ToString().Should().StartWith("http://localhost:11434/");
        }

        public void Dispose()
        {
            // Cleanup - Delete test directory
            var testRoot = Path.GetDirectoryName(Path.GetDirectoryName(_testConfigDirectory));
            if (testRoot != null && Directory.Exists(testRoot))
            {
                try
                {
                    Directory.Delete(testRoot, recursive: true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
    }
}
