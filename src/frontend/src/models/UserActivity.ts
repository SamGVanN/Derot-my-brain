import type { SourceType, ActivityType } from './Enums';

export interface UserActivity {
    id: string;
    userId: string;
    title: string;
    description: string;

    // Identity
    userSessionId: string;

    // Content Identity (denormalized for convenience in UI)
    sourceId: string; // GUID
    externalId: string; // URL or Doc GUID
    sourceType: SourceType;
    displayTitle?: string;

    type: ActivityType;

    // Timing
    sessionDateStart: string;
    sessionDateEnd?: string;

    // Durations
    durationSeconds: number;

    // Stats
    score: number;
    questionCount: number;
    scorePercentage?: number;
    isNewBestScore: boolean;
    isBaseline: boolean;
    isCurrentBest: boolean;
    isCompleted: boolean;

    // LLM Info
    llmModelName?: string;
    llmVersion?: string;

    isTracked: boolean;
    articleContent?: string;
    payload?: string;
    resultingReadActivityId?: string;
    resultingReadSourceName?: string;
    originExploreId?: string;
    backlogAddsCount?: number;
    refreshCount?: number;
}
