import { useCallback } from 'react';
import { usePreferencesStore } from '../stores/usePreferencesStore';
import { useAuthStore } from '../stores/useAuthStore';
import { userApi } from '../api/userApi';

/**
 * Custom hook for User Preferences.
 * Handles both local state (Zustand) and backend persistence.
 */
export function usePreferences() {
    const {
        theme,
        language,
        hasSeenWelcome,
        sessionWelcomeDismissed,
        setTheme: setStoreTheme,
        setLanguage: setStoreLanguage,
        setHasSeenWelcome: setStoreHasSeenWelcome,
        setSessionWelcomeDismissed: setStoreSessionWelcomeDismissed,
        setPreferences: setStorePreferences
    } = usePreferencesStore();

    const { user, updateUser } = useAuthStore();

    // Update Theme: Optimistic local update + Backend persist
    const updateTheme = useCallback(async (newTheme: string) => {
        setStoreTheme(newTheme);

        if (user?.id) {
            try {
                // Using generic updatePreferences which accepts Partial
                await userApi.updatePreferences(user.id, { preferredTheme: newTheme });

                // Keep auth store user in sync
                updateUser({
                    ...user,
                    preferences: {
                        ...user.preferences,
                        preferredTheme: newTheme
                    }
                });
            } catch (error) {
                console.error('Failed to persist theme preference:', error);
                // In a perfect world, we would rollback here on error
            }
        }
    }, [user, updateUser, setStoreTheme]);

    // Update Language: Optimistic local update + Backend persist
    const updateLanguage = useCallback(async (newLanguage: string) => {
        setStoreLanguage(newLanguage);

        if (user?.id) {
            try {
                await userApi.updatePreferences(user.id, { language: newLanguage } as any);

                // Keep auth store user in sync
                // Note: casting to any above because 'language' might not be strictly typed in UserPreferences yet depending on backend model
                updateUser({
                    ...user,
                    preferences: {
                        ...user.preferences,
                        language: newLanguage
                    } as any
                });
            } catch (error) {
                console.error('Failed to persist language preference:', error);
            }
        }
    }, [user, updateUser, setStoreLanguage]);

    // Mark welcome as seen (Local only per specs)
    const setHasSeenWelcome = useCallback((hasSeen: boolean) => {
        setStoreHasSeenWelcome(hasSeen);
    }, [setStoreHasSeenWelcome]);

    // Dismiss welcome for this session
    const dismissWelcomeSession = useCallback(() => {
        setStoreSessionWelcomeDismissed(true);
        if (typeof sessionStorage !== 'undefined') {
            sessionStorage.setItem('welcome_dismissed', 'true');
        }
    }, [setStoreSessionWelcomeDismissed]);

    // Generic update for other preferences (like question count)
    const updateGenericPreferences = useCallback(async (prefs: any) => {
        // Update local store if applicable (store handles partial updates)
        setStorePreferences(prefs);

        if (user?.id) {
            try {
                await userApi.updatePreferences(user.id, prefs);
                updateUser({
                    ...user,
                    preferences: {
                        ...user.preferences,
                        ...prefs
                    }
                });
            } catch (e) {
                console.error('Failed to persist preferences:', e);
                throw e;
            }
        }
    }, [user, updateUser, setStorePreferences]);

    return {
        theme,
        language,
        hasSeenWelcome,
        sessionWelcomeDismissed,
        setHasSeenWelcome,
        dismissWelcomeSession,
        updateTheme,
        updateLanguage,
        updateGenericPreferences
    };
}
