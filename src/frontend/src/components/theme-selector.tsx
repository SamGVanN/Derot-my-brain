import { useTheme } from "./theme-provider";
import { ThemeDropdown } from "./ThemeDropdown";

export function ThemeSelector() {
    const { theme, setTheme } = useTheme();

    const handleThemeChange = (themeName: string) => {
        setTheme(themeName); // Apply visual theme immediately (Local only)
    };

    return (
        <ThemeDropdown
            currentThemeName={theme.name}
            onThemeChange={handleThemeChange}
        />
    );
}
