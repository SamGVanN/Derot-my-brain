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

    const trackTopic = useCallback(async (topic: string, wikipediaUrl: string) => {
        if (!userId) throw new Error("User not authenticated");

        try {
            const newTopic = await activityApi.trackTopic(userId, topic, wikipediaUrl);
            setTrackedTopics(prev => [...prev, newTopic]);
            return newTopic;
        } catch (err) {
            console.error('Failed to track topic:', err);
            throw err;
        }
    }, [userId]);

    const untrackTopic = useCallback(async (topic: string) => {
        if (!userId) throw new Error("User not authenticated");

        try {
            await activityApi.untrackTopic(userId, topic);
            setTrackedTopics(prev => prev.filter(t => t.topic !== topic));
        } catch (err) {
            console.error('Failed to untrack topic:', err);
            throw err;
        }
    }, [userId]);

    const isTracked = useCallback((topic: string) => {
        return trackedTopics.some(t => t.topic === topic);
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
