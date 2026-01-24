export interface UserActivity {
    id: string;
    userId: string;
    type: string;
    title: string;
    wikipediaUrl: string;
    sessionDate: string;
    score?: number;
    totalQuestions?: number;
    llmModelName?: string;
    llmVersion?: string;
    isTracked: boolean;
}
