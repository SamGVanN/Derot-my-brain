import { useState, useEffect } from 'react';
import type { WikipediaCategory } from '../models/SeedData';
import { categoryApi } from '../api/categoryApi';

/**
 * React hook to fetch and cache Wikipedia categories.
 * Categories are the 13 official Wikipedia main categories.
 */
export function useCategories() {
    const [categories, setCategories] = useState<WikipediaCategory[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchCategories = async () => {
            try {
                setLoading(true);
                setError(null);

                const data = await categoryApi.getAllCategories();
                setCategories(data);
            } catch (err) {
                const errorMessage = err instanceof Error ? err.message : 'Failed to fetch categories';
                setError(errorMessage);
                console.error('Error fetching categories:', err);
            } finally {
                setLoading(false);
            }
        };

        fetchCategories();
    }, []);

    return { categories, loading, error };
}
