import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import {
    ExternalLink,
    Trophy,
    History,
    Clock,
    Calendar,
    ChevronDown,
    ChevronUp,
    Pin,
    Archive,
    Loader2,
    BookmarkCheck,
    BookOpen,
    GraduationCap
} from 'lucide-react';
import { useNavigate } from 'react-router';
import type { UserFocus } from '../models/UserFocus';
import type { UserActivity } from '../models/UserActivity';
import { userFocusApi } from '../api/userFocusApi';
import { activityApi } from '../api/activityApi';
import { cn } from '@/lib/utils';
import { Button } from './ui/button';
import { Card, CardContent, CardHeader, CardTitle } from './ui/card';
import { ActivityTimelineItem } from './ActivityTimelineItem';
import { parseDate, isValidDate } from '@/lib/dateUtils';

interface FocusAreaCardProps {
    focus: UserFocus;
    onTogglePin?: (focus: UserFocus) => void;
    onToggleArchive?: (focus: UserFocus) => void;
    onUntrack?: (focus: UserFocus) => void;
}

export const FocusAreaCard: React.FC<FocusAreaCardProps> = ({
    focus,
    onTogglePin,
    onToggleArchive,
    onUntrack
}) => {
    const { t, i18n } = useTranslation();
    const [isExpanded, setIsExpanded] = useState(false);
    const [activities, setActivities] = useState<UserActivity[]>([]);
    const [isLoading, setIsLoading] = useState(false);
    const [isActionLoading, setIsActionLoading] = useState<string | null>(null);
    const [hasLoadedEntries, setHasLoadedEntries] = useState(false);
    const navigate = useNavigate();

    const toggleExpand = async () => {
        const newExpandedState = !isExpanded;
        setIsExpanded(newExpandedState);

        if (newExpandedState && !hasLoadedEntries) {
            setIsLoading(true);
            try {
                const data = await userFocusApi.getFocusEvolution(focus.userId, focus.id);
                setActivities(data);
                setHasLoadedEntries(true);
            } catch (error) {
                console.error('Failed to load evolution:', error);
            } finally {
                setIsLoading(false);
            }
        }
    };

    const startAction = async (mode: 'read' | 'quiz') => {
        setIsActionLoading(mode);
        try {
            // Both actions create a NEW activity via activityApi.read
            // as requested by the user ("This action must create a NEW activity").
            const getSourceTypeNumber = (type?: string) => {
                switch (type) {
                    case 'Wikipedia': return 1;
                    case 'Document': return 2;
                    case 'Custom': return 3;
                    default: return 1;
                }
            };

            const activity = await activityApi.read(focus.userId, {
                title: focus.displayTitle,
                sourceId: focus.sourceId,
                sourceType: getSourceTypeNumber(focus.source?.type),
                language: i18n.language
            });

            if (activity?.id) {
                const url = mode === 'quiz'
                    ? `/derot?activityId=${activity.id}&mode=quiz`
                    : `/derot?activityId=${activity.id}`;
                navigate(url);
            }
        } catch (error) {
            console.error(`Failed to start ${mode}:`, error);
        } finally {
            setIsActionLoading(null);
        }
    };

    const formatDuration = (seconds: number) => {
        const minutes = Math.floor(seconds / 60);
        const remainingSeconds = seconds % 60;
        if (minutes === 0) return `${remainingSeconds}s`;
        return `${minutes}m ${remainingSeconds}s`;
    };

    return (
        <Card className={cn(
            "group transition-all duration-300 hover:shadow-md border-border/60",
            focus.isPinned && "border-primary/40 bg-primary/5 shadow-sm"
        )}>
            <CardHeader className="pb-3">
                <div className="flex justify-between items-start gap-2">
                    <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-2 mb-1">
                            {focus.isPinned && (
                                <span className="flex items-center gap-1 text-[10px] font-bold uppercase tracking-wider text-primary bg-primary/10 px-2 py-0.5 rounded-full">
                                    <Pin className="w-3 h-3" />
                                    {t('focusArea.pinned')}
                                </span>
                            )}
                            {focus.isArchived && (
                                <span className="flex items-center gap-1 text-[10px] font-bold uppercase tracking-wider text-muted-foreground bg-muted px-2 py-0.5 rounded-full">
                                    <Archive className="w-3 h-3" />
                                    {t('focusArea.archived')}
                                </span>
                            )}
                        </div>
                        <CardTitle className="text-xl leading-tight">
                            <a
                                href={focus.source?.url || focus.sourceId}
                                target="_blank"
                                rel="noopener noreferrer"
                                className="hover:text-primary transition-colors inline-flex items-center gap-2"
                            >
                                {focus.displayTitle}
                                <ExternalLink className="w-4 h-4 text-muted-foreground group-hover:text-primary transition-colors" />
                            </a>
                        </CardTitle>
                    </div>

                    <div className="flex gap-1 -mr-2">
                        {onTogglePin && (
                            <Button
                                variant="ghost"
                                size="icon"
                                className={cn("h-8 w-8", focus.isPinned && "text-primary")}
                                onClick={() => onTogglePin(focus)}
                                title={focus.isPinned ? t('common.unpin', 'Unpin') : t('common.pin', 'Pin to top')}
                            >
                                <Pin className={cn("w-4 h-4", focus.isPinned && "fill-current")} />
                            </Button>
                        )}
                        {onToggleArchive && (
                            <Button
                                variant="ghost"
                                size="icon"
                                className={cn("h-8 w-8", focus.isArchived && "text-yellow-600")}
                                onClick={() => onToggleArchive(focus)}
                                title={focus.isArchived ? t('common.unarchive', 'Unarchive') : t('common.archive', 'Archive')}
                            >
                                <Archive className={cn("w-4 h-4", focus.isArchived && "fill-current")} />
                            </Button>
                        )}
                        {onUntrack && (
                            <Button
                                variant="ghost"
                                size="icon"
                                className="h-8 w-8 text-primary/60 hover:text-primary"
                                onClick={() => onUntrack(focus)}
                                title={t('history.untrack', 'Untrack topic')}
                            >
                                <BookmarkCheck className="w-4 h-4" />
                            </Button>
                        )}
                    </div>
                </div>
            </CardHeader>

            <div className="flex gap-2 px-6 mb-4">
                <Button
                    variant="outline"
                    size="sm"
                    className="flex-1 gap-2 border-primary/20 hover:border-primary/40 hover:bg-primary/5 text-primary"
                    onClick={() => startAction('read')}
                    disabled={!!isActionLoading}
                >
                    {isActionLoading === 'read' ? <Loader2 className="w-4 h-4 animate-spin" /> : <BookOpen className="w-4 h-4" />}
                    {t('focusArea.startRead', 'New Read')}
                </Button>
                <Button
                    variant="outline"
                    size="sm"
                    className="flex-1 gap-2 border-primary/20 hover:border-primary/40 hover:bg-primary/5 text-primary"
                    onClick={() => startAction('quiz')}
                    disabled={!!isActionLoading}
                >
                    {isActionLoading === 'quiz' ? <Loader2 className="w-4 h-4 animate-spin" /> : <GraduationCap className="w-4 h-4" />}
                    {t('focusArea.startQuiz', 'New Quiz')}
                </Button>
            </div>

            <CardContent>
                <div className="grid grid-cols-2 gap-4">
                    {/* Stats */}
                    <div className="space-y-3">
                        <div className="flex items-center gap-2 text-sm">
                            <Trophy className="w-4 h-4 text-yellow-500" />
                            <div className="flex flex-col">
                                <span className="text-[10px] text-muted-foreground uppercase font-bold tracking-tight leading-none mb-1">
                                    {t('focusArea.bestScore')}
                                </span>
                                <span className="font-semibold">{Math.round(focus.bestScore)}%</span>
                            </div>
                        </div>
                        <div className="flex items-center gap-2 text-sm">
                            <History className="w-4 h-4 text-primary" />
                            <div className="flex flex-col">
                                <span className="text-[10px] text-muted-foreground uppercase font-bold tracking-tight leading-none mb-1">
                                    {t('focusArea.lastScore')}
                                </span>
                                <span className="font-semibold">{Math.round(focus.lastScore)}%</span>
                            </div>
                        </div>
                    </div>

                    <div className="space-y-3">
                        <div className="flex items-center gap-2 text-sm">
                            <Clock className="w-4 h-4 text-blue-500" />
                            <div className="flex flex-col">
                                <span className="text-[10px] text-muted-foreground uppercase font-bold tracking-tight leading-none mb-1">
                                    {t('focusArea.totalStudyTime')}
                                </span>
                                <span className="font-semibold">{formatDuration(focus.totalStudyTimeSeconds)}</span>
                            </div>
                        </div>
                        <div className="flex items-center gap-2 text-sm">
                            <Calendar className="w-4 h-4 text-green-500" />
                            <div className="flex flex-col">
                                <span className="text-[10px] text-muted-foreground uppercase font-bold tracking-tight leading-none mb-1">
                                    {t('focusArea.lastInteraction')}
                                </span>
                                <span className="font-semibold text-xs">
                                    {(() => {
                                        const d = parseDate(focus.lastAttemptDate);
                                        return isValidDate(d)
                                            ? d.toLocaleDateString(i18n.language, { day: 'numeric', month: 'short', year: '2-digit', hour: '2-digit', minute: '2-digit' })
                                            : '--';
                                    })()}
                                </span>
                            </div>
                        </div>
                    </div>
                </div>

                {/* Evolution Section */}
                <div className="mt-6 pt-4 border-t border-border/40">
                    <Button
                        variant="ghost"
                        size="sm"
                        className="w-full justify-between text-muted-foreground hover:text-foreground h-8 px-2"
                        onClick={toggleExpand}
                    >
                        <span className="text-xs font-medium">
                            {isExpanded ? t('focusArea.hideEvolution') : t('focusArea.viewEvolution')}
                        </span>
                        {isExpanded ? <ChevronUp className="w-4 h-4" /> : <ChevronDown className="w-4 h-4" />}
                    </Button>

                    {isExpanded && (
                        <div className="mt-4 space-y-4 animate-in fade-in slide-in-from-top-2 duration-300">
                            {isLoading ? (
                                <div className="flex justify-center py-6">
                                    <Loader2 className="w-6 h-6 animate-spin text-primary/40" />
                                </div>
                            ) : activities.length > 0 ? (
                                <div className="space-y-3 max-h-[300px] overflow-y-auto pr-2 custom-scrollbar">
                                    {activities.map((activity, idx) => (
                                        <ActivityTimelineItem
                                            key={activity.id}
                                            activity={activity}
                                            isTracked={true}
                                            isCompact={true}
                                            showTrackButton={false}
                                            isLast={idx === activities.length - 1}
                                            onTrack={() => { }}
                                            onUntrack={() => { }}
                                        />
                                    ))}
                                </div>
                            ) : (
                                <p className="text-xs text-center text-muted-foreground py-4">
                                    {t('history.empty')}
                                </p>
                            )}
                        </div>
                    )}
                </div>
            </CardContent>
        </Card>
    );
};
