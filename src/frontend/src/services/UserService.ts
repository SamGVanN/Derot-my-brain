import axios from 'axios';
import type { User, CreateUserRequest } from '../models/User';
import type { UserActivity } from '../models/UserActivity';


const API_URL = 'http://localhost:5077/api'; // Default .NET API URL, check launchSettings.json usually

const api = axios.create({
    baseURL: API_URL,
});

export const UserService = {
    getAllUsers: async (): Promise<User[]> => {
        const response = await api.get<User[]>('/users');
        return response.data;
    },

    createOrSelectUser: async (name: string): Promise<User> => {
        const request: CreateUserRequest = { name };
        const response = await api.post<User>('/users', request);
        return response.data;
    },

    getUserById: async (id: string): Promise<User> => {
        const response = await api.get<User>(`/users/${id}`);
        return response.data;
    },

    getHistory: async (userId: string): Promise<UserActivity[]> => {
        const response = await api.get<UserActivity[]>(`/users/${userId}/history`);
        return response.data;
    },

    addActivity: async (userId: string, activity: Partial<UserActivity>): Promise<UserActivity> => {
        const response = await api.post<UserActivity>(`/users/${userId}/history`, activity);
        return response.data;
    },

    updatePreferences: async (userId: string, preferences: Partial<User['preferences']>): Promise<User> => {
        // We'll need a specific endpoint or just partial update to user
        // However, roadmap says PUT /api/users/{id}/preferences
        const response = await api.put<User>(`/users/${userId}/preferences`, preferences);
        return response.data;
    }

};

