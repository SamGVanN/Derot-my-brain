import { client } from './client';
import type { UserActivity } from '../models/UserActivity';
import type {
    UserStatisticsDto,
    ActivityCalendarDto,
    TopScoreDto
} from '../models/UserStatistics';

export const activityApi = {
    // --- Activities ---

    getActivities: async (userId: string, sourceHash?: string): Promise<UserActivity[]> => {
        const params = sourceHash ? { sourceHash } : undefined;
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
