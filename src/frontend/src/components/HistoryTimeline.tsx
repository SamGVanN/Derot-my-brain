import React from 'react';
import { useTranslation } from 'react-i18next';
import type { UserActivity } from '../models/UserActivity';
import type { UserFocus, TrackTopicRequest } from '../models/UserFocus';
import { ActivityTimelineItem } from './ActivityTimelineItem';
import { parseDate, formatDateKey } from '@/lib/dateUtils';

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

    // Nested grouping: DateKey (YYYY-MM-DD) -> SessionId -> Activity[]
    const groupedByDateAndSession = activities.reduce((groups, activity) => {
        const dateObj = parseDate(activity.sessionDateStart);
        const dateKey = formatDateKey(dateObj);

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

    // Get sorted date keys (descending)
    const dateKeys = Object.keys(groupedByDateAndSession).sort((a, b) => {
        if (a === 'unknown') return 1;
        if (b === 'unknown') return -1;
        return b.localeCompare(a);
    });

    const isTopicFocused = (sourceId: string) => {
        return userFocuses.some(t => t.sourceId === sourceId);
    };

    return (
        <div className="space-y-12" >
            {
                dateKeys.map((dateKey) => (
                    <div key={dateKey}>
                        <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wider mb-6 pl-16">
                            {dateKey === 'unknown'
                                ? t('history.unknownDate', 'Unknown Date')
                                : parseDate(dateKey).toLocaleDateString(undefined, { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })
                            }
                        </h3>

                        <div className="space-y-10">
                            {Object.entries(groupedByDateAndSession[dateKey]).map(([sessionId, sessionActivities]) => {
                                // Sort session activities by start time (newest first)
                                const sortedActivities = [...sessionActivities].sort((a, b) =>
                                    parseDate(b.sessionDateStart).getTime() - parseDate(a.sessionDateStart).getTime()
                                );

                                return (
                                    <div key={sessionId} className="relative group/session">
                                        {/* Optional: Session Header or visual grouping indicator could go here */}
                                        <div className="space-y-0 relative">
                                            {sortedActivities.map((activity, index) => (
                                                <ActivityTimelineItem
                                                    key={activity.id}
                                                    activity={activity}
                                                    isTracked={isTopicFocused(activity.sourceId)}
                                                    onTrack={() => onTrack({
                                                        sourceId: activity.sourceId,
                                                        sourceType: activity.sourceType,
                                                        displayTitle: activity.title
                                                    })}
                                                    onUntrack={() => onUntrack(activity.sourceId)}
                                                    isLast={index === sortedActivities.length - 1}
                                                />
                                            ))}
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
