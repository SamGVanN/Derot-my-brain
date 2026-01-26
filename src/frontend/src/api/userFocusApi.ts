import { client } from './client';
import type { UserFocus, TrackTopicRequest } from '../models/UserFocus';
import type { UserActivity } from '../models/UserActivity';

export const userFocusApi = {
    getUserFocuses: async (userId: string): Promise<UserFocus[]> => {
        const response = await client.get<UserFocus[]>(`/users/${userId}/user-focus`);
        return response.data;
    },

    getUserFocus: async (userId: string, sourceId: string): Promise<UserFocus> => {
        const response = await client.get<UserFocus>(`/users/${userId}/user-focus/${sourceId}`);
        return response.data;
    },

    trackTopic: async (userId: string, request: TrackTopicRequest): Promise<UserFocus> => {
        const response = await client.post<UserFocus>(`/users/${userId}/user-focus`, request);
        return response.data;
    },

    untrackTopic: async (userId: string, sourceId: string): Promise<void> => {
        await client.delete(`/users/${userId}/user-focus/${sourceId}`);
    },

    getFocusEvolution: async (userId: string, sourceId: string): Promise<UserActivity[]> => {
        const response = await client.get<UserActivity[]>(`/users/${userId}/user-focus/${sourceId}/evolution`);
        return response.data;
    },

    togglePin: async (userId: string, sourceId: string): Promise<UserFocus> => {
        const response = await client.patch<UserFocus>(`/users/${userId}/user-focus/${sourceId}/pin`);
        return response.data;
    },

    toggleArchive: async (userId: string, sourceId: string): Promise<UserFocus> => {
        const response = await client.patch<UserFocus>(`/users/${userId}/user-focus/${sourceId}/archive`);
        return response.data;
    }
};
