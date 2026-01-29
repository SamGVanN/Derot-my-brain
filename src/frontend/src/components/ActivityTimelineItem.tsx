import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { BookOpenText, NotebookPen, ExternalLink, Bookmark, BookmarkCheck, Trophy, Info, PartyPopper, Radar, Flag, TrendingUp, BookOpen, GraduationCap, Loader2, Clock, ClockAlert } from 'lucide-react';
import type { UserActivity } from '../models/UserActivity';
import { cn } from '@/lib/utils';
import { Button } from './ui/button';
import { useNavigate } from 'react-router';
import { activityApi } from '../api/activityApi';
import { parseDate, isValidDate } from '@/lib/dateUtils';
import { formatScoreDisplay, formatDuration } from '@/lib/formatUtils';
import { mapSourceTypeToNumber } from '@/lib/sourceUtils';
import {
    Tooltip,
    TooltipContent,
    TooltipTrigger,
} from "@/components/ui/tooltip";

interface ActivityTimelineItemProps {
    activity: UserActivity;
    isTracked: boolean;
    onTrack: () => void;
    onUntrack: () => void;
    isLast?: boolean;
    isCompact?: boolean;
    showTrackButton?: boolean;
}

export const ActivityTimelineItem: React.FC<ActivityTimelineItemProps> = ({
    activity,
    isTracked,
    onTrack,
    onUntrack,
    isLast = false,
    isCompact = false,
    showTrackButton = true
}) => {
    const { t, i18n } = useTranslation();
    const navigate = useNavigate();
    const [isActionLoading, setIsActionLoading] = useState<string | null>(null);

    const handleToggleTrack = (e: React.MouseEvent) => {
        e.preventDefault();
        e.stopPropagation();
        if (isTracked) {
            onUntrack();
        } else {
            onTrack();
        }
    };

    return (
        <div className="flex gap-4 group">
            {/* Timeline Column */}
            {!isCompact && (
                <div className="flex flex-col items-center flex-shrink-0 w-16 pt-1">
                    <span className="text-xs font-mono text-muted-foreground">
                        {isValidDate(parseDate(activity.sessionDateStart))
                            ? parseDate(activity.sessionDateStart).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
                            : '--:--'}
                    </span>
                    <div className="relative flex flex-col items-center h-full mt-2">
                        <div className={cn(
                            "z-10 bg-background p-1 rounded-full border-2",
                            (activity.type === 'Quiz' && activity.isCurrentBest)
                                ? "border-yellow-500 text-yellow-600 dark:text-yellow-400 bg-yellow-500/10"
                                : "border-primary text-primary"
                        )}>
                            {(activity.type === 'Quiz' && activity.isCurrentBest) ? (
                                <Trophy className="w-4 h-4" />
                            ) : activity.type === 'Quiz' ? (
                                <NotebookPen className="w-4 h-4" />
                            ) : activity.type === 'Explore' ? (
                                <Radar className="w-4 h-4" />
                            ) : (
                                <BookOpenText className="w-4 h-4" />
                            )}
                        </div>
                        {!isLast && (
                            <div className="w-px h-full bg-border absolute top-3" />
                        )}
                    </div>
                </div>
            )}

            {/* Content Card */}
            <div className={cn("flex-1 min-w-0", !isCompact && "pb-8")}>
                <div className={cn(
                    "p-3.5 rounded-lg bg-card border border-border/50 hover:border-border transition-all shadow-sm",
                    isCompact && "p-3 bg-muted/30"
                )}>
                    {/* Header: Link & Actions */}
                    <div className="flex justify-between items-start gap-4 mb-2">
                        <div className="min-w-0">
                            <div className="flex items-center gap-2 mb-1">
                                <Tooltip>
                                    <TooltipTrigger asChild>
                                        <span className={cn(
                                            "px-1.5 py-0.5 rounded-full text-[10px] uppercase tracking-wide font-bold border flex items-center gap-1 cursor-default",
                                            "bg-primary/10 text-primary border-primary/20"
                                        )}>
                                            {activity.type}
                                        </span>
                                    </TooltipTrigger>
                                    <TooltipContent>
                                        <p>{t('history.tooltips.activityType', { type: activity.type })}</p>
                                    </TooltipContent>
                                </Tooltip>

                                {activity.type === 'Explore' && (
                                    <Tooltip>
                                        <TooltipTrigger asChild>
                                            <div className="flex items-center gap-1.5 text-[10px] font-medium text-muted-foreground bg-muted/50 px-2 py-0.5 rounded-md border border-border/40 cursor-default">
                                                <ClockAlert className="w-3 h-3" />
                                                <span>+{activity.backlogAddsCount || 0}</span>
                                            </div>
                                        </TooltipTrigger>
                                        <TooltipContent>
                                            <p>{t('history.tooltips.backlogAdds')}</p>
                                        </TooltipContent>
                                    </Tooltip>
                                )}

                                {activity.type === 'Quiz' && (
                                    <>
                                        {/* 1. Baseline Badge / First Attempt */}
                                        {activity.isBaseline && (
                                            <Tooltip>
                                                <TooltipTrigger asChild>
                                                    <span className={cn(
                                                        "px-1.5 py-0.5 rounded-full text-[10px] border flex items-center gap-1 font-bold cursor-default",
                                                        activity.isCurrentBest
                                                            ? "bg-indigo-500/10 text-indigo-600 dark:text-indigo-400 border-indigo-500/20"
                                                            : "bg-muted text-muted-foreground border-transparent"
                                                    )}>
                                                        <Flag className="w-3 h-3" />
                                                        {activity.isCurrentBest ? t('history.personalBestInitial', { score: Math.round(activity.scorePercentage ?? 0) }) : t('history.baseline')}
                                                    </span>
                                                </TooltipTrigger>
                                                <TooltipContent>
                                                    <p>{activity.isCurrentBest ? t('history.isInitialBestTooltip') : t('history.baselineTooltip')}</p>
                                                </TooltipContent>
                                            </Tooltip>
                                        )}

                                        {/* 2. Record Badge: New personal best at that specific time, NOT currently the best, and NOT baseline */}
                                        {activity.isNewBestScore && !activity.isCurrentBest && !activity.isBaseline && (
                                            <Tooltip>
                                                <TooltipTrigger asChild>
                                                    <span className="px-1.5 py-0.5 rounded-full text-[10px] bg-green-500/10 text-green-600 border border-green-500/20 flex items-center gap-1 font-bold cursor-default">
                                                        <TrendingUp className="w-3 h-3" />
                                                        {t('history.record')}
                                                    </span>
                                                </TooltipTrigger>
                                                <TooltipContent>
                                                    <p>{t('history.recordTooltip')}</p>
                                                </TooltipContent>
                                            </Tooltip>
                                        )}

                                        {/* 3. Trophy Badge: Current personal best (if not baseline) */}
                                        {activity.isCurrentBest && !activity.isBaseline && (
                                            <Tooltip>
                                                <TooltipTrigger asChild>
                                                    <span className={cn(
                                                        "px-1.5 py-0.5 rounded-full text-[10px] border flex items-center gap-1 font-bold cursor-default",
                                                        "bg-yellow-500/10 text-yellow-600 dark:text-yellow-400 border-yellow-500/20"
                                                    )}>
                                                        <Trophy className="w-3 h-3" />
                                                        {t('history.personalBest', { score: Math.round(activity.scorePercentage ?? 0) })}
                                                    </span>
                                                </TooltipTrigger>
                                                <TooltipContent>
                                                    <p>{t('history.isBestTooltip')}</p>
                                                </TooltipContent>
                                            </Tooltip>
                                        )}

                                        {/* 4. Simple Score Badge: Not baseline, not current best, and not a past record */}
                                        {!activity.isBaseline && !activity.isCurrentBest && !activity.isNewBestScore && (
                                            <Tooltip>
                                                <TooltipTrigger asChild>
                                                    <span className="px-1.5 py-0.5 rounded-full text-[10px] bg-muted text-muted-foreground border border-transparent flex items-center gap-1 font-bold cursor-default">
                                                        {`${Math.round(activity.scorePercentage ?? 0)}%`}
                                                    </span>
                                                </TooltipTrigger>
                                                <TooltipContent>
                                                    <p>{t('history.tooltips.score')}</p>
                                                </TooltipContent>
                                            </Tooltip>
                                        )}
                                    </>
                                )}
                            </div>

                            <a
                                href={activity.externalId}
                                target="_blank"
                                rel="noopener noreferrer"
                                className={cn(
                                    "font-semibold text-foreground hover:text-primary transition-colors flex items-center gap-1.5 truncate group/link",
                                    isCompact ? "text-sm" : "text-base"
                                )}
                            >
                                {activity.title}
                                <ExternalLink className="h-3 w-3 opacity-0 group-hover/link:opacity-100 transition-opacity flex-shrink-0" />
                            </a>
                        </div>

                        {showTrackButton && activity.type !== 'Explore' && (
                            <Tooltip>
                                <TooltipTrigger asChild>
                                    <Button
                                        variant="ghost"
                                        size="icon"
                                        className={cn(
                                            "h-7 w-7 flex-shrink-0 hover:bg-muted",
                                            isTracked ? "text-primary" : "text-muted-foreground/40 hover:text-primary"
                                        )}
                                        onClick={handleToggleTrack}
                                    >
                                        {isTracked ? <BookmarkCheck className="h-4 w-4 fill-current" /> : <Bookmark className="h-4 w-4" />}
                                    </Button>
                                </TooltipTrigger>
                                <TooltipContent>
                                    <p>{isTracked ? t('history.untrack', 'Untrack topic') : t('history.track', 'Track topic')}</p>
                                </TooltipContent>
                            </Tooltip>
                        )}
                    </div>

                    {/* Stats & Info */}
                    <div className="flex items-end justify-between mt-1.5">
                        <div className="flex-1 pr-4">
                            {activity.type === 'Explore' && (
                                <div className="space-y-1.5">
                                    {(activity.refreshCount ?? 0) > 0 && (
                                        <Tooltip>
                                            <TooltipTrigger asChild>
                                                <div className="flex items-center gap-1 text-[10px] text-muted-foreground/70 bg-muted/30 px-1.5 py-0.5 rounded border border-border/20 w-fit cursor-default">
                                                    <Radar className="w-2.5 h-2.5" />
                                                    <span>{activity.refreshCount} {t('history.refreshes', { count: activity.refreshCount })}</span>
                                                </div>
                                            </TooltipTrigger>
                                            <TooltipContent>
                                                <p>{t('history.tooltips.refreshes')}</p>
                                            </TooltipContent>
                                        </Tooltip>
                                    )}
                                    <Tooltip>
                                        <TooltipTrigger asChild>
                                            <div className={cn(
                                                "text-[11px] flex items-center gap-1 mt-1 pl-0.5 cursor-default",
                                                activity.resultingReadSourceName ? "text-primary/80" : "text-muted-foreground/60"
                                            )}>
                                                <BookOpenText className="w-3 h-3" />
                                                <span className="truncate max-w-[200px]">
                                                    {activity.resultingReadSourceName
                                                        ? `${t('history.readResult', 'Read article:')} ${activity.resultingReadSourceName}`
                                                        : t('history.noReadResult', 'No article read')
                                                    }
                                                </span>
                                            </div>
                                        </TooltipTrigger>
                                        <TooltipContent>
                                            <p>
                                                {activity.resultingReadSourceName
                                                    ? t('history.tooltips.readResult')
                                                    : t('history.tooltips.noReadResult')
                                                }
                                            </p>
                                        </TooltipContent>
                                    </Tooltip>
                                </div>
                            )}
                            {activity.type === 'Quiz' && (
                                <div>
                                    <div className="flex justify-between text-xs mb-1.5">
                                        <span className="text-muted-foreground">{t('history.score', 'Score')}</span>
                                        <span className="font-medium text-foreground">
                                            {formatScoreDisplay(activity.score, activity.questionCount)}
                                        </span>
                                    </div>
                                    <div className="h-1.5 w-full bg-muted rounded-full overflow-hidden">
                                        <div
                                            className="h-full bg-gradient-to-r from-primary/80 to-primary"
                                            style={{
                                                width: `${activity.questionCount > 0
                                                    ? (activity.score / activity.questionCount) * 100
                                                    : 0}%`
                                            }}
                                        />
                                    </div>
                                </div>
                            )}
                        </div>

                        <div className="flex flex-col items-end gap-2">
                            {/* Duration Display */}
                            {activity.durationSeconds > 0 && (
                                <Tooltip>
                                    <TooltipTrigger asChild>
                                        <div className="flex items-center gap-1.5 text-xs font-medium text-muted-foreground bg-muted/50 px-2 py-1 rounded-md border border-border/40 cursor-default">
                                            <Clock className="w-3 h-3" />
                                            {formatDuration(activity.durationSeconds)}
                                        </div>
                                    </TooltipTrigger>
                                    <TooltipContent>
                                        <p>{t('history.tooltips.duration')}</p>
                                    </TooltipContent>
                                </Tooltip>
                            )}

                            {/* LLM Info Tooltip */}
                            {activity.llmModelName && (
                                <Tooltip>
                                    <TooltipTrigger asChild>
                                        <div className="flex items-center gap-1 text-xs text-muted-foreground/60 cursor-help hover:text-muted-foreground transition-colors">
                                            <Info className="w-3.5 h-3.5" />
                                            <span className="hidden sm:inline">AI Info</span>
                                        </div>
                                    </TooltipTrigger>
                                    <TooltipContent>
                                        <p className="font-semibold">{activity.llmModelName}</p>
                                        <p className="text-xs text-muted-foreground">{activity.llmVersion}</p>
                                    </TooltipContent>
                                </Tooltip>
                            )}
                        </div>
                    </div>

                    {/* Action Buttons: Relire / Passer un quiz (non-compact, non-Explore) */}
                    {!isCompact && activity.type !== 'Explore' && (
                        <div className="flex gap-2 mt-3 pt-3 border-t border-border/20">
                            <Tooltip>
                                <TooltipTrigger asChild>
                                    <Button
                                        variant="outline"
                                        size="sm"
                                        className="flex-1 h-8 gap-2 border-primary/20 hover:border-primary/40 hover:bg-primary/5 text-primary text-xs hover:text-primary"
                                        disabled={!!isActionLoading}
                                        onClick={async (e) => {
                                            e.preventDefault();
                                            e.stopPropagation();

                                            setIsActionLoading('read');
                                            try {
                                                const newActivity = await activityApi.read(activity.userId, {
                                                    title: activity.title,
                                                    sourceId: activity.sourceId,
                                                    sourceType: mapSourceTypeToNumber(activity.sourceType),
                                                    language: i18n.language,
                                                    type: 'Read'
                                                });

                                                if (newActivity?.id) {
                                                    navigate(`/derot?activityId=${newActivity.id}`);
                                                }
                                            } catch (err) {
                                                console.error('Failed to create read activity:', err);
                                            } finally {
                                                setIsActionLoading(null);
                                            }
                                        }}
                                    >
                                        {isActionLoading === 'read' ? <Loader2 className="w-4 h-4 animate-spin" /> : <BookOpen className="w-4 h-4" />}
                                        {t('history.reRead')}
                                    </Button>
                                </TooltipTrigger>
                                <TooltipContent>
                                    <p>{t('history.reRead')}</p>
                                </TooltipContent>
                            </Tooltip>

                            <Tooltip>
                                <TooltipTrigger asChild>
                                    <Button
                                        variant="outline"
                                        size="sm"
                                        className="flex-1 h-8 gap-2 border-primary/20 hover:border-primary/40 hover:bg-primary/5 text-primary text-xs hover:text-primary"
                                        disabled={!!isActionLoading}
                                        onClick={async (e) => {
                                            e.preventDefault();
                                            e.stopPropagation();

                                            setIsActionLoading('quiz');
                                            try {
                                                const newActivity = await activityApi.read(activity.userId, {
                                                    title: activity.title,
                                                    sourceId: activity.sourceId,
                                                    sourceType: mapSourceTypeToNumber(activity.sourceType),
                                                    language: i18n.language,
                                                    type: 'Quiz'
                                                });

                                                if (newActivity?.id) {
                                                    navigate(`/derot?activityId=${newActivity.id}&mode=quiz`);
                                                }
                                            } catch (err) {
                                                console.error('Failed to create quiz activity:', err);
                                            } finally {
                                                setIsActionLoading(null);
                                            }
                                        }}
                                    >
                                        {isActionLoading === 'quiz' ? <Loader2 className="w-4 h-4 animate-spin" /> : <GraduationCap className="w-4 h-4" />}
                                        {t('history.takeQuiz')}
                                    </Button>
                                </TooltipTrigger>
                                <TooltipContent>
                                    <p>{t('history.takeQuiz')}</p>
                                </TooltipContent>
                            </Tooltip>
                        </div>
                    )
                    }

                    {/* Motivational Text */}
                    {
                        activity.type === 'Quiz' && activity.questionCount > 0 && (() => {
                            const percentage = (activity.score / activity.questionCount) * 100;
                            if (percentage <= 60) return null;

                            let messageKey = '';
                            let colorClass = 'text-muted-foreground';
                            let IconIcon = null;

                            if (percentage === 100) {
                                messageKey = 'history.motivational.perfect';
                                colorClass = 'text-green-600 dark:text-green-400 font-bold';
                                IconIcon = PartyPopper;
                            } else if (percentage > 90) {
                                messageKey = 'history.motivational.tier4';
                                colorClass = 'text-green-600 dark:text-green-400 font-medium';
                            } else if (percentage > 80) {
                                messageKey = 'history.motivational.tier3';
                                colorClass = 'text-primary font-medium';
                            } else if (percentage > 70) {
                                messageKey = 'history.motivational.tier2';
                                colorClass = 'text-primary/80';
                            } else {
                                messageKey = 'history.motivational.tier1';
                                colorClass = 'text-muted-foreground';
                            }

                            return (
                                <div className={cn("text-xs flex items-center gap-1.5 mt-3 pl-1", colorClass)}>
                                    {IconIcon && <IconIcon className="w-3.5 h-3.5" />}
                                    <span>{t(messageKey)}</span>
                                </div>
                            );
                        })()
                    }
                </div >
            </div >
        </div >
    );
};
