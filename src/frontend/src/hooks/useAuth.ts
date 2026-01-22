import { useCallback } from 'react';
import { useAuthStore } from '../stores/useAuthStore';
import { userApi } from '../api/userApi';
import { usePreferencesStore } from '../stores/usePreferencesStore';

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

    const login = useCallback((userToLogin: any) => {
        storeLogin(userToLogin);
        // Sync preferences store with user's saved preferences on login
        if (userToLogin.preferences) {
            setPreferences(userToLogin.preferences);
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
            // Updating the user will create a new object reference, 
            // but userId (primitive) remains unstable if we don't depend on it specifically.
            // By depending on userId primitive, we avoid the loop.
            updateUser(validUser);

            // Also sync preferences
            if (validUser.preferences) {
                setPreferences(validUser.preferences);
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
