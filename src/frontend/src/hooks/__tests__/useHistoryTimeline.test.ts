import { renderHook } from '@testing-library/react';
import { useHistoryTimeline } from '../useHistoryTimeline';
import { describe, it, expect } from 'vitest';
import type { UserActivity } from '../../models/UserActivity';
import type { TrackedTopicDto } from '../../models/UserStatistics';

describe('useHistoryTimeline', () => {
    const mockActivities: UserActivity[] = [
        {
            id: '1',
            userId: 'u1',
            topic: 'Topic A',
            wikipediaUrl: 'url',
            type: 'Read',
            sessionDate: '2023-10-10T10:00:00',
            isTracked: true
        },
        {
            id: '2',
            userId: 'u1',
            topic: 'Topic B',
            wikipediaUrl: 'url',
            type: 'Quiz',
            sessionDate: '2023-10-11T10:00:00',
            isTracked: false
        },
        {
            id: '3',
            userId: 'u1',
            topic: 'Topic A',
            wikipediaUrl: 'url',
            type: 'Quiz',
            sessionDate: '2023-10-10T11:00:00', // Same day as 1
            isTracked: true
        }
    ];

    const mockTrackedTopics: TrackedTopicDto[] = [
        {
            topic: 'Topic A',
            wikipediaUrl: 'url',
            addedAt: '2023-01-01',
            totalSessions: 1,
            bestScore: 90,
            bestScoreDate: '2023-01-01',
            totalQuestions: 100
        }
    ];

    it('groups activities by date correctly', () => {
        const { result } = renderHook(() => useHistoryTimeline({
            activities: mockActivities,
            trackedTopics: mockTrackedTopics
        }));

        const { groupedActivities } = result.current;

        // 10/10/2023 should have 2 activities
        const date1 = new Date('2023-10-10').toLocaleDateString();
        expect(groupedActivities[date1]).toHaveLength(2);

        // 10/11/2023 should have 1 activity
        const date2 = new Date('2023-10-11').toLocaleDateString();
        expect(groupedActivities[date2]).toHaveLength(1);
    });

    it('sorts dates descending', () => {
        const { result } = renderHook(() => useHistoryTimeline({
            activities: mockActivities,
            trackedTopics: mockTrackedTopics
        }));

        const { dates } = result.current;
        const date1 = new Date('2023-10-10').toLocaleDateString();
        const date2 = new Date('2023-10-11').toLocaleDateString();

        expect(dates[0]).toBe(date2); // Newer first
        expect(dates[1]).toBe(date1);
    });

    it('retrieves best score correctly', () => {
        const { result } = renderHook(() => useHistoryTimeline({
            activities: mockActivities,
            trackedTopics: mockTrackedTopics
        }));

        const bestScore = result.current.getBestScore('Topic A');
        expect(bestScore).toEqual({ score: 90, total: 100 });

        const noScore = result.current.getBestScore('Topic B');
        expect(noScore).toBeUndefined();
    });

    it('checks isTracked correctly', () => {
        const { result } = renderHook(() => useHistoryTimeline({
            activities: mockActivities,
            trackedTopics: mockTrackedTopics
        }));

        expect(result.current.isTracked('Topic A')).toBe(true);
        expect(result.current.isTracked('Topic B')).toBe(false);
    });
});
