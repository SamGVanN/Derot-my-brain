import React from 'react';
import { useTranslation } from 'react-i18next';
import type { UserActivity } from '../models/UserActivity';
import type { UserFocus, TrackTopicRequest } from '../models/UserFocus';
import { ActivityTimelineItem } from './ActivityTimelineItem';
import { parseDate, isValidDate } from '@/lib/dateUtils';

interface HistoryTimelineProps {
    activities: UserActivity[];
    userFocuses: UserFocus[];
    onTrack: (request: TrackTopicRequest) => void;
    onUntrack: (sourceHash: string) => void;
}

export const HistoryTimeline: React.FC<HistoryTimelineProps> = ({
    activities,
    userFocuses,
    onTrack,
    onUntrack
}) => {
    const { t } = useTranslation();

    const groupedActivities = activities.reduce((groups, activity) => {
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

    // Get sorted dates
    const dates = Object.keys(groupedActivities).sort((a, b) => {
        if (a === 'Unknown Date') return 1;
        if (b === 'Unknown Date') return -1;
        return parseDate(b).getTime() - parseDate(a).getTime();
    });

    const getBestScore = (sourceHash: string) => {
        const focus = userFocuses.find(t => t.sourceHash === sourceHash);
        if (focus?.bestScore != null) {
            return {
                score: focus.bestScore,
                lastScore: focus.lastScore
            };
        }
        return undefined;
    };

    const isTopicFocused = (sourceHash: string) => {
        return userFocuses.some(t => t.sourceHash === sourceHash);
    };

    return (
        <div className="space-y-8" >
            {
                dates.map((date) => (
                    <div key={date}>
                        <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wider mb-4 pl-16">
                            {date === 'Unknown Date'
                                ? t('history.unknownDate', 'Unknown Date')
                                : parseDate(date).toLocaleDateString(undefined, { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })
                            }
                        </h3>
                        <div className="space-y-0">
                            {groupedActivities[date].map((activity, index) => {
                                const best = getBestScore(activity.sourceHash);
                                const isCurrentBest = activity.type === 'Quiz' &&
                                    best?.score != null &&
                                    activity.scorePercentage != null &&
                                    Math.abs(activity.scorePercentage - best.score) < 0.01;

                                return (
                                    <ActivityTimelineItem
                                        key={activity.id}
                                        activity={activity}
                                        isTracked={isTopicFocused(activity.sourceHash)}
                                        bestScore={best}
                                        isCurrentBest={isCurrentBest}
                                        isBaseline={activity.isBaseline}
                                        onTrack={() => onTrack({
                                            sourceId: activity.sourceId,
                                            sourceType: activity.sourceType,
                                            displayTitle: activity.title
                                        })}
                                        onUntrack={() => onUntrack(activity.sourceHash)}
                                        isLast={index === groupedActivities[date].length - 1}
                                    />
                                );
                            })}
                        </div>
                    </div>
                ))
            }
        </div >
    );
};
