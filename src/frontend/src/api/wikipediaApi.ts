import { client } from './client';

export interface WikipediaArticle {
    title: string;
    summary?: string;
    lang?: string;
    sourceUrl?: string;
}

export const wikipediaApi = {
    addToBacklog: async (article: WikipediaArticle): Promise<{ id: string }> => {
        const response = await client.post<{ id: string }>('/backlog', article);
        return response.data;
    }
};
