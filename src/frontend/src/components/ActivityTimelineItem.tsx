import React from 'react';
import { useTranslation } from 'react-i18next';
import { BookOpenText, NotebookPen, ExternalLink, Bookmark, BookmarkCheck, Trophy, Info, PartyPopper } from 'lucide-react';
import type { UserActivity } from '../models/UserActivity';
import { cn } from '@/lib/utils';
import { Button } from './ui/button';
import { parseDate, isValidDate } from '@/lib/dateUtils';
import { calculatePercentage, formatScoreDisplay, isBestScore } from '@/lib/formatUtils';
import {
    Tooltip,
    TooltipContent,
    TooltipProvider,
    TooltipTrigger,
} from "@/components/ui/tooltip";

interface ActivityTimelineItemProps {
    activity: UserActivity;
    isTracked: boolean;
    bestScore?: { score: number; total?: number };
    onTrack: () => void;
    onUntrack: () => void;
    isLast?: boolean;
}

export const ActivityTimelineItem: React.FC<ActivityTimelineItemProps> = ({
    activity,
    isTracked,
    bestScore,
    onTrack,
    onUntrack,
    isLast = false
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
            <div className="flex flex-col items-center flex-shrink-0 w-16 pt-1">
                <span className="text-xs font-mono text-muted-foreground">
                    {isValidDate(parseDate(activity.sessionDate))
                        ? parseDate(activity.sessionDate).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
                        : '--:--'}
                </span>
                <div className="relative flex flex-col items-center h-full mt-2">
                    <div className={cn(
                        "z-10 bg-background p-1 rounded-full border-2",
                        isBestScore(activity.score, bestScore, isTracked)
                            ? "border-yellow-500 text-yellow-600 dark:text-yellow-400 bg-yellow-500/10"
                            : "border-primary text-primary"
                    )}>
                        {isBestScore(activity.score, bestScore, isTracked) ? (
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

            {/* Content Card */}
            <div className="flex-1 pb-8 min-w-0">
                <div className="p-4 rounded-lg bg-card border border-border/50 hover:border-border transition-all shadow-sm">
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
                                {isTracked && bestScore && (
                                    <span className="px-1.5 py-0.5 rounded-full text-[10px] bg-yellow-500/10 text-yellow-600 dark:text-yellow-400 border border-yellow-500/20 flex items-center gap-1" title={t('history.bestScore', 'All-time Best')}>
                                        <Trophy className="w-3 h-3" />
                                        {bestScore!.total
                                            ? calculatePercentage(bestScore!.score, bestScore!.total)
                                            : bestScore!.score}%
                                    </span>
                                )}
                            </div>

                            <a
                                href={activity.wikipediaUrl}
                                target="_blank"
                                rel="noopener noreferrer"
                                className="text-base font-semibold text-foreground hover:text-primary transition-colors flex items-center gap-1.5 truncate group/link"
                            >
                                {activity.topic}
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
                            {activity.score !== undefined && activity.score !== null && (
                                <div>
                                    <div className="flex justify-between text-xs mb-1.5">
                                        <span className="text-muted-foreground">{t('history.score', 'Score')}</span>
                                        <span className="font-medium text-foreground">
                                            {formatScoreDisplay(activity.score, activity.totalQuestions ?? undefined)}
                                        </span>
                                    </div>
                                    <div className="h-1.5 w-full bg-muted rounded-full overflow-hidden">
                                        <div
                                            className="h-full bg-gradient-to-r from-primary/80 to-primary"
                                            style={{
                                                width: `${activity.totalQuestions
                                                    ? (activity.score! / activity.totalQuestions) * 100
                                                    : activity.score}%`
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

                    {/* Motivational Text - Moved to separate row */}
                    {activity.score !== undefined && activity.totalQuestions && (() => {
                        const percentage = (activity.score / activity.totalQuestions) * 100;
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
