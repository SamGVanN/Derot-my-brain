/**
 * LLM (Large Language Model) configuration
 */
export interface LLMConfiguration {
    /** LLM server URL (e.g., "http://localhost:11434") */
    url: string;
    /** LLM server port */
    port: number;
    /** LLM provider type: "ollama", "anythingllm", or "openai" */
    provider: string;
    /** Default model to use (e.g., "llama3:8b") */
    defaultModel: string;
    /** Request timeout in seconds */
    timeoutSeconds: number;
}

/**
 * Global application configuration shared across all users
 */
export interface AppConfiguration {
    /** Configuration identifier (always "global") */
    id: string;
    /** LLM configuration settings */
    llm: LLMConfiguration;
    /** Last time this configuration was updated */
    lastUpdated: string;
}
