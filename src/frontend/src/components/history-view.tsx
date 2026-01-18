import React from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { useHistory } from '../hooks/useHistory';
import type { User } from '../models/User';
import type { UserActivity } from '../models/UserActivity';
import { Loader2, AlertCircle } from 'lucide-react';

interface HistoryViewProps {
    user: User;
}

export const HistoryView: React.FC<HistoryViewProps> = ({ user }) => {
    const { t } = useTranslation();
    const { fetchHistory } = useHistory();

    const { data: history, isLoading, error } = useQuery({
        queryKey: ['history', user.id],
        queryFn: fetchHistory,
        select: (data) => [...data].sort((a, b) =>
            new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime()
        ),
        enabled: !!user.id
    });

    if (isLoading) {
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

            {!history || history.length === 0 ? (
                <p className="text-muted-foreground italic text-center py-8">{t('history.empty')}</p>
            ) : (
                <div className="space-y-4">
                    {history.map((activity: UserActivity) => (
                        <div
                            key={activity.id}
                            className="p-4 rounded-lg bg-muted/40 border border-border/50 hover:border-border transition-all group"
                        >
                            <div className="flex justify-between items-start mb-1">
                                <span className="px-2 py-0.5 rounded-full text-xs font-semibold bg-primary/10 text-primary border border-primary/20">
                                    {activity.activityType}
                                </span>
                                <span className="text-xs text-muted-foreground group-hover:text-foreground transition-colors">
                                    {new Date(activity.timestamp).toLocaleString()}
                                </span>
                            </div>
                            <p className="text-foreground font-medium">
                                {activity.description}
                            </p>
                            {activity.score !== undefined && activity.score !== null && (
                                <div className="mt-2 flex items-center gap-2">
                                    <div className="h-1.5 flex-1 bg-muted rounded-full overflow-hidden">
                                        <div
                                            className="h-full bg-gradient-to-r from-green-400 to-emerald-500"
                                            style={{ width: `${activity.score}%` }}
                                        />
                                    </div>
                                    <span className="text-sm font-bold text-green-500">{activity.score}%</span>
                                </div>
                            )}
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
};
