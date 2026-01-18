import React, { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { UserService } from '../services/UserService';
import type { UserActivity } from '../models/UserActivity';
import type { User } from '../models/User';

interface HistoryViewProps {
    user: User;
}

export const HistoryView: React.FC<HistoryViewProps> = ({ user }) => {
    const { t } = useTranslation();
    const [history, setHistory] = useState<UserActivity[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchHistory = async () => {
            try {
                setLoading(true);
                const data = await UserService.getHistory(user.id);
                setHistory(data.sort((a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime()));
                setError(null);
            } catch (err) {
                console.error('Failed to fetch history:', err);
                setError(t('history.error'));
            } finally {
                setLoading(false);
            }
        };

        fetchHistory();
    }, [user.id, t]);


    if (loading) return <div className="p-4">{t('history.loading')}</div>;
    if (error) return <div className="p-4 text-red-500">{error}</div>;

    return (
        <div className="p-6 bg-card/50 backdrop-blur-md rounded-xl border border-border shadow-2xl">
            <h2 className="text-2xl font-bold mb-6 text-foreground">
                {t('history.title')}
            </h2>

            {history.length === 0 ? (
                <p className="text-muted-foreground italic text-center py-8">{t('history.empty')}</p>
            ) : (
                <div className="space-y-4">
                    {history.map((activity) => (
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
