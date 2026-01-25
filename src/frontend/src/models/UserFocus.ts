import type { SourceType } from './Enums';

export interface UserFocus {
    id: string;
    userId: string;

    // Identity
    sourceHash: string;
    sourceId: string;
    sourceType: SourceType;
    displayTitle: string;

    // Stats
    bestScore: number;
    lastScore: number;
    lastAttemptDate: string;

    totalReadTimeSeconds: number;
    totalQuizTimeSeconds: number;
    totalStudyTimeSeconds: number;

    isPinned: boolean;
    isArchived: boolean;
}

export interface TrackTopicRequest {
    sourceId: string;
    sourceType: SourceType;
    displayTitle: string;
}
