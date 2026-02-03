import { client } from './client';
import type { UserSession } from '../models/UserSession';

export const sessionApi = {
    getSessions: async (userId: string): Promise<UserSession[]> => {
        const response = await client.get<UserSession[]>(`/users/${userId}/sessions`);
        return response.data;
    },

    getSession: async (userId: string, sessionId: string): Promise<UserSession> => {
        const response = await client.get<UserSession>(`/users/${userId}/sessions/${sessionId}`);
        return response.data;
    }
};
