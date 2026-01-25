export interface UserStatisticsDto {
    totalActivities: number;
    totalQuizzes: number;
    totalReads: number;
    userFocusCount: number;
    lastActivity?: LastActivityDto;
    bestScore?: BestScoreDto;
}

export interface LastActivityDto {
    activityId: string;
    title: string;
    date: string;
    type: string;
}

export interface BestScoreDto {
    activityId: string;
    title: string;
    score: number;
    questionCount: number;
    percentage: number;
    date: string;
}

export interface ActivityCalendarDto {
    date: string;
    count: number;
}

export interface TopScoreDto {
    activityId: string;
    title: string;
    score: number;
    questionCount: number;
    percentage: number;
    date: string;
}
