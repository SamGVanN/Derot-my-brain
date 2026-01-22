import { useTranslation } from 'react-i18next';
import type { UserActivity } from '../models/UserActivity';
import type { TrackedTopicDto } from '../models/UserStatistics';
import { ActivityTimelineItem } from './ActivityTimelineItem';

import { parseDate, isValidDate } from '@/lib/dateUtils';

interface HistoryTimelineProps {
    activities: UserActivity[];
    trackedTopics: TrackedTopicDto[];
    onTrack: (topic: string, url: string) => void;
    onUntrack: (topic: string) => void;
}

export const HistoryTimeline: React.FC<HistoryTimelineProps> = ({
    activities,
    trackedTopics,
    onTrack,
    onUntrack
}) => {
    const { t } = useTranslation();

    const groupedActivities = activities.reduce((groups, activity) => {
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

    // Get sorted dates
    const dates = Object.keys(groupedActivities).sort((a, b) => {
        if (a === 'Unknown Date') return 1;
        if (b === 'Unknown Date') return -1;
        return parseDate(b).getTime() - parseDate(a).getTime();
    });

    const getBestScore = (topic: string) => {
        const tracked = trackedTopics.find(t => t.topic === topic);
        if (tracked?.bestScore != null) {
            return {
                score: tracked.bestScore,
                total: tracked.totalQuestions
            };
        }
        return undefined;
    };

    const isTracked = (topic: string) => {
        return trackedTopics.some(t => t.topic === topic);
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
                            {groupedActivities[date].map((activity, index) => (
                                <ActivityTimelineItem
                                    key={activity.id}
                                    activity={activity}
                                    isTracked={isTracked(activity.topic)}
                                    bestScore={getBestScore(activity.topic)}
                                    onTrack={() => onTrack(activity.topic, activity.wikipediaUrl)}
                                    onUntrack={() => onUntrack(activity.topic)}
                                    isLast={index === groupedActivities[date].length - 1}
                                />
                            ))}
                        </div>
                    </div>
                ))
            }
        </div >
    );
};
