import { client } from './client';
import type { User } from '../models/User';
import type { UserActivity } from '../models/UserActivity';

export const userApi = {
    getAllUsers: async (): Promise<User[]> => {
        const response = await client.get<User[]>('/users');
        return response.data;
    },

    createOrSelectUser: async (name: string, options?: { language?: string; preferredTheme?: string }): Promise<User> => {
        const response = await client.post<User>('/users', { name, ...options });
        return response.data;
    },

    getUserById: async (id: string): Promise<User> => {
        const response = await client.get<User>(`/users/${id}`);
        return response.data;
    },

    getHistory: async (userId: string): Promise<UserActivity[]> => {
        const response = await client.get<UserActivity[]>(`/users/${userId}/history`);
        return response.data;
    },

    addActivity: async (userId: string, activity: Partial<UserActivity>): Promise<UserActivity> => {
        const response = await client.post<UserActivity>(`/users/${userId}/history`, activity);
        return response.data;
    },

    updatePreferences: async (userId: string, preferences: Partial<User['preferences']>): Promise<User> => {
        const response = await client.put<User>(`/users/${userId}/preferences`, preferences);
        return response.data;
    },

    getPreferences: async (userId: string): Promise<User['preferences']> => {
        const response = await client.get<User['preferences']>(`/users/${userId}/preferences`);
        return response.data;
    },

    updateGeneralPreferences: async (userId: string, preferences: {
        language: string;
        preferredTheme: string;
        questionCount: number;
    }): Promise<User> => {
        const response = await client.patch<User>(`/users/${userId}/preferences/general`, preferences);
        return response.data;
    },

    updateCategoryPreferences: async (userId: string, selectedCategories: string[]): Promise<User> => {
        const response = await client.patch<User>(`/users/${userId}/preferences/categories`, { selectedCategories });
        return response.data;
    }
};
