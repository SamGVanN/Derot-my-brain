import { useState, useRef, useEffect } from "react";
import { themes } from "@/lib/themes";
import { Palette, Check, ChevronDown } from "lucide-react";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

interface ThemeDropdownProps {
    currentThemeName: string;
    onThemeChange: (themeName: string) => void;
    className?: string; // Allow custom styling for the trigger button
}

export function ThemeDropdown({ currentThemeName, onThemeChange, className }: ThemeDropdownProps) {
    const [isOpen, setIsOpen] = useState(false);
    const dropdownRef = useRef<HTMLDivElement>(null);
    const currentTheme = themes[currentThemeName];

    // Close dropdown when clicking outside
    useEffect(() => {
        function handleClickOutside(event: MouseEvent) {
            if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
                setIsOpen(false);
            }
        }
        document.addEventListener("mousedown", handleClickOutside);
        return () => {
            document.removeEventListener("mousedown", handleClickOutside);
        };
    }, []);

    const toggleOpen = () => setIsOpen(!isOpen);

    const handleSelect = (name: string) => {
        onThemeChange(name);
        setIsOpen(false);
    };

    return (
        <div className="relative inline-block text-left" ref={dropdownRef}>
            <Button
                variant="outline"
                size="sm"
                onClick={toggleOpen}
                className={cn(
                    "flex items-center justify-between gap-2 h-9 border-border/60 bg-background/50 backdrop-blur-sm group px-3",
                    className
                )}
            >
                <div className="flex items-center gap-2">
                    <Palette className="h-4 w-4 text-primary group-hover:text-accent-foreground transition-colors" />
                    <span className="font-medium">
                        {currentTheme?.label || 'Theme'}
                    </span>
                </div>
                <ChevronDown className={cn("h-3 w-3 text-muted-foreground transition-transform duration-200", isOpen && "rotate-180")} />
            </Button>

            {isOpen && (
                <div className="absolute right-0 mt-2 w-64 origin-top-right rounded-md border border-border bg-popover p-1 shadow-md ring-1 ring-black ring-opacity-5 focus:outline-none animate-in fade-in zoom-in-95 duration-100 z-50">
                    <div className="space-y-1">
                        {Object.values(themes).map((t) => {
                            const isSelected = currentThemeName === t.name;
                            return (
                                <button
                                    key={t.name}
                                    onClick={() => handleSelect(t.name)}
                                    className={cn(
                                        "w-full flex items-center justify-between rounded-sm px-3 py-2.5 text-sm transition-colors",
                                        isSelected
                                            ? "bg-accent text-accent-foreground font-medium"
                                            : "text-popover-foreground hover:bg-muted hover:text-foreground"
                                    )}
                                >
                                    <span className="font-medium mr-4">{t.label}</span>
                                    <div className="flex items-center gap-3">
                                        <div className="flex items-center gap-1 bg-muted/40 p-1 rounded-md border border-border/40">
                                            {/* Color Preview Blocks */}
                                            <div className="h-3 w-3 rounded-full shadow-sm ring-1 ring-inset ring-black/10 dark:ring-white/10" style={{ backgroundColor: t.colors.background }} title="Background" />
                                            <div className="h-3 w-3 rounded-full shadow-sm ring-1 ring-inset ring-black/10 dark:ring-white/10" style={{ backgroundColor: t.colors.primary }} title="Primary" />
                                            <div className="h-3 w-3 rounded-full shadow-sm ring-1 ring-inset ring-black/10 dark:ring-white/10" style={{ backgroundColor: t.colors.secondary }} title="Secondary" />
                                            <div className="h-3 w-3 rounded-full shadow-sm ring-1 ring-inset ring-black/10 dark:ring-white/10" style={{ backgroundColor: t.colors.accent }} title="Accent" />
                                        </div>
                                        {isSelected && <Check className="h-4 w-4 text-primary" />}
                                    </div>
                                </button>
                            );
                        })}
                    </div>
                </div>
            )}
        </div>
    );
}
