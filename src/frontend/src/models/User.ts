export interface User {
    id: string;
    name: string;
    createdAt: string;
    lastConnectionAt?: string;
    preferences: UserPreferences;
}

export interface UserPreferences {
    questionCount: number;
    preferredTheme: string;
    language: string;
    selectedCategories: string[];
}


export interface CreateUserRequest {
    name: string;
}
