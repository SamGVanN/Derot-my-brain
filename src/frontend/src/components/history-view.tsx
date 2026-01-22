import React from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { useHistory } from '../hooks/useHistory';
import type { User } from '../models/User';
import type { UserActivity } from '../models/UserActivity';
import { Loader2, AlertCircle, ExternalLink, Notebook, NotebookPen } from 'lucide-react';

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
            new Date(b.sessionDate).getTime() - new Date(a.sessionDate).getTime()
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
                            className="p-4 rounded-lg bg-muted/40 border border-border/50 hover:border-border transition-all group relative overflow-hidden"
                        >
                            <div className="flex justify-between items-start mb-2">
                                <span className={
                                    `px-2 py-0.5 rounded-full text-xs font-semibold border flex items-center gap-1.5 ` +
                                    (activity.type === 'Quiz'
                                        ? 'bg-primary/10 text-primary border-primary/20'
                                        : 'bg-secondary/10 text-foreground border-secondary/20')
                                }>
                                    {activity.type === 'Quiz' ? (
                                        <NotebookPen className="w-3.5 h-3.5" />
                                    ) : (
                                        <Notebook className="w-3.5 h-3.5" />
                                    )}
                                    {activity.type}
                                </span>
                                <div className="flex flex-col items-end">
                                    <span className="text-xs text-muted-foreground group-hover:text-foreground transition-colors">
                                        {new Date(activity.sessionDate).toLocaleString()}
                                    </span>
                                    {activity.isTracked && (
                                        <span className="text-[10px] uppercase tracking-wider text-muted-foreground/60 mt-0.5">
                                            Tracked
                                        </span>
                                    )}
                                </div>
                            </div>

                            <a
                                href={activity.wikipediaUrl}
                                target="_blank"
                                rel="noopener noreferrer"
                                className="text-foreground font-medium hover:text-primary transition-colors flex items-center gap-1 group/link"
                            >
                                {activity.topic}
                                <ExternalLink className="h-3 w-3 opacity-0 group-hover/link:opacity-100 transition-opacity" />
                            </a>

                            {activity.score !== undefined && activity.score !== null && (
                                <div className="mt-3">
                                    <div className="flex justify-between text-xs mb-1">
                                        <span className="text-muted-foreground">{t('history.score', 'Score')}</span>
                                        <span className="font-bold text-foreground">
                                            {activity.score}%
                                            {activity.totalQuestions && <span className="text-muted-foreground font-normal"> ({activity.totalQuestions} qs)</span>}
                                        </span>
                                    </div>
                                    <div className="h-1.5 w-full bg-muted rounded-full overflow-hidden">
                                        <div
                                            className="h-full bg-gradient-to-r from-primary to-violet-500"
                                            style={{ width: `${activity.score}%` }}
                                        />
                                    </div>
                                </div>
                            )}

                            {activity.llmModelName && (
                                <div className="mt-2 text-[10px] text-muted-foreground/50 text-right">
                                    {activity.llmModelName} {activity.llmVersion}
                                </div>
                            )}
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
};
