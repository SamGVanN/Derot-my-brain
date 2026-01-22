import { describe, it, expect, vi, beforeEach } from 'vitest';
import { activityApi } from '../activityApi';

// Mock the client
const mocks = vi.hoisted(() => ({
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    patch: vi.fn(),
    delete: vi.fn(),
}));

vi.mock('../client', () => ({
    client: {
        get: mocks.get,
        post: mocks.post,
        put: mocks.put,
        patch: mocks.patch,
        delete: mocks.delete,
    },
}));

describe('activityApi', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    // --- Activity Tests ---

    it('getActivities calls GET /users/:id/activities', async () => {
        const mockActivities = [{ id: '1', type: 'read' }];
        mocks.get.mockResolvedValueOnce({ data: mockActivities });

        const result = await activityApi.getActivities('user1');

        expect(mocks.get).toHaveBeenCalledWith('/users/user1/activities', { params: undefined });
        expect(result).toEqual(mockActivities);
    });

    it('getActivities calls GET /users/:id/activities with topic filter', async () => {
        const mockActivities = [{ id: '1', type: 'read' }];
        mocks.get.mockResolvedValueOnce({ data: mockActivities });

        const result = await activityApi.getActivities('user1', 'science');

        expect(mocks.get).toHaveBeenCalledWith('/users/user1/activities', { params: { topic: 'science' } });
        expect(result).toEqual(mockActivities);
    });

    it('createActivity calls POST /users/:id/activities', async () => {
        const newActivity = { type: 'read', topic: 'test' };
        const createdActivity = { id: '1', ...newActivity };
        mocks.post.mockResolvedValueOnce({ data: createdActivity });

        const result = await activityApi.createActivity('user1', newActivity);

        expect(mocks.post).toHaveBeenCalledWith('/users/user1/activities', newActivity);
        expect(result).toEqual(createdActivity);
    });

    // --- Statistics Tests ---

    it('getStatistics calls GET /users/:id/statistics', async () => {
        const mockStats = { totalActivities: 10 };
        mocks.get.mockResolvedValueOnce({ data: mockStats });

        const result = await activityApi.getStatistics('user1');

        expect(mocks.get).toHaveBeenCalledWith('/users/user1/statistics');
        expect(result).toEqual(mockStats);
    });

    it('getActivityCalendar calls GET /users/:id/statistics/activity-calendar', async () => {
        const mockCalendar = [{ date: '2023-01-01', count: 1 }];
        mocks.get.mockResolvedValueOnce({ data: mockCalendar });

        const result = await activityApi.getActivityCalendar('user1');

        expect(mocks.get).toHaveBeenCalledWith('/users/user1/statistics/activity-calendar', { params: { days: 365 } });
        expect(result).toEqual(mockCalendar);
    });

    // --- Tracked Topics Tests ---

    it('getTrackedTopics calls GET /users/:id/tracked-topics', async () => {
        const mockTopics = [{ topic: 'test' }];
        mocks.get.mockResolvedValueOnce({ data: mockTopics });

        const result = await activityApi.getTrackedTopics('user1');

        expect(mocks.get).toHaveBeenCalledWith('/users/user1/tracked-topics');
        expect(result).toEqual(mockTopics);
    });

    it('trackTopic calls POST /users/:id/tracked-topics', async () => {
        const mockTopic = { topic: 'test', wikipediaUrl: 'url' };
        mocks.post.mockResolvedValueOnce({ data: mockTopic });

        const result = await activityApi.trackTopic('user1', 'test', 'url');

        expect(mocks.post).toHaveBeenCalledWith('/users/user1/tracked-topics', { topic: 'test', wikipediaUrl: 'url' });
        expect(result).toEqual(mockTopic);
    });

    it('untrackTopic calls DELETE /users/:id/tracked-topics/:topic', async () => {
        mocks.delete.mockResolvedValueOnce({});

        await activityApi.untrackTopic('user1', 'test');

        expect(mocks.delete).toHaveBeenCalledWith('/users/user1/tracked-topics/test');
    });
});
