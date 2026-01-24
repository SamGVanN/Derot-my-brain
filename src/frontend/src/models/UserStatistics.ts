export interface UserStatisticsDto {
    lastActivity?: {
        date: string;
        title: string;
        activityId: string;
    };
    totalActivities: number;
    bestScore?: {
        score: number;
        totalQuestions: number;
        percentage: number;
        title: string;
        activityId: string;
        date: string;
    };
    activityCalendar: ActivityCalendarDto[];
}

export interface ActivityCalendarDto {
    date: string;
    count: number;
}

export interface TopScoreDto {
    score: number;
    totalQuestions: number;
    percentage: number;
    title: string;
    activityId: string;
    date: string;
}

export interface TrackedTopicDto {
    title: string;
    wikipediaUrl: string;
    addedAt: string;
    lastActivityAt?: string;
    bestScore?: number;
    bestScoreDate?: string;
    totalQuestions?: number;
    totalSessions: number;
}

export interface TrackTopicDto {
    title: string;
    wikipediaUrl: string;
}
