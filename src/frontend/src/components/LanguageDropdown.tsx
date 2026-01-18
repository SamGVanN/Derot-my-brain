import { useState, useRef, useEffect } from "react";
import { Globe, Check, ChevronDown } from "lucide-react";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

interface Language {
    code: string;
    label: string;
}

const languages: Language[] = [
    { code: 'en', label: 'English' },
    { code: 'fr', label: 'Français' }
];

interface LanguageDropdownProps {
    currentLanguageCode: string;
    onLanguageChange: (code: string) => void;
    className?: string; // Allow custom styling for the trigger button
}

export function LanguageDropdown({ currentLanguageCode, onLanguageChange, className }: LanguageDropdownProps) {
    const [isOpen, setIsOpen] = useState(false);
    const dropdownRef = useRef<HTMLDivElement>(null);

    const currentLanguage = languages.find(l => l.code === currentLanguageCode) || languages[0];

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

    const handleSelect = (code: string) => {
        onLanguageChange(code);
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
                    <Globe className="h-4 w-4 text-primary group-hover:text-accent-foreground transition-colors" />
                    <span className="font-medium">
                        {currentLanguage.label}
                    </span>
                </div>
                <ChevronDown className={cn("h-3 w-3 text-muted-foreground transition-transform duration-200", isOpen && "rotate-180")} />
            </Button>

            {isOpen && (
                <div className="absolute right-0 mt-2 w-40 origin-top-right rounded-md border border-border bg-popover p-1 shadow-md ring-1 ring-black ring-opacity-5 focus:outline-none animate-in fade-in zoom-in-95 duration-100 z-50">
                    <div className="space-y-1">
                        {languages.map((lang) => {
                            const isSelected = currentLanguageCode === lang.code;
                            return (
                                <button
                                    key={lang.code}
                                    onClick={() => handleSelect(lang.code)}
                                    className={cn(
                                        "w-full flex items-center justify-between rounded-sm px-3 py-2 text-sm transition-colors",
                                        isSelected
                                            ? "bg-accent text-accent-foreground font-medium"
                                            : "text-popover-foreground hover:bg-muted hover:text-foreground"
                                    )}
                                >
                                    <span className="font-medium mr-4">{lang.label}</span>
                                    {isSelected && <Check className="h-4 w-4 text-primary" />}
                                </button>
                            );
                        })}
                    </div>
                </div>
            )}
        </div>
    );
}

// Export languages for use elsewhere if needed, though they are standard
export { languages };
