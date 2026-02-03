import { useState, useEffect, useRef } from 'react';
import { ContentExtractionStatus } from '@/models/ContentExtractionStatus';
import { documentApi } from '@/api/documentApi';
import { useAuth } from './useAuth';

interface UseDocumentExtractionStatusResult {
    status: ContentExtractionStatus;
    error: string | null;
    isExtracting: boolean;
}

/**
 * Hook to poll document extraction status while processing
 * @param sourceId - The source ID to monitor
 * @param initialStatus - Initial status from document DTO
 * @returns Current extraction status and loading state
 */
export const useDocumentExtractionStatus = (
    sourceId: string | null,
    initialStatus?: ContentExtractionStatus
): UseDocumentExtractionStatusResult => {
    const { user } = useAuth();
    const [status, setStatus] = useState<ContentExtractionStatus>(
        initialStatus ?? ContentExtractionStatus.Completed
    );
    const [error, setError] = useState<string | null>(null);
    const intervalRef = useRef<NodeJS.Timeout | null>(null);

    useEffect(() => {
        // Only poll if we have a sourceId, userId, and status is Pending or Processing
        if (!sourceId || !user?.id) {
            return;
        }

        const shouldPoll =
            status === ContentExtractionStatus.Pending ||
            status === ContentExtractionStatus.Processing;

        if (!shouldPoll) {
            // Clear any existing interval
            if (intervalRef.current) {
                clearInterval(intervalRef.current);
                intervalRef.current = null;
            }
            return;
        }

        // Start polling
        const pollStatus = async () => {
            try {
                const result = await documentApi.getExtractionStatus(user.id, sourceId);
                setStatus(result.status);
                setError(result.error);

                // Stop polling if completed or failed
                if (result.status === ContentExtractionStatus.Completed ||
                    result.status === ContentExtractionStatus.Failed) {
                    if (intervalRef.current) {
                        clearInterval(intervalRef.current);
                        intervalRef.current = null;
                    }
                }
            } catch (err) {
                console.error('Failed to fetch extraction status:', err);
                // Don't stop polling on error, might be temporary
            }
        };

        // Poll immediately
        pollStatus();

        // Then poll every 2 seconds
        intervalRef.current = setInterval(pollStatus, 2000);

        // Cleanup on unmount or when dependencies change
        return () => {
            if (intervalRef.current) {
                clearInterval(intervalRef.current);
                intervalRef.current = null;
            }
        };
    }, [sourceId, user?.id, status]);

    return {
        status,
        error,
        isExtracting: status === ContentExtractionStatus.Processing || status === ContentExtractionStatus.Pending
    };
};
