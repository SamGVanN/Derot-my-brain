import type { SourceType, ActivityType } from './Enums';

export interface UserActivity {
    id: string;
    userId: string;
    title: string;
    description: string;

    // Content Identity
    sourceId: string;
    sourceType: SourceType;
    sourceHash: string;

    type: ActivityType;

    // Timing
    sessionDateStart: string;
    sessionDateEnd?: string;

    // Durations
    exploreDurationSeconds?: number;
    readDurationSeconds?: number;
    quizDurationSeconds?: number;
    totalDurationSeconds: number;

    // Stats
    score: number;
    questionCount: number;
    scorePercentage?: number;
    isNewBestScore: boolean;
    isBaseline: boolean;
    isCompleted: boolean;

    // LLM Info
    llmModelName?: string;
    llmVersion?: string;

    isTracked: boolean;
    articleContent?: string;
    payload?: string;
}
