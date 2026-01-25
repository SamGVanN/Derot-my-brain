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

    const getBestScore = useCallback((sourceHash: string) => {
        const focus = userFocuses.find(t => t.sourceHash === sourceHash);
        if (focus?.bestScore != null) {
            return {
                score: focus.bestScore,
                lastScore: focus.lastScore
            };
        }
        return undefined;
    }, [userFocuses]);

    const isTracked = useCallback((sourceHash: string) => {
        return userFocuses.some(t => t.sourceHash === sourceHash);
    }, [userFocuses]);

    return {
        groupedActivities,
        dates,
        getBestScore,
        isTracked
    };
}
