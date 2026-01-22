import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { useActivities } from '../useActivities';
import { useTrackedTopics } from '../useTrackedTopics';
import { useUserStatistics } from '../useUserStatistics';
import { activityApi } from '../../api/activityApi';

// Mock dependencies
vi.mock('../../api/activityApi');

const mockUser = { id: 'user1', name: 'User' };
vi.mock('../../stores/useAuthStore', () => ({
    useAuthStore: () => ({ user: mockUser }),
}));

describe('Custom Hooks', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    describe('useActivities', () => {
        it('refresh calls activityApi.getActivities', async () => {
            const mockData = [{ id: '1' }];
            (activityApi.getActivities as any).mockResolvedValue(mockData);

            const { result } = renderHook(() => useActivities());

            await act(async () => {
                await result.current.refresh();
            });

            expect(activityApi.getActivities).toHaveBeenCalledWith('user1', undefined);
            expect(result.current.activities).toEqual(mockData);
        });

        it('createActivity calls activityApi.createActivity', async () => {
            const newActivity = { type: 'read' };
            const created = { id: '2', ...newActivity };
            (activityApi.createActivity as any).mockResolvedValue(created);

            const { result } = renderHook(() => useActivities());

            await act(async () => {
                await result.current.createActivity(newActivity);
            });

            expect(activityApi.createActivity).toHaveBeenCalledWith('user1', newActivity);
            expect(result.current.activities).toContainEqual(created);
        });
    });

    describe('useTrackedTopics', () => {
        it('refresh calls activityApi.getTrackedTopics', async () => {
            const mockData = [{ topic: 'test' }];
            (activityApi.getTrackedTopics as any).mockResolvedValue(mockData);

            const { result } = renderHook(() => useTrackedTopics());

            await act(async () => {
                await result.current.refresh();
            });

            expect(activityApi.getTrackedTopics).toHaveBeenCalledWith('user1');
            expect(result.current.trackedTopics).toEqual(mockData);
        });

        it('trackTopic calls activityApi.trackTopic', async () => {
            const newTopic = { topic: 'new', wikipediaUrl: 'url' };
            (activityApi.trackTopic as any).mockResolvedValue(newTopic);

            const { result } = renderHook(() => useTrackedTopics());

            await act(async () => {
                await result.current.trackTopic('new', 'url');
            });

            expect(activityApi.trackTopic).toHaveBeenCalledWith('user1', 'new', 'url');
            expect(result.current.trackedTopics).toContainEqual(newTopic);
        });
    });

    describe('useUserStatistics', () => {
        it('refreshStatistics calls activityApi.getStatistics', async () => {
            const mockStats = { totalActivities: 5 };
            (activityApi.getStatistics as any).mockResolvedValue(mockStats);

            const { result } = renderHook(() => useUserStatistics());

            await act(async () => {
                await result.current.refreshStatistics();
            });

            expect(activityApi.getStatistics).toHaveBeenCalledWith('user1');
            expect(result.current.statistics).toEqual(mockStats);
        });
    });
});
