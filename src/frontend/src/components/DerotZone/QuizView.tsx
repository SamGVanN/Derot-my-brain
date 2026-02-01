import { useEffect } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { GraduationCap, Loader2, AlertCircle, Send } from 'lucide-react';
import { QuizQuestion } from './QuizQuestion';
import { QuizResults } from './QuizResults';
import type { Quiz, QuizResult, Question } from '@/models/Quiz';

interface QuizViewProps {
    activityId: string;
    onGetDuration: () => number;
    quiz: Quiz | null;
    userAnswers: Map<number, string>;
    isLoading: boolean;
    error: string | null;
    result: QuizResult | null;
    isSubmitting: boolean;
    onGenerate: (activityId: string, initialDuration?: number) => Promise<void>;
    onAnswerChange: (questionId: number, answer: string) => void;
    onSubmit: (activityId: string, durationSeconds: number) => Promise<QuizResult | null>;
    onFinish: () => void;
}

export function QuizView({
    activityId,
    onGetDuration,
    quiz,
    userAnswers,
    isLoading,
    error,
    result,
    isSubmitting,
    onGenerate,
    onAnswerChange,
    onSubmit,
    onFinish
}: QuizViewProps) {
    // The instruction implies that 't' was destructured from useTranslation() but not used.
    // Since 't' is not present in the provided code, and no `const { t } = useTranslation();` line exists,
    // the most faithful interpretation is to remove the `useTranslation` import if it's truly unused.
    // However, the instruction specifically asks to remove 't', not `useTranslation`.
    // As 't' is not found in the code, no change is made to the component's logic.
    // If there was a line like `const { t } = useTranslation();` it would be removed.
    // Given the current content, 't' is not present to be removed.

    // Generate quiz when component mounts
    useEffect(() => {
        if (!quiz && !isLoading && !error && !result) {
            onGenerate(activityId);
        }
    }, [activityId, quiz, isLoading, error, result, onGenerate]);

    const handleRetry = () => {
        onGenerate(activityId);
    };

    const handleFinish = () => {
        onFinish();
    };

    // Loading state
    if (isLoading) {
        return (
            <div className="flex flex-col items-center justify-center py-20 space-y-4">
                <Loader2 className="h-10 w-10 animate-spin text-primary/40" />
                <div className="text-center">
                    <h3 className="text-lg font-semibold">Generating Quiz...</h3>
                    <p className="text-sm text-muted-foreground italic">Our AI is reading the content to prepare your challenges.</p>
                </div>
            </div>
        );
    }

    // Error state
    if (error) {
        return (
            <Card className="border-destructive/50 bg-destructive/5">
                <CardContent className="flex flex-col items-center justify-center py-20 space-y-4">
                    <AlertCircle className="h-10 w-10 text-destructive" />
                    <p className="text-destructive font-semibold">Erreur lors de la génération du quiz</p>
                    <p className="text-sm text-muted-foreground text-center max-w-md">{error}</p>
                    <Button onClick={() => onGenerate(activityId)} variant="outline">
                        Réessayer
                    </Button>
                </CardContent>
            </Card>
        );
    }

    // Result view
    if (result) {
        return (
            <div className="space-y-8 animate-in fade-in zoom-in-95 duration-500">
                <QuizResults
                    result={result}
                    onRetry={handleRetry}
                    onFinish={handleFinish}
                />
                <div className="space-y-6">
                    <h3 className="text-xl font-bold flex items-center gap-2 px-2">
                        <GraduationCap className="h-6 w-6 text-primary" />
                        Review Your Answers
                    </h3>
                    <div className="grid gap-6">
                        {quiz?.questions.map((question: Question, idx: number) => {
                            const qResult = result.results.find(r => r.questionId === question.id);
                            return (
                                <QuizQuestion
                                    key={question.id}
                                    number={idx + 1}
                                    question={question}
                                    userAnswer={userAnswers.get(question.id)}
                                    onAnswerChange={() => { }} // Read-only in results
                                    result={qResult}
                                    disabled={true}
                                />
                            );
                        })}
                    </div>
                    <div className="flex justify-center pt-4">
                        <Button onClick={handleFinish} variant="outline" className="px-8">
                            Back to Explorer
                        </Button>
                    </div>
                </div>
            </div>
        );
    }

    // Quiz view missing
    if (!quiz) {
        return (
            <div className="flex flex-col items-center justify-center py-20 space-y-4">
                <AlertCircle className="h-10 w-10 text-muted-foreground" />
                <p className="text-muted-foreground">Aucun quiz disponible</p>
                <Button onClick={() => onGenerate(activityId)} variant="outline">
                    Générer à nouveau
                </Button>
            </div>
        );
    }

    const allAnswered = quiz.questions.every((q: Question) => userAnswers.has(q.id));
    const answeredCount = Array.from(userAnswers.keys()).length;

    return (
        <div className="space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-700">
            <Card className="border-border/60 shadow-sm overflow-hidden bg-gradient-to-b from-background to-accent/5">
                <CardHeader className="border-b border-border/40 pb-6 flex flex-row items-center justify-between">
                    <div className="flex items-center gap-3">
                        <div className="p-2 bg-primary/10 rounded-xl">
                            <GraduationCap className="h-6 w-6 text-primary" />
                        </div>
                        <div>
                            <CardTitle className="text-2xl font-bold tracking-tight">Active Quiz</CardTitle>
                            <p className="text-sm text-muted-foreground font-medium">Test your knowledge on the article you just read.</p>
                        </div>
                    </div>
                    <div className="text-right hidden sm:block">
                        <p className="text-2xl font-bold text-primary">{answeredCount}/{quiz.questions.length}</p>
                        <p className="text-xs text-muted-foreground uppercase tracking-widest font-bold">Progress</p>
                    </div>
                </CardHeader>
                <CardContent className="p-8">
                    <div className="space-y-12">
                        {quiz.questions.map((question: Question, idx: number) => (
                            <QuizQuestion
                                key={question.id}
                                number={idx + 1}
                                question={question}
                                userAnswer={userAnswers.get(question.id)}
                                onAnswerChange={(val) => onAnswerChange(question.id, val)}
                                disabled={isSubmitting}
                            />
                        ))}

                        <div className="pt-8 border-t border-border/40 space-y-4">
                            <div className="flex flex-col sm:flex-row items-center justify-between gap-6">
                                <div className="flex items-center gap-2 italic">
                                    {allAnswered ? (
                                        <>
                                            <AlertCircle className="h-5 w-5 text-green-600" />
                                            <span className="text-green-600 font-semibold">All questions answered! Ready to submit.</span>
                                        </>
                                    ) : (
                                        <>
                                            <AlertCircle className="h-5 w-5 text-amber-500" />
                                            <span className="text-muted-foreground font-medium">Please answer all questions before submitting</span>
                                        </>
                                    )}
                                </div>

                                <Button
                                    onClick={() => onSubmit(activityId, onGetDuration())}
                                    disabled={!allAnswered || isSubmitting}
                                    size="lg"
                                    className="w-full sm:w-auto px-10 h-14 text-lg font-bold rounded-xl shadow-lg hover:shadow-primary/25 transition-all gap-3"
                                >
                                    {isSubmitting ? (
                                        <>
                                            <Loader2 className="h-6 w-6 animate-spin" />
                                            Submitting...
                                        </>
                                    ) : (
                                        <>
                                            <Send className="h-6 w-6" />
                                            Submit Quiz
                                        </>
                                    )}
                                </Button>
                            </div>
                        </div>
                    </div>
                </CardContent>
            </Card>
        </div>
    );
}
