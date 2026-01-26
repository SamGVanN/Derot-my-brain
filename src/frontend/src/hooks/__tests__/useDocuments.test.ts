import { renderHook, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useDocuments } from '../useDocuments';
import { documentApi } from '../../api/documentApi';

// Mock the API
vi.mock('../../api/documentApi', () => ({
    documentApi: {
        getAll: vi.fn(),
        upload: vi.fn(),
        delete: vi.fn()
    }
}));

// Mock the toast hook
vi.mock('../use-toast', () => ({
    useToast: () => ({
        toast: vi.fn()
    })
}));

describe('useDocuments', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('should set isLoading to false if userId is undefined', async () => {
        const { result } = renderHook(() => useDocuments(undefined));

        // This should pass if handled correctly, but currently will time out or fail
        await waitFor(() => expect(result.current.isLoading).toBe(false));
        expect(result.current.documents).toEqual([]);
    });

    it('should load documents when userId is provided', async () => {
        const mockDocs = [{
            id: '1',
            userId: 'user-1',
            fileName: 'test.pdf',
            fileType: '.pdf',
            fileSize: 1024,
            uploadDate: '2023-01-01',
            displayTitle: 'Test',
            sourceHash: 'hash',
            storagePath: 'path'
        }];
        (documentApi.getAll as any).mockResolvedValue(mockDocs);

        const { result } = renderHook(() => useDocuments('user-1'));

        await waitFor(() => expect(result.current.isLoading).toBe(false));
        expect(result.current.documents).toEqual(mockDocs);
    });
});
