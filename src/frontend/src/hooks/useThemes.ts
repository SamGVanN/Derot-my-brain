import { useState, useEffect } from 'react';
import axios from 'axios';
import type { Theme } from '../models/SeedData';

const API_URL = 'http://localhost:5077/api';

/**
 * React hook to fetch and cache UI themes.
 * Themes are the 5 available color palettes for the application.
 */
export function useThemes() {
    const [themes, setThemes] = useState<Theme[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchThemes = async () => {
            try {
                setLoading(true);
                setError(null);

                const response = await axios.get<Theme[]>(`${API_URL}/themes`);
                setThemes(response.data);
            } catch (err) {
                const errorMessage = err instanceof Error ? err.message : 'Failed to fetch themes';
                setError(errorMessage);
                console.error('Error fetching themes:', err);
            } finally {
                setLoading(false);
            }
        };

        fetchThemes();
    }, []);

    return { themes, loading, error };
}
