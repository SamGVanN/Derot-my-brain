import { Button } from '@/components/ui/button';
import { Radar, StopCircle, GraduationCap, Send, ArrowRight } from 'lucide-react';
import type { ReactNode } from 'react';
import { useTranslation } from 'react-i18next';

export type DerotZoneMode = 'EXPLORE' | 'READ' | 'QUIZ';

interface DerotZoneSubHeaderProps {
    mode: DerotZoneMode;
    onStopExplore?: () => void;
    onGoToQuiz?: () => void;
    onSubmitQuiz?: () => void;
    children?: ReactNode; // For filters or other contextual content
}

export function DerotZoneSubHeader({
    mode,
    onStopExplore,
    onGoToQuiz,
    onSubmitQuiz,
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
                {mode === 'EXPLORE' && (
                    <Button
                        variant="destructive"
                        size="sm"
                        onClick={onStopExplore}
                        className="gap-2"
                    >
                        <StopCircle className="h-4 w-4" />
                        {t('derot.subheader.stopExplore', "Arrêter l'exploration")}
                    </Button>
                )}

                {mode === 'READ' && (
                    <Button
                        variant="default" /* Used to be custom blue */
                        size="sm"
                        onClick={onGoToQuiz}
                        className="gap-2"
                    >
                        {t('derot.subheader.goToQuiz', 'Passer au Quiz')}
                        <ArrowRight className="h-4 w-4" />
                    </Button>
                )}

                {mode === 'QUIZ' && (
                    <Button
                        variant="default" /* Used to be custom purple */
                        size="sm"
                        onClick={onSubmitQuiz}
                        className="gap-2"
                    >
                        {t('derot.subheader.submitQuiz', 'Soumettre mes réponses')}
                        <Send className="h-4 w-4" />
                    </Button>
                )}
            </div>
        </div>
    );
}
