import { useState, useCallback } from 'react';
import { userApi } from '../api/userApi';
import type { User } from '../models/User';

/**
 * Custom hook for User operations.
 * Handles fetching all users and creating/selecting users.
 */
export function useUser() {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const getAllUsers = useCallback(async (): Promise<User[]> => {
        setLoading(true);
        setError(null);
        try {
            const users = await userApi.getAllUsers();
            return users;
        } catch (err) {
            const errorMessage = err instanceof Error ? err.message : 'Failed to fetch users';
            setError(errorMessage);
            throw err;
        } finally {
            setLoading(false);
        }
    }, []);

    const createOrSelectUser = useCallback(async (
        name: string,
        options?: { language?: string; preferredTheme?: string }
    ): Promise<User> => {
        setLoading(true);
        setError(null);
        try {
            const user = await userApi.createOrSelectUser(name, options);
            return user;
        } catch (err) {
            const errorMessage = err instanceof Error ? err.message : 'Failed to create/select user';
            setError(errorMessage);
            throw err;
        } finally {
            setLoading(false);
        }
    }, []);

    return {
        loading,
        error,
        getAllUsers,
        createOrSelectUser
    };
}
