import { useState, useCallback } from 'react';
import { userFocusApi } from '../api/userFocusApi';
import type { UserFocus, TrackTopicRequest } from '../models/UserFocus';
import { useAuthStore } from '../stores/useAuthStore';

export function useUserFocus() {
    const { user } = useAuthStore();
    const userId = user?.id;
    const [userFocuses, setUserFocuses] = useState<UserFocus[]>([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const refresh = useCallback(async () => {
        if (!userId) return;

        setLoading(true);
        setError(null);
        try {
            const data = await userFocusApi.getUserFocuses(userId);
            setUserFocuses(data);
        } catch (err) {
            console.error('Failed to fetch user focus areas:', err);
            setError('Failed to fetch user focus areas');
        } finally {
            setLoading(false);
        }
    }, [userId]);

    const trackSource = useCallback(async (request: TrackTopicRequest) => {
        if (!userId) throw new Error("User not authenticated");

        try {
            const newFocus = await userFocusApi.trackTopic(userId, request);
            setUserFocuses(prev => [...prev, newFocus]);
            return newFocus;
        } catch (err) {
            console.error('Failed to track source:', err);
            throw err;
        }
    }, [userId]);

    const untrackSource = useCallback(async (id: string) => {
        if (!userId) throw new Error("User not authenticated");

        try {
            await userFocusApi.untrackTopic(userId, id);
            setUserFocuses(prev => prev.filter(t => t.id !== id));
        } catch (err) {
            console.error('Failed to untrack source:', err);
            throw err;
        }
    }, [userId]);

    const isTracked = useCallback((sourceId: string) => {
        return userFocuses.some(t => t.sourceId === sourceId);
    }, [userFocuses]);

    const findBySourceId = useCallback((sourceId: string) => {
        return userFocuses.find(t => t.sourceId === sourceId);
    }, [userFocuses]);

    return {
        userFocuses,
        loading,
        error,
        refresh,
        trackSource,
        untrackSource,
        isTracked,
        findBySourceId
    };
}
