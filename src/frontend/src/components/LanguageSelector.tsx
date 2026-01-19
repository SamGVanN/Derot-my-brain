import { useTranslation } from 'react-i18next';
import { Languages } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { usePreferences } from '@/hooks/usePreferences';

const languages = [
    { code: 'en', name: 'English', flag: '🇬🇧' },
    { code: 'fr', name: 'Français', flag: '🇫🇷' },
];

export function LanguageSelector() {
    const { i18n } = useTranslation();
    const { updateLanguage } = usePreferences();

    const currentLanguage = languages.find(lang => lang.code === i18n.language) || languages[0];

    const handleLanguageChange = async (languageCode: string) => {
        await i18n.changeLanguage(languageCode);
        await updateLanguage(languageCode);
    };

    return (
        <DropdownMenu>
            <DropdownMenuTrigger asChild>
                <Button variant="ghost" size="icon" title="Change language">
                    <Languages className="h-5 w-5" />
                </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
                {languages.map((lang) => (
                    <DropdownMenuItem
                        key={lang.code}
                        onClick={() => handleLanguageChange(lang.code)}
                        className={currentLanguage.code === lang.code ? 'bg-accent' : ''}
                    >
                        <span className="mr-2">{lang.flag}</span>
                        {lang.name}
                    </DropdownMenuItem>
                ))}
            </DropdownMenuContent>
        </DropdownMenu>
    );
}
