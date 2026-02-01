import { Card, CardContent } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { CheckCircle2, XCircle, AlertCircle } from 'lucide-react';
import type { Question, QuestionResult } from '@/models/Quiz';

interface QuizQuestionProps {
    question: Question;
    number: number;
    userAnswer: string | undefined;
    onAnswerChange: (answer: string) => void;
    result?: QuestionResult;
    disabled: boolean;
}

export function QuizQuestion({
    question,
    number,
    userAnswer,
    onAnswerChange,
    result,
    disabled
}: QuizQuestionProps) {
    const showResult = !!result;
    const isCorrect = result?.isCorrect ?? false;

    return (
        <Card className={`
      border transition-all duration-300
      ${showResult
                ? isCorrect
                    ? 'border-green-500/50 bg-green-500/5'
                    : 'border-red-500/50 bg-red-500/5'
                : 'border-border/40 bg-accent/10'
            }
    `}>
            <CardContent className="pt-6 space-y-4">
                {/* Question Header */}
                <div className="flex items-start gap-3">
                    <div className={`
            flex items-center justify-center w-8 h-8 rounded-full text-sm font-bold shrink-0
            ${showResult
                            ? isCorrect
                                ? 'bg-green-500/20 text-green-600 dark:text-green-400'
                                : 'bg-red-500/20 text-red-600 dark:text-red-400'
                            : 'bg-primary/20 text-primary'
                        }
          `}>
                        {showResult ? (
                            isCorrect ? <CheckCircle2 className="w-4 h-4" /> : <XCircle className="w-4 h-4" />
                        ) : (
                            number
                        )}
                    </div>
                    <h4 className="font-bold text-foreground flex-1 leading-relaxed">
                        {question.text}
                    </h4>
                </div>

                {/* Answer Options/Input */}
                {question.type === 'MCQ' && question.options ? (
                    <div className="grid grid-cols-1 gap-2 pl-11">
                        {question.options.map((option, idx) => {
                            const isSelected = userAnswer === option;
                            const isCorrectOption = showResult && option === result?.correctAnswer;
                            const isWrongSelection = showResult && isSelected && !isCorrect;

                            return (
                                <button
                                    key={idx}
                                    onClick={() => !disabled && onAnswerChange(option)}
                                    disabled={disabled}
                                    className={`
                    flex items-center gap-3 p-3 rounded-lg border-2 text-left transition-all
                    ${disabled ? 'cursor-not-allowed' : 'cursor-pointer hover:border-primary/40 hover:bg-primary/5'}
                    ${isSelected && !showResult ? 'border-primary bg-primary/10' : 'border-border/40 bg-background/50'}
                    ${isCorrectOption ? 'border-green-500 bg-green-500/10' : ''}
                    ${isWrongSelection ? 'border-red-500 bg-red-500/10' : ''}
                  `}
                                >
                                    <div className={`
                    w-5 h-5 rounded-full border-2 shrink-0 flex items-center justify-center
                    ${isSelected && !showResult ? 'border-primary bg-primary' : 'border-border/60'}
                    ${isSelected && showResult && isCorrectOption ? 'border-green-500 bg-green-500' : ''}
                    ${isSelected && showResult && isWrongSelection ? 'border-red-500 bg-red-500' : ''}
                    ${!isSelected && isCorrectOption ? 'border-green-500' : ''}
                  `}>
                                        {isSelected && (
                                            <div className="w-2 h-2 rounded-full bg-white" />
                                        )}
                                    </div>
                                    <span className={`
                    text-sm flex-1
                    ${isCorrectOption ? 'font-semibold text-green-700 dark:text-green-300' : ''}
                    ${isWrongSelection ? 'text-red-700 dark:text-red-300' : 'text-foreground/80'}
                  `}>
                                        {option}
                                    </span>
                                    {isCorrectOption && <CheckCircle2 className="w-4 h-4 text-green-600" />}
                                    {isWrongSelection && <XCircle className="w-4 h-4 text-red-600" />}
                                </button>
                            );
                        })}
                    </div>
                ) : (
                    <div className="pl-11">
                        <Input
                            value={userAnswer || ''}
                            onChange={(e) => onAnswerChange(e.target.value)}
                            disabled={disabled}
                            placeholder="Type your answer here..."
                            className={`
                ${showResult
                                    ? isCorrect
                                        ? 'border-green-500 bg-green-500/5'
                                        : 'border-red-500 bg-red-500/5'
                                    : ''
                                }
              `}
                        />
                    </div>
                )}

                {/* Result Feedback */}
                {showResult && result && (
                    <div className="pl-11 space-y-2">
                        {/* Semantic Score for Open-Ended */}
                        {question.type === 'OpenEnded' && result.semanticScore !== undefined && (
                            <div className="flex items-center gap-2 text-sm">
                                <AlertCircle className="w-4 h-4 text-muted-foreground" />
                                <span className="text-muted-foreground">
                                    Similarity Score: <span className="font-semibold">{(result.semanticScore * 100).toFixed(0)}%</span>
                                </span>
                            </div>
                        )}

                        {/* Show correct answer if wrong */}
                        {!isCorrect && result.correctAnswer && (
                            <div className="p-3 rounded-lg bg-muted/50 border border-border/40">
                                <p className="text-xs font-semibold text-muted-foreground mb-1">Correct Answer:</p>
                                <p className="text-sm text-foreground">{result.correctAnswer}</p>
                            </div>
                        )}

                        {/* Explanation */}
                        {result.explanation && (
                            <div className={`
                p-3 rounded-lg border
                ${isCorrect
                                    ? 'bg-green-500/5 border-green-500/20'
                                    : 'bg-blue-500/5 border-blue-500/20'
                                }
              `}>
                                <p className="text-xs font-semibold text-muted-foreground mb-1">Explanation:</p>
                                <p className="text-sm text-foreground/90">{result.explanation}</p>
                            </div>
                        )}
                    </div>
                )}
            </CardContent>
        </Card>
    );
}
