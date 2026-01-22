import { useState, useCallback } from 'react';
import { activityApi } from '../api/activityApi';
import { useAuthStore } from '../stores/useAuthStore';
import type { UserActivity } from '../models/UserActivity';

/**
 * Custom hook for History operations.
 * Handles fetching and adding user activities.
 */
export function useHistory() {
    const { user } = useAuthStore();
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const fetchHistory = useCallback(async (): Promise<UserActivity[]> => {
        if (!user?.id) {
            return [];
        }

        setLoading(true);
        setError(null);
        try {
            return await activityApi.getActivities(user.id);
        } catch (err) {
            const errorMessage = err instanceof Error ? err.message : 'Failed to fetch history';
            setError(errorMessage);
            throw err;
        } finally {
            setLoading(false);
        }
    }, [user?.id]);

    const addActivity = useCallback(async (activity: Partial<UserActivity>): Promise<UserActivity> => {
        if (!user?.id) {
            throw new Error("User not authenticated");
        }

        setLoading(true);
        setError(null);
        try {
            return await activityApi.createActivity(user.id, activity);
        } catch (err) {
            const errorMessage = err instanceof Error ? err.message : 'Failed to add activity';
            setError(errorMessage);
            throw err;
        } finally {
            setLoading(false);
        }
    }, [user?.id]);

    return {
        loading,
        error,
        fetchHistory,
        addActivity
    };
}
