export interface UserActivity {
    id: string;
    userId: string;
    activityType: string;
    description: string;
    timestamp: string;
    score?: number;
}
