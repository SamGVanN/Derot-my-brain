import { useState, useCallback, useRef } from 'react';
import { generateQuiz, submitQuiz } from '@/api/quizApi';
import { useAuth } from './useAuth';
import type { Quiz, QuizResult, AnswerSubmission } from '@/models/Quiz';

/**
 * Custom hook for managing quiz state and operations
 */
export function useQuiz() {
    const { user } = useAuth();
    const [quiz, setQuiz] = useState<Quiz | null>(null);
    const [userAnswers, setUserAnswers] = useState<Map<number, string>>(new Map());
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [result, setResult] = useState<QuizResult | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [startTime, setStartTime] = useState<number | null>(null);
    const [accumulatedDuration, setAccumulatedDuration] = useState(0);

    // Refs to track state without re-renders
    const isGeneratingRef = useRef(false);
    const lastActivityIdRef = useRef<string | null>(null);

    /**
     * Generate a quiz for the given activity
     */
    const generate = useCallback(async (activityId: string, initialDuration?: number) => {
        if (!user?.id) {
            setError('User not authenticated');
            return;
        }

        if (isGeneratingRef.current) {
            console.log('Quiz generation already in progress, skipping duplicate call');
            return;
        }

        // Handle activity transition or resumption
        if (lastActivityIdRef.current !== activityId) {
            setAccumulatedDuration(initialDuration ?? 0);
            lastActivityIdRef.current = activityId;
        }

        isGeneratingRef.current = true;
        setIsLoading(true);
        setError(null);
        setQuiz(null);
        setUserAnswers(new Map());
        setResult(null);

        try {
            const quizData = await generateQuiz(user.id, activityId);
            setQuiz(quizData);
            setStartTime(Date.now());
        } catch (err) {
            const errorMessage = err instanceof Error ? err.message : 'Failed to generate quiz';
            setError(errorMessage);
            console.error('Error generating quiz:', err);
        } finally {
            setIsLoading(false);
            isGeneratingRef.current = false;
        }
    }, [user?.id]);

    /**
     * Select an answer for a question
     */
    const selectAnswer = useCallback((questionId: number, answer: string) => {
        setUserAnswers(prev => {
            const newAnswers = new Map(prev);
            newAnswers.set(questionId, answer);
            return newAnswers;
        });
    }, []);

    /**
     * Check if all questions have been answered
     */
    const allQuestionsAnswered = useCallback(() => {
        if (!quiz) return false;
        return quiz.questions.every(q => userAnswers.has(q.id));
    }, [quiz, userAnswers]);

    /**
     * Calculate current total duration
     */
    const getDuration = useCallback(() => {
        const currentSessionSeconds = startTime ? Math.floor((Date.now() - startTime) / 1000) : 0;
        return accumulatedDuration + currentSessionSeconds;
    }, [accumulatedDuration, startTime]);

    /**
     * Submit the quiz and get results
     */
    const submit = useCallback(async (activityId: string, durationOverride?: number): Promise<QuizResult | null> => {
        if (!user?.id || !quiz) {
            setError('Cannot submit quiz: missing user or quiz data');
            return null;
        }

        if (!allQuestionsAnswered()) {
            setError('Please answer all questions before submitting');
            return null;
        }

        setIsSubmitting(true);
        setError(null);

        try {
            // Calculate total cumulative duration
            const totalDurationSeconds = durationOverride !== undefined
                ? durationOverride
                : getDuration();

            // Store current accumulated for retries and STOP timer
            setAccumulatedDuration(totalDurationSeconds);
            setStartTime(null);

            const answers: AnswerSubmission[] = quiz.questions.map(q => {
                const userAnswer = userAnswers.get(q.id) || '';

                if (q.type === 'MCQ') {
                    return {
                        questionId: q.id,
                        selectedOption: userAnswer
                    };
                } else {
                    return {
                        questionId: q.id,
                        textAnswer: userAnswer
                    };
                }
            });

            const submission = {
                answers,
                durationSeconds: totalDurationSeconds
            };

            const quizResult = await submitQuiz(user.id, activityId, submission);
            setResult(quizResult);
            return quizResult;
        } catch (err) {
            const errorMessage = err instanceof Error ? err.message : 'Failed to submit quiz';
            setError(errorMessage);
            console.error('Error submitting quiz:', err);
            return null;
        } finally {
            setIsSubmitting(false);
        }
    }, [user?.id, quiz, userAnswers, allQuestionsAnswered, getDuration]);

    /**
     * Reset the quiz state
     */
    const reset = useCallback(() => {
        setQuiz(null);
        setUserAnswers(new Map());
        setError(null);
        setResult(null);
        setStartTime(null);
        setAccumulatedDuration(0);
        lastActivityIdRef.current = null;
    }, []);

    return {
        quiz,
        userAnswers,
        isLoading,
        error,
        result,
        isSubmitting,
        generate,
        selectAnswer,
        submit,
        reset,
        getDuration,
        allQuestionsAnswered: allQuestionsAnswered()
    };
}
