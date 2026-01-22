import { useMemo, useCallback } from 'react';
import type { UserActivity } from '../models/UserActivity';
import type { TrackedTopicDto } from '../models/UserStatistics';
import { parseDate, isValidDate } from '@/lib/dateUtils';

interface UseHistoryTimelineProps {
    activities: UserActivity[];
    trackedTopics: TrackedTopicDto[];
}

export function useHistoryTimeline({ activities, trackedTopics }: UseHistoryTimelineProps) {
    const groupedActivities = useMemo(() => {
        return activities.reduce((groups, activity) => {
            const dateObj = parseDate(activity.sessionDate);
            const date = isValidDate(dateObj)
                ? dateObj.toLocaleDateString()
                : 'Unknown Date';

            if (!groups[date]) {
                groups[date] = [];
            }
            groups[date].push(activity);
            return groups;
        }, {} as Record<string, UserActivity[]>);
    }, [activities]);

    const dates = useMemo(() => {
        return Object.keys(groupedActivities).sort((a, b) => {
            if (a === 'Unknown Date') return 1;
            if (b === 'Unknown Date') return -1;
            return parseDate(b).getTime() - parseDate(a).getTime();
        });
    }, [groupedActivities]);

    const getBestScore = useCallback((topic: string) => {
        const tracked = trackedTopics.find(t => t.topic === topic);
        if (tracked?.bestScore != null) {
            return {
                score: tracked.bestScore,
                total: tracked.totalQuestions
            };
        }
        return undefined;
    }, [trackedTopics]);

    const isTracked = useCallback((topic: string) => {
        return trackedTopics.some(t => t.topic === topic);
    }, [trackedTopics]);

    return {
        groupedActivities,
        dates,
        getBestScore,
        isTracked
    };
}
