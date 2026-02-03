import { client } from './client';
import { ContentExtractionStatus } from '@/models/ContentExtractionStatus';
import type { ContentExtractionStatusDto } from '@/models/ContentExtractionStatus';

export interface DocumentDto {
    id: string;
    userId: string;
    fileName: string;
    fileType: string;
    fileSize: number;
    uploadDate: string;
    displayTitle: string;
    sourceId: string;
    storagePath: string;
    contentExtractionStatus?: ContentExtractionStatus;
    contentExtractionError?: string | null;
    contentExtractionCompletedAt?: string | null;
}

export const documentApi = {
    getAll: async (userId: string): Promise<DocumentDto[]> => {
        const response = await client.get<DocumentDto[]>(`/users/${userId}/documents`);
        return response.data;
    },

    upload: async (userId: string, file: File): Promise<DocumentDto> => {
        const formData = new FormData();
        formData.append('file', file);

        const response = await client.post<DocumentDto>(`/users/${userId}/documents`, formData, {
            headers: {
                'Content-Type': undefined
            }
        });
        return response.data;
    },

    delete: async (userId: string, documentId: string): Promise<void> => {
        await client.delete(`/users/${userId}/documents/${documentId}`);
    },

    getExtractionStatus: async (userId: string, sourceId: string): Promise<ContentExtractionStatusDto> => {
        const response = await client.get<ContentExtractionStatusDto>(`/users/${userId}/sources/${sourceId}/extraction-status`);
        return response.data;
    }
};
