using DerotMyBrain.Core.Entities;
using System.Text.Json;
using DerotMyBrain.Core.Interfaces.Services;
using DerotMyBrain.Core.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DerotMyBrain.Infrastructure.Services;

/// <summary>
/// Service responsible for managing global application configuration.
/// Configuration is stored in the database, with JSON file used only for defaults.
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly string _configDirectory;
    private readonly ILogger<ConfigurationService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfigurationRepository _configRepository;
    private const string ConfigFileName = "app-config.json";
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public ConfigurationService(
        IConfiguration configuration, 
        ILogger<ConfigurationService> logger, 
        IHttpClientFactory httpClientFactory,
        IConfigurationRepository configRepository)
    {
        var dataDirectory = configuration["DataDirectory"] ?? "Data";
        _configDirectory = Path.Combine(dataDirectory, "config");
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configRepository = configRepository;
    }

    /// <summary>
    /// Initialize configuration with default values if not present in database.
    /// This method is idempotent - safe to call multiple times.
    /// </summary>
    public async Task InitializeAsync()
    {
        _logger.LogInformation("Starting configuration initialization...");

        await _semaphore.WaitAsync();
        try
        {
            // Check if configuration exists in database
            var existingConfig = await _configRepository.GetAsync();
            if (existingConfig != null)
            {
                _logger.LogInformation("Configuration already exists in database. Skipping initialization.");
                return;
            }

            // Load default configuration from JSON file
            var defaultConfig = await LoadDefaultConfigurationFromFileAsync();
            
            // Save to database
            await _configRepository.SaveAsync(defaultConfig);
            
            _logger.LogInformation("Configuration initialized in database with default values");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Get the global application configuration from database
    /// </summary>
    public async Task<AppConfiguration> GetConfigurationAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            return await GetConfigurationInternalAsync();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Internal method to get configuration without semaphore.
    /// Used by other methods that already hold the lock.
    /// </summary>
    private async Task<AppConfiguration> GetConfigurationInternalAsync()
    {
        // Try to get from database first
        var config = await _configRepository.GetAsync();
        
        if (config == null)
        {
            _logger.LogWarning("Configuration not found in database. Loading defaults and saving...");
            
            // Load from JSON file and save to database
            config = await LoadDefaultConfigurationFromFileAsync();
            await _configRepository.SaveAsync(config);
        }

        return config;
    }

    /// <summary>
    /// Update the global application configuration in database
    /// </summary>
    public async Task<AppConfiguration> UpdateConfigurationAsync(AppConfiguration config)
    {
        await _semaphore.WaitAsync();
        try
        {
            // Validate configuration
            ValidateConfiguration(config);

            // Update timestamp
            config.LastUpdated = DateTime.UtcNow;

            // Save to database
            await _configRepository.SaveAsync(config);

            _logger.LogInformation("Configuration updated successfully in database");
            return config;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Get LLM configuration only
    /// </summary>
    public async Task<LLMConfiguration> GetLLMConfigurationAsync()
    {
        var config = await GetConfigurationAsync();
        return config.LLM;
    }

    /// <summary>
    /// Update LLM configuration only
    /// </summary>
    public async Task<LLMConfiguration> UpdateLLMConfigurationAsync(LLMConfiguration llmConfig)
    {
        await _semaphore.WaitAsync();
        try
        {
            // Validate LLM configuration
            ValidateLLMConfiguration(llmConfig);

            var config = await GetConfigurationInternalAsync();
            config.LLM = llmConfig;
            config.LastUpdated = DateTime.UtcNow;

            await _configRepository.SaveAsync(config);

            _logger.LogInformation("LLM configuration updated successfully in database");
            return llmConfig;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Test the connection to the LLM server
    /// </summary>
    public async Task<bool> TestLLMConnectionAsync(LLMConfiguration config)
    {
        ValidateLLMConfiguration(config);

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5); // Fast test timeout

            var url = config.GetFullUrl();

            _logger.LogInformation("Testing LLM connection at {Url}", url);

            // Try to hit the root endpoint or a lightweight status endpoint
            // For Ollama, GET / is usually OK or returns 200
            var response = await client.GetAsync(url);
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "LLM connection test failed");
            return false;
        }
    }

    /// <summary>
    /// Reset configuration to default values from JSON file
    /// </summary>
    public async Task<AppConfiguration> ResetToDefaultAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            _logger.LogInformation("Resetting configuration to defaults...");

            // Delete existing configuration from database
            await _configRepository.DeleteAsync();

            // Load defaults from JSON file
            var defaultConfig = await LoadDefaultConfigurationFromFileAsync();

            // Save to database
            await _configRepository.SaveAsync(defaultConfig);

            _logger.LogInformation("Configuration reset to defaults successfully");
            return defaultConfig;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Load default configuration from JSON file
    /// </summary>
    private async Task<AppConfiguration> LoadDefaultConfigurationFromFileAsync()
    {
        var filePath = Path.Combine(_configDirectory, ConfigFileName);

        // Ensure config directory exists
        if (!Directory.Exists(_configDirectory))
        {
            Directory.CreateDirectory(_configDirectory);
            _logger.LogInformation("Created config directory: {Directory}", _configDirectory);
        }

        // If JSON file doesn't exist, create it with defaults
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Configuration file not found. Creating default configuration file...");
            var defaultConfig = CreateDefaultConfiguration();
            var json = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
            return defaultConfig;
        }

        // Read from JSON file
        var jsonContent = await File.ReadAllTextAsync(filePath);
        var config = JsonSerializer.Deserialize<AppConfiguration>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return config ?? CreateDefaultConfiguration();
    }

    /// <summary>
    /// Create default configuration with standard LLM settings
    /// </summary>
    private AppConfiguration CreateDefaultConfiguration()
    {
        return new AppConfiguration
        {
            Id = "global",
            LLM = new LLMConfiguration
            {
                Url = "127.0.0.1",
                Port = 11434,
                Provider = "ollama",
                DefaultModel = "llama3:8b",
                TimeoutSeconds = 30
            },
            LastUpdated = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Validate configuration data
    /// </summary>
    private void ValidateConfiguration(AppConfiguration config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config), "Configuration cannot be null");

        if (string.IsNullOrWhiteSpace(config.Id))
            throw new ArgumentException("Configuration ID cannot be empty", nameof(config));

        if (config.LLM == null)
            throw new ArgumentException("LLM configuration cannot be null", nameof(config));

        ValidateLLMConfiguration(config.LLM);
    }

    /// <summary>
    /// Validate LLM configuration data
    /// </summary>
    private void ValidateLLMConfiguration(LLMConfiguration llmConfig)
    {
        if (llmConfig == null)
            throw new ArgumentNullException(nameof(llmConfig), "LLM configuration cannot be null");

        if (string.IsNullOrWhiteSpace(llmConfig.Url))
            throw new ArgumentException("LLM URL cannot be empty", nameof(llmConfig));

        if (llmConfig.Port <= 0 || llmConfig.Port > 65535)
            throw new ArgumentException("LLM Port must be between 1 and 65535", nameof(llmConfig));

        if (string.IsNullOrWhiteSpace(llmConfig.Provider))
            throw new ArgumentException("LLM Provider cannot be empty", nameof(llmConfig));

        if (string.IsNullOrWhiteSpace(llmConfig.DefaultModel))
            throw new ArgumentException("LLM DefaultModel cannot be empty", nameof(llmConfig));

        if (llmConfig.TimeoutSeconds <= 0)
            throw new ArgumentException("LLM TimeoutSeconds must be greater than 0", nameof(llmConfig));
    }
}
