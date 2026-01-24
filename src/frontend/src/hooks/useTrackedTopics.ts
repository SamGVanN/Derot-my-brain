import { useState, useCallback } from 'react';
import { activityApi } from '../api/activityApi';
import type { TrackedTopicDto } from '../models/UserStatistics';

import { useAuthStore } from '../stores/useAuthStore';

export function useTrackedTopics() {
    const { user } = useAuthStore();
    const userId = user?.id;
    const [trackedTopics, setTrackedTopics] = useState<TrackedTopicDto[]>([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const refresh = useCallback(async () => {
        if (!userId) return;

        setLoading(true);
        setError(null);
        try {
            const data = await activityApi.getTrackedTopics(userId);
            setTrackedTopics(data);
        } catch (err) {
            console.error('Failed to fetch tracked topics:', err);
            setError('Failed to fetch tracked topics');
        } finally {
            setLoading(false);
        }
    }, [userId]);

    const trackTopic = useCallback(async (title: string, wikipediaUrl: string) => {
        if (!userId) throw new Error("User not authenticated");

        try {
            const newTopic = await activityApi.trackTopic(userId, title, wikipediaUrl);
            setTrackedTopics(prev => [...prev, newTopic]);
            return newTopic;
        } catch (err) {
            console.error('Failed to track topic:', err);
            throw err;
        }
    }, [userId]);

    const untrackTopic = useCallback(async (title: string) => {
        if (!userId) throw new Error("User not authenticated");

        try {
            await activityApi.untrackTopic(userId, title);
            setTrackedTopics(prev => prev.filter(t => t.title !== title));
        } catch (err) {
            console.error('Failed to untrack topic:', err);
            throw err;
        }
    }, [userId]);

    const isTracked = useCallback((title: string) => {
        return trackedTopics.some(t => t.title === title);
    }, [trackedTopics]);

    return {
        trackedTopics,
        loading,
        error,
        refresh,
        trackTopic,
        untrackTopic,
        isTracked
    };
}
