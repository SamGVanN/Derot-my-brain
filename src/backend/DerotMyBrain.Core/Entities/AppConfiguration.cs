namespace DerotMyBrain.Core.Entities;

/// <summary>
/// Global application configuration shared across all users.
/// </summary>
public class AppConfiguration
{
    /// <summary>
    /// Configuration identifier (always "global")
    /// </summary>
    public string Id { get; set; } = "global";

    /// <summary>
    /// LLM configuration settings
    /// </summary>
    public LLMConfiguration LLM { get; set; } = new();

    /// <summary>
    /// Last time this configuration was updated
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Configuration for LLM (Large Language Model) integration.
/// </summary>
public class LLMConfiguration
{
    /// <summary>
    /// LLM server URL (e.g., "http://localhost:11434")
    /// </summary>
    public string Url { get; set; } = "http://127.0.0.1:11434";

    /// <summary>
    /// LLM server port
    /// </summary>
    public int Port { get; set; } = 11434;

    /// <summary>
    /// LLM provider type: "ollama", "anythingllm", or "openai"
    /// </summary>
    public string Provider { get; set; } = "ollama";

    /// <summary>
    /// Default model to use (e.g., "llama3:8b")
    /// </summary>
    public string DefaultModel { get; set; } = "llama3:8b";

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}
