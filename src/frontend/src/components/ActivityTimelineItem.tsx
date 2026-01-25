import React from 'react';
import { useTranslation } from 'react-i18next';
import { BookOpenText, NotebookPen, ExternalLink, Bookmark, BookmarkCheck, Trophy, Info, PartyPopper } from 'lucide-react';
import type { UserActivity } from '../models/UserActivity';
import { cn } from '@/lib/utils';
import { Button } from './ui/button';
import { parseDate, isValidDate } from '@/lib/dateUtils';
import { formatScoreDisplay } from '@/lib/formatUtils';
import {
    Tooltip,
    TooltipContent,
    TooltipProvider,
    TooltipTrigger,
} from "@/components/ui/tooltip";

interface ActivityTimelineItemProps {
    activity: UserActivity;
    isTracked: boolean;
    bestScore?: { score: number; lastScore: number };
    isCurrentBest?: boolean;
    isBaseline?: boolean;
    onTrack: () => void;
    onUntrack: () => void;
    isLast?: boolean;
    isCompact?: boolean;
}

export const ActivityTimelineItem: React.FC<ActivityTimelineItemProps> = ({
    activity,
    isTracked,
    bestScore,
    isCurrentBest = false,
    isBaseline = false,
    onTrack,
    onUntrack,
    isLast = false,
    isCompact = false
}) => {
    const { t } = useTranslation();

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
                            isCurrentBest
                                ? "border-yellow-500 text-yellow-600 dark:text-yellow-400 bg-yellow-500/10"
                                : "border-primary text-primary"
                        )}>
                            {isCurrentBest ? (
                                <Trophy className="w-4 h-4" />
                            ) : activity.type === 'Quiz' ? (
                                <NotebookPen className="w-4 h-4" />
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
                    "p-4 rounded-lg bg-card border border-border/50 hover:border-border transition-all shadow-sm",
                    isCompact && "p-3 bg-muted/30"
                )}>
                    {/* Header: Link & Actions */}
                    <div className="flex justify-between items-start gap-4 mb-3">
                        <div className="min-w-0">
                            <div className="flex items-center gap-2 mb-1">
                                <span className={cn(
                                    "px-1.5 py-0.5 rounded-full text-[10px] uppercase tracking-wide font-bold border flex items-center gap-1",
                                    "bg-primary/10 text-primary border-primary/20"
                                )}>
                                    {activity.type}
                                </span>
                                {activity.isNewBestScore && !isCurrentBest && !isBaseline && (
                                    <TooltipProvider>
                                        <Tooltip>
                                            <TooltipTrigger asChild>
                                                <span className="px-1.5 py-0.5 rounded-full text-[10px] bg-green-500/10 text-green-600 border border-green-500/20 flex items-center gap-1">
                                                    <PartyPopper className="w-3 h-3" />
                                                    {t('history.record', 'Record')}
                                                </span>
                                            </TooltipTrigger>
                                            <TooltipContent>
                                                <p>{t('history.recordTooltip', 'This activity was a new personal best at that time!')}</p>
                                            </TooltipContent>
                                        </Tooltip>
                                    </TooltipProvider>
                                )}
                                {isBaseline && !isCurrentBest && (
                                    <TooltipProvider>
                                        <Tooltip>
                                            <TooltipTrigger asChild>
                                                <span className="px-1.5 py-0.5 rounded-full text-[10px] bg-muted text-muted-foreground border border-transparent flex items-center gap-1">
                                                    <Info className="w-3 h-3" />
                                                    {t('history.baseline', 'Baseline')}
                                                </span>
                                            </TooltipTrigger>
                                            <TooltipContent>
                                                <p>{t('history.baselineTooltip', 'This was your first assessment for this topic.')}</p>
                                            </TooltipContent>
                                        </Tooltip>
                                    </TooltipProvider>
                                )}
                                {bestScore && (
                                    <TooltipProvider>
                                        <Tooltip>
                                            <TooltipTrigger asChild>
                                                <span className={cn(
                                                    "px-1.5 py-0.5 rounded-full text-[10px] border flex items-center gap-1",
                                                    isCurrentBest
                                                        ? "bg-yellow-500/10 text-yellow-600 dark:text-yellow-400 border-yellow-500/20"
                                                        : "bg-muted text-muted-foreground border-transparent"
                                                )}>
                                                    <Trophy className="w-3 h-3" />
                                                    {isCurrentBest
                                                        ? isBaseline
                                                            ? t('history.personalBestInitial', 'Baseline (Best): {{score}}%', { score: Math.round(bestScore.score) })
                                                            : t('history.personalBest', 'Personal Best: {{score}}%', { score: Math.round(bestScore.score) })
                                                        : `${Math.round(bestScore.score)}%`
                                                    }
                                                </span>
                                            </TooltipTrigger>
                                            <TooltipContent>
                                                <p>
                                                    {isCurrentBest
                                                        ? isBaseline
                                                            ? t('history.isInitialBestTooltip', 'This was your first attempt and is still your best score!')
                                                            : t('history.isBestTooltip', 'This is your current best score for this topic!')
                                                        : t('history.bestCompareTooltip', 'Your personal best for this topic is {{score}}%', { score: Math.round(bestScore.score) })
                                                    }
                                                </p>
                                            </TooltipContent>
                                        </Tooltip>
                                    </TooltipProvider>
                                )}
                            </div>

                            <a
                                href={activity.sourceId}
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

                        <Button
                            variant="ghost"
                            size="icon"
                            className={cn(
                                "h-8 w-8 flex-shrink-0 hover:bg-muted",
                                isTracked ? "text-primary" : "text-muted-foreground/40 hover:text-primary"
                            )}
                            onClick={handleToggleTrack}
                            title={isTracked ? t('history.untrack', 'Untrack topic') : t('history.track', 'Track topic')}
                        >
                            {isTracked ? <BookmarkCheck className="h-5 w-5 fill-current" /> : <Bookmark className="h-5 w-5" />}
                        </Button>
                    </div>

                    {/* Stats & Info */}
                    <div className="flex items-end justify-between mt-4">
                        <div className="flex-1 pr-4">
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

                        {/* LLM Info Tooltip */}
                        {activity.llmModelName && (
                            <TooltipProvider>
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
                            </TooltipProvider>
                        )}
                    </div>

                    {/* Motivational Text */}
                    {activity.type === 'Quiz' && activity.questionCount > 0 && (() => {
                        const percentage = (activity.score / activity.questionCount) * 100;
                        if (percentage <= 60) return null;

                        let messageKey = '';
                        let colorClass = 'text-muted-foreground';
                        let Icon = null;

                        if (percentage === 100) {
                            messageKey = 'history.motivational.perfect';
                            colorClass = 'text-green-600 dark:text-green-400 font-bold';
                            Icon = PartyPopper;
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
                                {Icon && <Icon className="w-3.5 h-3.5" />}
                                <span>{t(messageKey)}</span>
                            </div>
                        );
                    })()}
                </div>
            </div>
        </div>
    );
};
