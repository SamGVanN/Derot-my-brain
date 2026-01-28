import { client } from './client';
import type { UserActivity } from '../models/UserActivity';
import type {
    UserStatisticsDto,
    ActivityCalendarDto,
    TopScoreDto
} from '../models/UserStatistics';

export const activityApi = {
    // --- Activities ---

    getActivities: async (userId: string, sourceId?: string): Promise<UserActivity[]> => {
        const params = sourceId ? { sourceId } : undefined;
        const response = await client.get<UserActivity[]>(`/users/${userId}/activities`, { params });
        return response.data;
    },

    getActivity: async (userId: string, activityId: string): Promise<UserActivity> => {
        const response = await client.get<UserActivity>(`/users/${userId}/activities/${activityId}`);
        return response.data;
    },

    createActivity: async (userId: string, activity: Partial<UserActivity>): Promise<UserActivity> => {
        const response = await client.post<UserActivity>(`/users/${userId}/activities`, activity);
        return response.data;
    },

    updateActivity: async (userId: string, activityId: string, activity: Partial<UserActivity>): Promise<UserActivity> => {
        const response = await client.put<UserActivity>(`/users/${userId}/activities/${activityId}`, activity);
        return response.data;
    },

    deleteActivity: async (userId: string, activityId: string): Promise<void> => {
        await client.delete(`/users/${userId}/activities/${activityId}`);
    },

    getExploreArticles: async (userId: string, count: number = 6): Promise<any[]> => {
        const response = await client.get<any[]>(`/users/${userId}/activities/explore/articles`, {
            params: { count }
        });
        return response.data;
    },

    explore: async (userId: string, request: { title?: string, sourceId?: string, sourceType: number, sessionId?: string }): Promise<UserActivity> => {
        const response = await client.post<UserActivity>(`/users/${userId}/activities/explore`, request);
        return response.data;
    },

    read: async (userId: string, request: {
        title: string,
        language?: string,
        sourceId?: string,
        sourceType: number,
        originExploreId?: string,
        backlogAddsCount?: number,
        refreshCount?: number,
        exploreDurationSeconds?: number
    }): Promise<UserActivity> => {
        const response = await client.post<UserActivity>(`/users/${userId}/activities/read`, request);
        return response.data;
    },

    stopExplore: async (userId: string, activityId: string, request: { durationSeconds: number, backlogAddsCount?: number, refreshCount?: number }): Promise<void> => {
        await client.post(`/users/${userId}/activities/${activityId}/stop-explore`, request);
    },

    stopSession: async (userId: string, sessionId: string): Promise<void> => {
        await client.post(`/users/${userId}/sessions/${sessionId}/stop`);
    },

    // --- Statistics ---

    getStatistics: async (userId: string): Promise<UserStatisticsDto> => {
        const response = await client.get<UserStatisticsDto>(`/users/${userId}/statistics`);
        return response.data;
    },

    getActivityCalendar: async (userId: string, days: number = 365): Promise<ActivityCalendarDto[]> => {
        const response = await client.get<ActivityCalendarDto[]>(`/users/${userId}/statistics/activity-calendar`, {
            params: { days }
        });
        return response.data;
    },

    getTopScores: async (userId: string, limit: number = 10): Promise<TopScoreDto[]> => {
        const response = await client.get<TopScoreDto[]>(`/users/${userId}/statistics/top-scores`, {
            params: { limit }
        });
        return response.data;
    }
};
