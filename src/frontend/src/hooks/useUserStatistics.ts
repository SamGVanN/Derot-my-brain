import { useState, useCallback } from 'react';
import { activityApi } from '../api/activityApi';
import type { UserStatisticsDto, ActivityCalendarDto, TopScoreDto } from '../models/UserStatistics';

import { useAuthStore } from '../stores/useAuthStore';

export function useUserStatistics() {
    const { user } = useAuthStore();
    const userId = user?.id;
    const [statistics, setStatistics] = useState<UserStatisticsDto | null>(null);
    const [activityCalendar, setActivityCalendar] = useState<ActivityCalendarDto[]>([]);
    const [topScores, setTopScores] = useState<TopScoreDto[]>([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const refreshStatistics = useCallback(async () => {
        if (!userId) return;

        setLoading(true);
        setError(null);
        try {
            const stats = await activityApi.getStatistics(userId);
            setStatistics(stats);
        } catch (err) {
            console.error('Failed to fetch statistics:', err);
            setError('Failed to fetch statistics');
        } finally {
            setLoading(false);
        }
    }, [userId]);

    const refreshCalendar = useCallback(async (days: number = 365) => {
        if (!userId) return;

        try {
            const calendar = await activityApi.getActivityCalendar(userId, days);
            setActivityCalendar(calendar);
        } catch (err) {
            console.error('Failed to fetch activity calendar:', err);
            // Don't set global error for partial failures if possible, or handle individually
        }
    }, [userId]);

    const refreshTopScores = useCallback(async (limit: number = 10) => {
        if (!userId) return;

        try {
            const scores = await activityApi.getTopScores(userId, limit);
            setTopScores(scores);
        } catch (err) {
            console.error('Failed to fetch top scores:', err);
        }
    }, [userId]);

    const refreshAll = useCallback(async () => {
        setLoading(true);
        await Promise.all([
            refreshStatistics(),
            refreshCalendar(),
            refreshTopScores()
        ]);
        setLoading(false);
    }, [refreshStatistics, refreshCalendar, refreshTopScores]);

    return {
        statistics,
        activityCalendar,
        topScores,
        loading,
        error,
        refreshStatistics,
        refreshCalendar,
        refreshTopScores,
        refreshAll
    };
}
