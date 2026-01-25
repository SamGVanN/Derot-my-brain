import { useState, useCallback } from 'react';
import { activityApi } from '../api/activityApi';
import type { UserActivity } from '../models/UserActivity';

import { useAuthStore } from '../stores/useAuthStore';

export function useActivities() {
    const { user } = useAuthStore();
    const userId = user?.id;
    const [activities, setActivities] = useState<UserActivity[]>([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const refresh = useCallback(async (sourceHash?: string) => {
        if (!userId) return;

        setLoading(true);
        setError(null);
        try {
            const data = await activityApi.getActivities(userId, sourceHash);
            setActivities(data);
        } catch (err) {
            console.error('Failed to fetch activities:', err);
            setError('Failed to fetch activities');
        } finally {
            setLoading(false);
        }
    }, [userId]);

    const createActivity = useCallback(async (activity: Partial<UserActivity>) => {
        if (!userId) throw new Error("User not authenticated");

        try {
            const newActivity = await activityApi.createActivity(userId, activity);
            setActivities(prev => [newActivity, ...prev]);
            return newActivity;
        } catch (err) {
            console.error('Failed to create activity:', err);
            throw err;
        }
    }, [userId]);

    const updateActivity = useCallback(async (activityId: string, updates: Partial<UserActivity>) => {
        if (!userId) throw new Error("User not authenticated");

        try {
            const updatedActivity = await activityApi.updateActivity(userId, activityId, updates);
            setActivities(prev => prev.map(a => a.id === activityId ? updatedActivity : a));
            return updatedActivity;
        } catch (err) {
            console.error('Failed to update activity:', err);
            throw err;
        }
    }, [userId]);

    const deleteActivity = useCallback(async (activityId: string) => {
        if (!userId) throw new Error("User not authenticated");

        try {
            await activityApi.deleteActivity(userId, activityId);
            setActivities(prev => prev.filter(a => a.id !== activityId));
        } catch (err) {
            console.error('Failed to delete activity:', err);
            throw err;
        }
    }, [userId]);

    const getActivity = useCallback(async (activityId: string) => {
        if (!userId) throw new Error("User not authenticated");
        return await activityApi.getActivity(userId, activityId);
    }, [userId]);

    return {
        activities,
        loading,
        error,
        refresh,
        createActivity,
        updateActivity,
        deleteActivity,
        getActivity
    };
}
