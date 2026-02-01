import { Button } from '@/components/ui/button';
import { Radar, StopCircle, GraduationCap, ArrowRight, Send, Loader2 } from 'lucide-react';
import type { ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import {
    Tooltip,
    TooltipContent,
    TooltipTrigger,
} from "@/components/ui/tooltip";

export type DerotZoneMode = 'EXPLORE' | 'READ' | 'QUIZ';

interface DerotZoneSubHeaderProps {
    mode: DerotZoneMode;
    onStopActivity?: () => void;
    onGoToQuiz?: () => void;
    onSubmitQuiz?: () => void;
    isQuizSubmittable?: boolean;
    isSubmitting?: boolean;
    children?: ReactNode; // For filters or other contextual content
}

export function DerotZoneSubHeader({
    mode,
    onStopActivity,
    onGoToQuiz,
    onSubmitQuiz,
    isQuizSubmittable = false,
    isSubmitting = false,
    children
}: DerotZoneSubHeaderProps) {
    const { t } = useTranslation();

    return (
        <div className="flex flex-col md:flex-row items-center justify-between gap-4 p-4 bg-accent/20 rounded-xl border border-accent/30 animate-in fade-in slide-in-from-left-4 duration-500">
            <div className="flex items-center gap-4 w-full md:w-auto">
                {mode === 'EXPLORE' && (
                    <div className="flex items-center gap-3 flex-1 md:flex-none">
                        <div className="p-2 bg-primary/10 rounded-lg">
                            <Radar className="h-5 w-5 text-primary" />
                        </div>
                        <div className="flex flex-col">
                            <span className="text-xs font-semibold uppercase tracking-wider text-primary/70">Mode</span>
                            <span className="text-sm font-bold">{t('derot.subheader.modeExplore', 'Exploration')}</span>
                        </div>
                        {children}
                    </div>
                )}

                {mode === 'READ' && (
                    <div className="flex items-center gap-3">
                        <div className="p-2 bg-chart-1/10 rounded-lg">
                            <GraduationCap className="h-5 w-5 text-chart-1" />
                        </div>
                        <div className="flex flex-col">
                            <span className="text-xs font-semibold uppercase tracking-wider text-chart-1/70">Mode</span>
                            <span className="text-sm font-bold">{t('derot.subheader.modeRead', 'Lecture')}</span>
                        </div>
                    </div>
                )}

                {mode === 'QUIZ' && (
                    <div className="flex items-center gap-3">
                        <div className="p-2 bg-chart-2/10 rounded-lg">
                            <Send className="h-5 w-5 text-chart-2" />
                        </div>
                        <div className="flex flex-col">
                            <span className="text-xs font-semibold uppercase tracking-wider text-chart-2/70">Mode</span>
                            <span className="text-sm font-bold">{t('derot.subheader.modeQuiz', 'Quiz')}</span>
                        </div>
                    </div>
                )}
            </div>

            <div className="flex items-center gap-3 w-full md:w-auto justify-end">
                {/* Universal Stop Button */}
                <Button
                    variant="destructive"
                    size="sm"
                    onClick={onStopActivity}
                    className="gap-2"
                >
                    <StopCircle className="h-4 w-4" />
                    {mode === 'EXPLORE'
                        ? t('derot.subheader.stopExplore', "Arrêter l'exploration")
                        : t('derot.subheader.stopActivity', "Quitter l'activité")}
                </Button>

                {mode === 'READ' && (
                    <Button
                        variant="default"
                        size="sm"
                        onClick={onGoToQuiz}
                        className="gap-2"
                    >
                        {t('derot.subheader.goToQuiz', 'Passer au Quiz')}
                        <ArrowRight className="h-4 w-4" />
                    </Button>
                )}

                {mode === 'QUIZ' && (
                    <Tooltip>
                        <TooltipTrigger asChild>
                            <div className="inline-block">
                                <Button
                                    variant="default"
                                    size="sm"
                                    onClick={onSubmitQuiz}
                                    disabled={!isQuizSubmittable || isSubmitting}
                                    className="gap-2"
                                >
                                    {isSubmitting ? (
                                        <Loader2 className="h-4 w-4 animate-spin" />
                                    ) : (
                                        <Send className="h-4 w-4" />
                                    )}
                                    {t('derot.subheader.submitQuiz', 'Soumettre mes réponses')}
                                </Button>
                            </div>
                        </TooltipTrigger>
                        {!isQuizSubmittable && !isSubmitting && (
                            <TooltipContent>
                                <p>{t('derot.quiz.validationWarning', 'Veuillez répondre à toutes les questions avant de soumettre')}</p>
                            </TooltipContent>
                        )}
                    </Tooltip>
                )}
            </div>
        </div>
    );
}
