import { client } from './client';
import type { User } from '../models/User';


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
    },

    updateDerotZonePreferences: async (userId: string, preferences: { questionCount: number; selectedCategories: string[] }): Promise<User> => {
        const response = await client.patch<User>(`/users/${userId}/preferences/derot-zone`, preferences);
        return response.data;
    },

    updateUser: async (userId: string, data: { name: string }): Promise<User> => {
        const response = await client.put<User>(`/users/${userId}`, data);
        return response.data;
    },

    deleteUser: async (userId: string): Promise<void> => {
        await client.delete(`/users/${userId}`);
    }
};
