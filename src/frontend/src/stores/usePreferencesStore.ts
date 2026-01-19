import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { defaultTheme } from '@/lib/themes';

interface PreferencesState {
    theme: string;
    language: string;
    hasSeenWelcome: boolean;
    sessionWelcomeDismissed: boolean;

    // Actions
    setTheme: (theme: string) => void;
    setLanguage: (language: string) => void;
    setHasSeenWelcome: (hasSeen: boolean) => void;
    setSessionWelcomeDismissed: (dismissed: boolean) => void;

    // Bulk update (e.g. on login)
    setPreferences: (prefs: { preferredTheme?: string; language?: string }) => void;
}

export const usePreferencesStore = create<PreferencesState>()(
    persist(
        (set) => ({
            theme: defaultTheme.name,
            language: 'en', // Default, but i18n detection might override this initially if we are not careful.
            hasSeenWelcome: false,
            // Initialize from sessionStorage to persist across reloads in same tab
            sessionWelcomeDismissed: typeof sessionStorage !== 'undefined'
                ? sessionStorage.getItem('welcome_dismissed') === 'true'
                : false,

            setTheme: (theme) => set({ theme }),
            setLanguage: (language) => set({ language }),
            setHasSeenWelcome: (hasSeen) => set({ hasSeenWelcome: hasSeen }),
            setSessionWelcomeDismissed: (dismissed) => set({ sessionWelcomeDismissed: dismissed }),

            setPreferences: (prefs) => set((state) => ({
                theme: prefs.preferredTheme || state.theme,
                language: prefs.language !== 'auto' ? (prefs.language || state.language) : state.language
            })),
        }),
        {
            name: 'preferences-storage',
            // Only persist persistent preferences, not session state
            partialize: (state) => ({
                theme: state.theme,
                language: state.language,
                hasSeenWelcome: state.hasSeenWelcome
            }),
        }
    )
);
