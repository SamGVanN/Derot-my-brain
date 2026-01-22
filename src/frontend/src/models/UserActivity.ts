export interface UserActivity {
    id: string;
    userId: string;
    type: string;
    topic: string;
    wikipediaUrl: string;
    sessionDate: string;
    score?: number;
    totalQuestions?: number;
    llmModelName?: string;
    llmVersion?: string;
    isTracked: boolean;
}
