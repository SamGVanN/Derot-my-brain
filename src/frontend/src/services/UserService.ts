import type { User } from '../models/User';
import type { UserActivity } from '../models/UserActivity';
import { userApi } from '../api/userApi';

/**
 * @deprecated Use userApi directly instead. This service is kept for backward compatibility to ease migration.
 */
export const UserService = {
    getAllUsers: (): Promise<User[]> => userApi.getAllUsers(),

    createOrSelectUser: (name: string): Promise<User> => userApi.createOrSelectUser(name),

    getUserById: (id: string): Promise<User> => userApi.getUserById(id),

    getHistory: (userId: string): Promise<UserActivity[]> => userApi.getHistory(userId),

    addActivity: (userId: string, activity: Partial<UserActivity>): Promise<UserActivity> =>
        userApi.addActivity(userId, activity),

    updatePreferences: (userId: string, preferences: Partial<User['preferences']>): Promise<User> =>
        userApi.updatePreferences(userId, preferences),

    getPreferences: (userId: string): Promise<User['preferences']> => userApi.getPreferences(userId),

    updateGeneralPreferences: (userId: string, preferences: {
        language: string;
        preferredTheme: string;
        questionCount: number;
    }): Promise<User> => userApi.updateGeneralPreferences(userId, preferences),

    updateCategoryPreferences: (userId: string, selectedCategories: string[]): Promise<User> =>
        userApi.updateCategoryPreferences(userId, selectedCategories)
};
