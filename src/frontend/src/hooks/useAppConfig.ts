import { useState, useEffect, useCallback } from 'react';
import { client } from '../api/client';
import type { AppConfiguration, LLMConfiguration } from '../models/Configuration';

/**
 * React hook to fetch and update global application configuration.
 * Configuration is shared across all users.
 */
export function useAppConfig() {
    const [config, setConfig] = useState<AppConfiguration | null>(null);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [updating, setUpdating] = useState<boolean>(false);

    // Fetch configuration
    const fetchConfig = useCallback(async () => {
        try {
            setLoading(true);
            setError(null);

            const response = await client.get<AppConfiguration>(`/global-config`);
            setConfig(response.data);
        } catch (err) {
            const errorMessage = err instanceof Error ? err.message : 'Failed to fetch configuration';
            setError(errorMessage);
            console.error('Error fetching configuration:', err);
        } finally {
            setLoading(false);
        }
    }, []);

    // Update full configuration
    const updateConfig = useCallback(async (newConfig: AppConfiguration): Promise<boolean> => {
        try {
            setUpdating(true);
            setError(null);

            const response = await client.put<AppConfiguration>(`/global-config`, newConfig);
            setConfig(response.data);
            return true;
        } catch (err) {
            const errorMessage = err instanceof Error ? err.message : 'Failed to update configuration';
            setError(errorMessage);
            console.error('Error updating configuration:', err);
            return false;
        } finally {
            setUpdating(false);
        }
    }, []);

    // Update LLM configuration only
    const updateLLMConfig = useCallback(async (llmConfig: LLMConfiguration): Promise<boolean> => {
        try {
            setUpdating(true);
            setError(null);

            const response = await client.put<LLMConfiguration>(`/global-config/llm`, llmConfig);

            // Update local state
            if (config) {
                setConfig({
                    ...config,
                    llm: response.data,
                    lastUpdated: new Date().toISOString()
                });
            }

            return true;
        } catch (err) {
            const errorMessage = err instanceof Error ? err.message : 'Failed to update LLM configuration';
            setError(errorMessage);
            console.error('Error updating LLM configuration:', err);
            return false;
        } finally {
            setUpdating(false);
        }
    }, [config]);

    // Test LLM Connection
    const testLLMConnection = useCallback(async (llmConfig: LLMConfiguration): Promise<{ success: boolean; message: string }> => {
        try {
            // We use POST here as per backend implementation
            const response = await client.post<{ success: boolean; message: string }>(`/global-config/llm/test`, llmConfig);
            return response.data;
        } catch (err: any) {
            console.error('Error testing LLM connection:', err);
            const message = err.response?.data?.message || err.message || 'Failed to test connection';
            return { success: false, message };
        }
    }, []);

    useEffect(() => {
        fetchConfig();
    }, [fetchConfig]);

    return {
        config,
        loading,
        error,
        updating,
        updateConfig,
        updateLLMConfig,
        testLLMConnection,
        refetch: fetchConfig
    };
}
