import axios from 'axios';
import type { User, CreateUserRequest } from '../models/User';

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
    }
};
