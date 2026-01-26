import { client } from './client';

export interface DocumentDto {
    id: string;
    userId: string;
    fileName: string;
    fileType: string;
    fileSize: number;
    uploadDate: string;
    displayTitle: string;
    sourceHash: string;
    storagePath: string;
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
    }
};
