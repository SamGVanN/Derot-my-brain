import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Trophy, RotateCcw, CheckCircle2 } from 'lucide-react';
import type { QuizResult } from '@/models/Quiz';

interface QuizResultsProps {
    result: QuizResult;
    onRetry: () => void;
    onFinish: () => void;
}

export function QuizResults({ result, onRetry, onFinish }: QuizResultsProps) {
    const { totalQuestions, correctAnswers, scorePercentage } = result;

    // Determine performance level
    const getPerformanceLevel = () => {
        if (scorePercentage >= 90) return { label: 'Excellent!', color: 'text-green-600 dark:text-green-400', bgColor: 'bg-green-500/10' };
        if (scorePercentage >= 70) return { label: 'Good Job!', color: 'text-blue-600 dark:text-blue-400', bgColor: 'bg-blue-500/10' };
        if (scorePercentage >= 50) return { label: 'Not Bad!', color: 'text-yellow-600 dark:text-yellow-400', bgColor: 'bg-yellow-500/10' };
        return { label: 'Keep Practicing!', color: 'text-orange-600 dark:text-orange-400', bgColor: 'bg-orange-500/10' };
    };

    const performance = getPerformanceLevel();

    return (
        <Card className="border-border/30 bg-card/30 backdrop-blur-xl rounded-3xl overflow-hidden shadow-xl animate-in fade-in slide-in-from-bottom-4 duration-700">
            <CardHeader className="flex flex-row items-center gap-6 border-b border-border/20 p-8">
                <div className={`p-4 rounded-2xl ${performance.bgColor} shadow-sm`}>
                    <Trophy className={`h-8 w-8 ${performance.color}`} />
                </div>
                <div className="space-y-1.5 flex-1">
                    <CardTitle className="text-3xl font-bold tracking-tight">Quiz Completed!</CardTitle>
                    <p className={`text-2xl font-bold ${performance.color}`}>{performance.label}</p>
                </div>
            </CardHeader>

            <CardContent className="p-8 space-y-8">
                {/* Score Summary */}
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                    <div className="p-6 rounded-2xl bg-primary/5 border border-primary/10">
                        <p className="text-sm font-semibold text-muted-foreground mb-2">Score</p>
                        <p className="text-4xl font-bold text-primary">
                            {correctAnswers}/{totalQuestions}
                        </p>
                    </div>

                    <div className="p-6 rounded-2xl bg-primary/5 border border-primary/10">
                        <p className="text-sm font-semibold text-muted-foreground mb-2">Percentage</p>
                        <p className="text-4xl font-bold text-primary">
                            {scorePercentage.toFixed(0)}%
                        </p>
                    </div>

                    <div className="p-6 rounded-2xl bg-primary/5 border border-primary/10">
                        <p className="text-sm font-semibold text-muted-foreground mb-2">Accuracy</p>
                        <div className="flex items-center gap-2">
                            <CheckCircle2 className="w-6 h-6 text-green-600" />
                            <p className="text-4xl font-bold text-primary">
                                {correctAnswers}
                            </p>
                        </div>
                    </div>
                </div>

                {/* Performance Message */}
                <div className={`p-6 rounded-2xl ${performance.bgColor} border border-border/20`}>
                    <p className="text-center text-foreground/80 leading-relaxed">
                        {scorePercentage >= 90 && "Outstanding performance! You've mastered this content."}
                        {scorePercentage >= 70 && scorePercentage < 90 && "Great work! You have a solid understanding of the material."}
                        {scorePercentage >= 50 && scorePercentage < 70 && "Good effort! Review the explanations to improve your understanding."}
                        {scorePercentage < 50 && "Don't give up! Review the content and try again to improve your score."}
                    </p>
                </div>

                {/* Action Buttons */}
                <div className="flex gap-4 justify-center pt-4">
                    <Button
                        onClick={onRetry}
                        variant="outline"
                        size="lg"
                        className="gap-2 rounded-xl px-8"
                    >
                        <RotateCcw className="w-5 h-5" />
                        Try Again
                    </Button>
                    <Button
                        onClick={onFinish}
                        size="lg"
                        className="gap-2 rounded-xl px-8 shadow-lg shadow-primary/20"
                    >
                        <CheckCircle2 className="w-5 h-5" />
                        Finish
                    </Button>
                </div>
            </CardContent>
        </Card>
    );
}
