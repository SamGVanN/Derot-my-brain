import { client } from './client';
import type { Quiz, QuizSubmission, QuizResult } from '@/models/Quiz';

/**
 * Generate a quiz for a specific activity
 * @param userId - The user ID
 * @param activityId - The activity ID (from Read activity)
 * @returns Quiz with questions
 */
export async function generateQuiz(userId: string, activityId: string): Promise<Quiz> {
    const response = await client.post<Quiz>(
        `/users/${userId}/activities/${activityId}/quiz`
    );
    return response.data;
}

/**
 * Submit quiz answers and get evaluation results
 * @param userId - The user ID
 * @param activityId - The activity ID
 * @param submission - Quiz answers and duration
 * @returns Quiz evaluation results with score and feedback
 */
export async function submitQuiz(
    userId: string,
    activityId: string,
    submission: QuizSubmission
): Promise<QuizResult> {
    const response = await client.post<QuizResult>(
        `/users/${userId}/activities/${activityId}/quiz/submit`,
        submission
    );
    return response.data;
}
