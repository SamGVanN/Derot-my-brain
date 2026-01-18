import React, { createContext, useContext, useEffect, useState } from "react";
import { type Theme, themes, defaultTheme } from "@/lib/themes";

type ThemeContextType = {
    theme: Theme;
    setTheme: (themeName: string) => void;
};

const ThemeContext = createContext<ThemeContextType | undefined>(undefined);

export function ThemeProvider({ children }: { children: React.ReactNode }) {
    const [currentTheme, setCurrentTheme] = useState<Theme>(defaultTheme);

    // Initialize from localStorage or default
    useEffect(() => {
        const savedThemeName = localStorage.getItem("theme");
        if (savedThemeName && themes[savedThemeName]) {
            setCurrentTheme(themes[savedThemeName]);
        }
    }, []);

    const setTheme = (themeName: string) => {
        if (themes[themeName]) {
            const newTheme = themes[themeName];
            setCurrentTheme(newTheme);
            localStorage.setItem("theme", themeName);
        }
    };

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
            // Special handling if we had non-string values, but now all are strings
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
