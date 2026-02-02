using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.Interfaces.Services;

/// <summary>
/// Interface for configuration service
/// </summary>
public interface IConfigurationService
{
    Task InitializeAsync();
    Task<AppConfiguration> GetConfigurationAsync();
    Task<AppConfiguration> UpdateConfigurationAsync(AppConfiguration config);
    Task<LLMConfiguration> GetLLMConfigurationAsync();
    Task<LLMConfiguration> UpdateLLMConfigurationAsync(LLMConfiguration llmConfig);
    Task<bool> TestLLMConnectionAsync(LLMConfiguration config);
    Task<AppConfiguration> ResetToDefaultAsync();
}
