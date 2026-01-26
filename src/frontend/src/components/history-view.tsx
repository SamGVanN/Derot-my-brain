import React, { useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useActivities } from '../hooks/useActivities';
import { useUserFocus } from '../hooks/useUserFocus';
import type { User } from '../models/User';
import { Loader2, AlertCircle } from 'lucide-react';
import { HistoryTimeline } from './HistoryTimeline';

interface HistoryViewProps {
    user: User;
}

export const HistoryView: React.FC<HistoryViewProps> = ({ user }) => {
    const { t } = useTranslation();
    const { activities, loading: loadingActivities, error: activitiesError, refresh: refreshActivities } = useActivities();
    const { userFocuses, loading: loadingTracks, refresh: refreshTracks, trackSource, untrackSource } = useUserFocus();

    useEffect(() => {
        if (user.id) {
            refreshActivities();
            refreshTracks();
        }
    }, [user.id, refreshActivities, refreshTracks]);

    const isLoading = loadingActivities || loadingTracks;
    const error = activitiesError; // For now mainly focus on activities error

    if (isLoading && activities.length === 0) {
        return (
            <div className="flex justify-center items-center p-8 bg-card/50 rounded-xl border border-border">
                <Loader2 className="h-8 w-8 animate-spin text-primary" />
                <span className="ml-2 text-muted-foreground">{t('history.loading')}</span>
            </div>
        );
    }

    if (error) {
        return (
            <div className="flex items-center gap-2 p-4 text-destructive bg-destructive/10 rounded-xl border border-destructive/20">
                <AlertCircle className="h-5 w-5" />
                <span>{t('history.error')}</span>
            </div>
        );
    }

    return (
        <div className="p-6 bg-card/50 backdrop-blur-md rounded-xl border border-border shadow-2xl">
            <h2 className="text-2xl font-bold mb-6 text-foreground">
                {t('history.title')}
            </h2>

            {!activities || activities.length === 0 ? (
                <p className="text-muted-foreground italic text-center py-8">{t('history.empty')}</p>
            ) : (
                <HistoryTimeline
                    activities={activities}
                    userFocuses={userFocuses}
                    onTrack={trackSource}
                    onUntrack={untrackSource}
                />
            )}
        </div>
    );
};
