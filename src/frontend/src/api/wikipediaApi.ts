import { client } from './client';

export interface WikipediaArticle {
    title: string;
    summary?: string;
    lang?: string;
    sourceUrl?: string;
}

export const wikipediaApi = {
    explore: async (): Promise<{ id: string }> => {
        const response = await client.post<{ id: string }>('/wikipedia/explore', {});
        return response.data;
    },

    addToBacklog: async (article: WikipediaArticle): Promise<{ id: string }> => {
        const response = await client.post<{ id: string }>('/backlog', article);
        return response.data;
    },

    read: async (request: {
        title: string;
        language?: string;
        sourceUrl?: string;
        originExploreId?: string;
        backlogAddsCount?: number;
    }): Promise<{ activity: { id: string, userId: string, type: string } }> => {
        const response = await client.post<{ activity: { id: string, userId: string, type: string } }>('/wikipedia/read', request);
        return response.data;
    }
};
