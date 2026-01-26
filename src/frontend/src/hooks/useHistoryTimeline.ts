import { useMemo, useCallback } from 'react';
import type { UserActivity } from '../models/UserActivity';
import type { UserFocus } from '../models/UserFocus';
import { parseDate, isValidDate } from '@/lib/dateUtils';

interface UseHistoryTimelineProps {
    activities: UserActivity[];
    userFocuses: UserFocus[];
}

export function useHistoryTimeline({ activities, userFocuses }: UseHistoryTimelineProps) {
    const groupedActivities = useMemo(() => {
        return activities.reduce((groups, activity) => {
            const dateObj = parseDate(activity.sessionDateStart);
            const date = isValidDate(dateObj)
                ? dateObj.toLocaleDateString()
                : 'Unknown Date';

            if (!groups[date]) {
                groups[date] = {};
            }

            const sessionId = activity.userSessionId || 'no-session';
            if (!groups[date][sessionId]) {
                groups[date][sessionId] = [];
            }

            groups[date][sessionId].push(activity);
            return groups;
        }, {} as Record<string, Record<string, UserActivity[]>>);
    }, [activities]);

    const dates = useMemo(() => {
        return Object.keys(groupedActivities).sort((a, b) => {
            if (a === 'Unknown Date') return 1;
            if (b === 'Unknown Date') return -1;
            return parseDate(b).getTime() - parseDate(a).getTime();
        });
    }, [groupedActivities]);

    const getBestScore = useCallback((sourceId: string) => {
        const focus = userFocuses.find(t => t.sourceId === sourceId);
        if (focus?.bestScore != null) {
            return {
                score: focus.bestScore,
                lastScore: focus.lastScore
            };
        }
        return undefined;
    }, [userFocuses]);

    const isTracked = useCallback((sourceId: string) => {
        return userFocuses.some(t => t.sourceId === sourceId);
    }, [userFocuses]);

    return {
        groupedActivities,
        dates,
        getBestScore,
        isTracked
    };
}
