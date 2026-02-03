import { useState, useEffect, useCallback } from 'react';
import { documentApi, type DocumentDto } from '@/api/documentApi';
import { activityApi } from '@/api/activityApi';
import { useToast } from '@/hooks/use-toast';
import { useNavigate } from 'react-router';
import { SourceTypes } from '@/models/Enums';

export const useDocuments = (userId: string | undefined) => {
    const [documents, setDocuments] = useState<DocumentDto[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const { toast } = useToast();
    const navigate = useNavigate();

    const loadDocuments = useCallback(async () => {
        if (!userId) {
            setIsLoading(false);
            return;
        }
        setIsLoading(true);
        try {
            const data = await documentApi.getAll(userId);
            setDocuments(data);
        } catch (error) {
            console.error("Failed to load documents", error);
            toast({ variant: "destructive", title: "Error", description: "Failed to load documents." });
        } finally {
            setIsLoading(false);
        }
    }, [userId]);

    useEffect(() => {
        loadDocuments();
    }, [loadDocuments]);

    const uploadDocument = async (file: File) => {
        if (!userId) return;
        try {
            await documentApi.upload(userId, file);
            toast({ title: "Success", description: "Document uploaded successfully." });
            await loadDocuments();
        } catch (error) {
            console.error("Failed to upload", error);
            toast({ variant: "destructive", title: "Error", description: "Failed to upload document." });
            throw error;
        }
    };

    const deleteDocument = async (documentId: string) => {
        if (!userId) return;
        try {
            await documentApi.delete(userId, documentId);
            toast({ title: "Deleted", description: "Document deleted successfully." });
            await loadDocuments();
        } catch (error) {
            console.error("Failed to delete", error);
            toast({ variant: "destructive", title: "Error", description: "Failed to delete document." });
            throw error;
        }
    };

    const readDocument = async (doc: DocumentDto) => {
        if (!userId) return;
        try {
            // SourceType.Document = 2
            const activity = await activityApi.read(userId, {
                title: doc.fileName, // Or displayName if available
                sourceId: doc.sourceId,
                sourceType: SourceTypes.Document,
                exploreDurationSeconds: 0
            });

            if (activity && activity.id) {
                navigate(`/derot?activityId=${activity.id}&mode=read`);
            }
        } catch (error) {
            console.error("Failed to start reading document", error);
            toast({ variant: "destructive", title: "Error", description: "Impossible d'ouvrir le document." });
        }
    };

    return {
        documents,
        isLoading,
        uploadDocument,
        deleteDocument,
        readDocument,
        refresh: loadDocuments
    };
};
