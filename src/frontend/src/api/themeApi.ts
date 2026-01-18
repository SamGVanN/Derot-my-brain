import { client } from './client';

export interface Theme {
    id: string;
    name: string;
    description: string;
    isDefault: boolean;
    isActive: boolean;
}

export const themeApi = {
    getAllThemes: async (): Promise<Theme[]> => {
        const response = await client.get<Theme[]>('/themes');
        return response.data;
    }
};
