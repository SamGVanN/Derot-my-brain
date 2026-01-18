import { useState, useRef, useEffect } from "react";
import { useTranslation } from 'react-i18next';
import { Globe, Check, ChevronDown } from "lucide-react";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

const languages = [
    { code: 'en', label: 'English' },
    { code: 'fr', label: 'Français' }
];

export function LanguageSwitcher() {
    const { i18n } = useTranslation();
    const [isOpen, setIsOpen] = useState(false);
    const dropdownRef = useRef<HTMLDivElement>(null);

    const currentLanguage = languages.find(l => l.code === i18n.language.split('-')[0]) || languages[0];

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

    const changeLanguage = (code: string) => {
        i18n.changeLanguage(code);
        setIsOpen(false);
    };

    return (
        <div className="relative inline-block text-left" ref={dropdownRef}>
            <Button
                variant="outline"
                size="sm"
                onClick={toggleOpen}
                className="flex items-center gap-2 h-9 border-border/60 bg-background/50 backdrop-blur-sm"
            >
                <Globe className="h-4 w-4 text-primary" />
                <span className="hidden sm:inline-block font-medium">{currentLanguage.label}</span>
                <ChevronDown className={cn("h-3 w-3 text-muted-foreground transition-transform duration-200", isOpen && "rotate-180")} />
            </Button>

            {isOpen && (
                <div className="absolute right-0 mt-2 w-40 origin-top-right rounded-md border border-border bg-popover p-1 shadow-md ring-1 ring-black ring-opacity-5 focus:outline-none animate-in fade-in zoom-in-95 duration-100 z-50">
                    <div className="space-y-1">
                        {languages.map((lang) => {
                            const isSelected = i18n.language.split('-')[0] === lang.code;
                            return (
                                <button
                                    key={lang.code}
                                    onClick={() => changeLanguage(lang.code)}
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
