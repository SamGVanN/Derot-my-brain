using DerotMyBrain.API.Models;
using System.Text.Json;

namespace DerotMyBrain.API.Services
{
    /// <summary>
    /// Service responsible for managing global application configuration.
    /// Configuration is shared across all users and stored in /data/config/
    /// </summary>
    public class ConfigurationService : IConfigurationService
    {
        private readonly string _configDirectory;
        private readonly ILogger<ConfigurationService> _logger;
        private const string ConfigFileName = "app-config.json";
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public ConfigurationService(IConfiguration configuration, ILogger<ConfigurationService> logger)
        {
            var dataDirectory = configuration["DataDirectory"] ?? "data";
            _configDirectory = Path.Combine(dataDirectory, "config");
            _logger = logger;
        }

        /// <summary>
        /// Initialize configuration with default values if not present.
        /// This method is idempotent - safe to call multiple times.
        /// </summary>
        public async Task InitializeAsync()
        {
            _logger.LogInformation("Starting configuration initialization...");

            // Ensure config directory exists
            if (!Directory.Exists(_configDirectory))
            {
                Directory.CreateDirectory(_configDirectory);
                _logger.LogInformation("Created config directory: {Directory}", _configDirectory);
            }

            var filePath = Path.Combine(_configDirectory, ConfigFileName);

            // Skip if already exists (idempotent)
            if (File.Exists(filePath))
            {
                _logger.LogInformation("Configuration already initialized. Skipping.");
                return;
            }

            // Create default configuration
            var defaultConfig = CreateDefaultConfiguration();
            await SaveConfigurationAsync(defaultConfig);

            _logger.LogInformation("Configuration initialized with default values at {FilePath}", filePath);
        }

        /// <summary>
        /// Get the global application configuration
        /// </summary>
        public async Task<AppConfiguration> GetConfigurationAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                var filePath = Path.Combine(_configDirectory, ConfigFileName);

                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("Configuration file not found. Creating default configuration...");
                    await InitializeAsync();
                }

                var json = await File.ReadAllTextAsync(filePath);
                var config = JsonSerializer.Deserialize<AppConfiguration>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return config ?? CreateDefaultConfiguration();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Update the global application configuration
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

                await SaveConfigurationAsync(config);

                _logger.LogInformation("Configuration updated successfully");
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

                var config = await GetConfigurationAsync();
                config.LLM = llmConfig;
                config.LastUpdated = DateTime.UtcNow;

                await SaveConfigurationAsync(config);

                _logger.LogInformation("LLM configuration updated successfully");
                return llmConfig;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Save configuration to file
        /// </summary>
        private async Task SaveConfigurationAsync(AppConfiguration config)
        {
            var filePath = Path.Combine(_configDirectory, ConfigFileName);
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
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
                    Url = "http://localhost:11434",
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
}
