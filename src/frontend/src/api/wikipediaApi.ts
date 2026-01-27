import { client } from './client';

export interface WikipediaArticle {
    title: string;
    summary?: string;
    lang?: string;
    sourceUrl?: string;
    imageUrl?: string;
}

export const wikipediaApi = {
    addToBacklog: async (userId: string, article: WikipediaArticle): Promise<{ id: string }> => {
        const response = await client.post<{ id: string }>(`/users/${userId}/backlog`, {
            sourceId: article.sourceUrl || article.title,
            sourceType: 1, // Wikipedia
            title: article.title,
            url: article.sourceUrl,
            provider: 'Wikipedia'
        });
        return response.data;
    }
};
