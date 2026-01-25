import { client } from './client';

export interface BacklogItemDto {
    id: string;
    userId: string;
    sourceId: string;
    sourceType: 'Wikipedia' | 'Document' | 'Custom';
    sourceHash: string;
    title: string;
    addedAt: string;
}

export interface AddToBacklogDto {
    sourceId: string;
    sourceType: 'Wikipedia' | 'Document' | 'Custom';
    title: string;
}

export const backlogApi = {
    getAll: async (userId: string): Promise<BacklogItemDto[]> => {
        const response = await client.get<BacklogItemDto[]>(`/users/${userId}/backlog`);
        return response.data;
    },

    add: async (userId: string, item: AddToBacklogDto): Promise<BacklogItemDto> => {
        const response = await client.post<BacklogItemDto>(`/users/${userId}/backlog`, item);
        return response.data;
    },

    remove: async (userId: string, sourceHash: string): Promise<void> => {
        await client.delete(`/users/${userId}/backlog/${sourceHash}`);
    }
};
