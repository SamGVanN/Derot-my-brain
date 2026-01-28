import { useCallback } from 'react';
import { useAuthStore } from '../stores/useAuthStore';
import { userApi } from '../api/userApi';
import { usePreferencesStore } from '../stores/usePreferencesStore';
import type { User } from '../models/User';


/**
 * Normalizes user preferences by converting category objects to IDs.
 * This ensures consistency with frontend UI state and prevents 400 errors
 * when sending preferences back to the backend.
 */
const normalizeUser = (user: User): User => {
    if (!user.preferences) return user;

    const prefs = { ...user.preferences };

    // Backend sends WikipediaCategory objects, but frontend expects IDs (strings)
    if (prefs.selectedCategories &&
        prefs.selectedCategories.length > 0 &&
        typeof prefs.selectedCategories[0] === 'object') {
        prefs.selectedCategories = (prefs.selectedCategories as any).map((c: any) => c.id);
    }

    return {
        ...user,
        preferences: prefs
    };
};

/**
 * Custom hook for authentication logic.
 * Wraps useAuthStore and adds session validation logic.
 */
export function useAuth() {
    const {
        user,
        isAuthenticated,
        isInitializing,
        setInitializing,
        login: storeLogin,
        logout: storeLogout,
        updateUser
    } = useAuthStore();

    const { setPreferences } = usePreferencesStore();
    const userId = user?.id;

    const login = useCallback((data: { user: User; token: string }) => {
        const normalizedUser = normalizeUser(data.user);
        storeLogin(normalizedUser, data.token);

        // Sync preferences store with user's saved preferences on login
        if (normalizedUser.preferences) {
            setPreferences(normalizedUser.preferences);
        }
    }, [storeLogin, setPreferences]);


    const logout = useCallback(() => {
        storeLogout();
        // Optional: Reset preferences to defaults or keep them? 
        // Usually keeping them is fine for UX, or we can reset.
    }, [storeLogout]);

    const validateSession = useCallback(async () => {
        if (!userId) {
            setInitializing(false);
            return;
        }

        try {
            // Validate if user still exists in backend
            const validUser = await userApi.getUserById(userId);
            const normalizedUser = normalizeUser(validUser);

            // Updating the user will create a new object reference
            updateUser(normalizedUser);

            // Also sync preferences
            if (normalizedUser.preferences) {
                setPreferences(normalizedUser.preferences);
            }
        } catch (error) {
            console.warn('Session validation failed (User presumably deleted):', error);
            logout(); // Invalid session
        } finally {
            setInitializing(false);
        }
    }, [userId, updateUser, logout, setInitializing, setPreferences]);

    return {
        user,
        isAuthenticated,
        isInitializing,
        login,
        logout,
        validateSession,
        updateUser
    };
}
