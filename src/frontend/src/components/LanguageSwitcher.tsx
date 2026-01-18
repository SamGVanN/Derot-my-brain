import { useTranslation } from 'react-i18next';
import { LanguageDropdown } from "./LanguageDropdown";

export function LanguageSwitcher() {
    const { i18n } = useTranslation();

    const changeLanguage = (code: string) => {
        i18n.changeLanguage(code);
    };

    // Handle 'en-US' vs 'en'
    const languageCode = i18n.language.split('-')[0];

    return (
        <LanguageDropdown
            currentLanguageCode={languageCode}
            onLanguageChange={changeLanguage}
        />
    );
}
