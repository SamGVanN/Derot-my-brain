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
    onUntrack: (sourceId: string) => void;
}

export const HistoryTimeline: React.FC<HistoryTimelineProps> = ({
    activities,
    userFocuses,
    onTrack,
    onUntrack
}) => {
    const { t } = useTranslation();

    // Nested grouping: Date -> SessionId -> Activity[]
    const groupedByDateAndSession = activities.reduce((groups, activity) => {
        const dateObj = parseDate(activity.sessionDateStart);
        const dateKey = isValidDate(dateObj)
            ? dateObj.toLocaleDateString()
            : 'Unknown Date';

        if (!groups[dateKey]) {
            groups[dateKey] = {};
        }

        const sessionId = activity.userSessionId || 'no-session';
        if (!groups[dateKey][sessionId]) {
            groups[dateKey][sessionId] = [];
        }

        groups[dateKey][sessionId].push(activity);
        return groups;
    }, {} as Record<string, Record<string, UserActivity[]>>);

    // Get sorted dates
    const dates = Object.keys(groupedByDateAndSession).sort((a, b) => {
        if (a === 'Unknown Date') return 1;
        if (b === 'Unknown Date') return -1;
        return parseDate(b).getTime() - parseDate(a).getTime();
    });

    const getBestScore = (sourceId: string) => {
        const focus = userFocuses.find(t => t.sourceId === sourceId);
        if (focus?.bestScore != null) {
            return {
                score: focus.bestScore,
                lastScore: focus.lastScore
            };
        }
        return undefined;
    };

    const isTopicFocused = (sourceId: string) => {
        return userFocuses.some(t => t.sourceId === sourceId);
    };

    return (
        <div className="space-y-12" >
            {
                dates.map((date) => (
                    <div key={date}>
                        <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wider mb-6 pl-16">
                            {date === 'Unknown Date'
                                ? t('history.unknownDate', 'Unknown Date')
                                : parseDate(date).toLocaleDateString(undefined, { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })
                            }
                        </h3>

                        <div className="space-y-10">
                            {Object.entries(groupedByDateAndSession[date]).map(([sessionId, sessionActivities]) => {
                                // Sort session activities by start time
                                const sortedActivities = [...sessionActivities].sort((a, b) =>
                                    parseDate(a.sessionDateStart).getTime() - parseDate(b.sessionDateStart).getTime()
                                );

                                const firstActivity = sortedActivities[0];
                                const best = getBestScore(firstActivity.sourceId);

                                return (
                                    <div key={sessionId} className="relative group/session">
                                        {/* Optional: Session Header or visual grouping indicator could go here */}
                                        <div className="space-y-0 relative">
                                            {sortedActivities.map((activity, index) => {
                                                const isCurrentBest = activity.type === 'Quiz' &&
                                                    best?.score != null &&
                                                    activity.scorePercentage != null &&
                                                    Math.abs(activity.scorePercentage - best.score) < 0.01;

                                                return (
                                                    <ActivityTimelineItem
                                                        key={activity.id}
                                                        activity={activity}
                                                        isTracked={isTopicFocused(activity.sourceId)}
                                                        bestScore={best}
                                                        isCurrentBest={isCurrentBest}
                                                        isBaseline={activity.isBaseline}
                                                        onTrack={() => onTrack({
                                                            sourceId: activity.sourceId,
                                                            sourceType: activity.sourceType,
                                                            displayTitle: activity.title
                                                        })}
                                                        onUntrack={() => onUntrack(activity.sourceHash)}
                                                        isLast={index === sortedActivities.length - 1}
                                                    />
                                                );
                                            })}
                                        </div>
                                    </div>
                                );
                            })}
                        </div>
                    </div>
                ))
            }
        </div >
    );
};
