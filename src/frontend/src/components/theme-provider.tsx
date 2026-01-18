import React, { createContext, useContext, useEffect } from "react";
import { type Theme, themes, defaultTheme } from "@/lib/themes";
import { usePreferencesStore } from "@/stores/usePreferencesStore";

type ThemeContextType = {
    theme: Theme;
    setTheme: (themeName: string) => void;
};

// We keep the context for backward compatibility for now, 
// OR we can just use the store hook directly in components.
// But the requirement says "No direct localStorage access".
// This component is responsible for applying the theme to the DOM.

const ThemeContext = createContext<ThemeContextType | undefined>(undefined);

export function ThemeProvider({ children }: { children: React.ReactNode }) {
    const themeName = usePreferencesStore((state) => state.theme);
    const setTheme = usePreferencesStore((state) => state.setTheme);

    const currentTheme = themes[themeName] || defaultTheme;

    // Apply theme to DOM
    useEffect(() => {
        const root = document.documentElement;

        // Toggle 'dark' class based on theme type
        if (currentTheme.type === "dark") {
            root.classList.add("dark");
        } else {
            root.classList.remove("dark");
        }

        // Apply CSS variables
        Object.entries(currentTheme.colors).forEach(([key, value]) => {
            root.style.setProperty(`--${key}`, value);
        });
    }, [currentTheme]);

    return (
        <ThemeContext.Provider value={{ theme: currentTheme, setTheme }}>
            {children}
        </ThemeContext.Provider>
    );
}

export function useTheme() {
    const context = useContext(ThemeContext);
    if (context === undefined) {
        throw new Error("useTheme must be used within a ThemeProvider");
    }
    return context;
}

