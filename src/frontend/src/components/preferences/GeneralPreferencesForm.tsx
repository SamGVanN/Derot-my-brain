import { useTranslation } from 'react-i18next';
import { useTheme } from '@/components/theme-provider';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Label } from '@/components/ui/label';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
import { Separator } from '@/components/ui/separator';
import { Button } from '@/components/ui/button';
import { Loader2, Palette, Save, Languages, HelpCircle, AlertTriangle } from 'lucide-react';
import { useState, useEffect } from 'react';
import type { UserPreferences } from '@/models/User';
import { ThemeDropdown } from '@/components/ThemeDropdown';
import { LanguageDropdown, languages } from '@/components/LanguageDropdown';

interface GeneralPreferencesFormProps {
    preferences: UserPreferences;
    onSave: (prefs: Partial<UserPreferences>) => Promise<void>;
    isSaving: boolean;
}

export function GeneralPreferencesForm({ preferences, onSave, isSaving }: GeneralPreferencesFormProps) {
    const { t, i18n } = useTranslation();
    const { setTheme, theme } = useTheme();

    const [localPrefs, setLocalPrefs] = useState<{
        language: string;
        preferredTheme: string;
        questionCount: number;
    }>({
        language: preferences.language,
        preferredTheme: preferences.preferredTheme,
        questionCount: preferences.questionCount
    });

    const [hasChanges, setHasChanges] = useState(false);

    // Check if current visual theme differs from the form's selected theme (or saved one)
    // This happens if user changed theme via Header (temporary) but hasn't saved it here.
    const isThemeMismatch = theme && theme.name !== preferences.preferredTheme;

    // Check if current active language differs from form's elected language
    // We treat 'en-US' and 'en' as the same
    const currentLangCode = i18n.language.split('-')[0];
    const isLanguageMismatch = currentLangCode !== preferences.language;

    // Sync local state when props change
    useEffect(() => {
        setLocalPrefs({
            language: preferences.language,
            preferredTheme: preferences.preferredTheme,
            questionCount: preferences.questionCount
        });
        setHasChanges(false);
    }, [preferences]);

    const handleChange = (key: keyof typeof localPrefs, value: any) => {
        setLocalPrefs(prev => {
            const next = { ...prev, [key]: value };
            setHasChanges(
                next.language !== preferences.language ||
                next.preferredTheme !== preferences.preferredTheme ||
                next.questionCount !== preferences.questionCount
            );
            return next;
        });

        // Live preview for theme
        if (key === 'preferredTheme') {
            setTheme(value);
        }
        // Live preview for language
        if (key === 'language' && value !== 'auto') {
            i18n.changeLanguage(value);
        }
    };

    const handleSave = async () => {
        await onSave(localPrefs);
        setHasChanges(false);
    };

    const handleCancel = () => {
        // Revert live preview
        setTheme(preferences.preferredTheme);
        if (preferences.language !== 'auto') {
            i18n.changeLanguage(preferences.language);
        }
        // Reset state
        setLocalPrefs({
            language: preferences.language,
            preferredTheme: preferences.preferredTheme,
            questionCount: preferences.questionCount
        });
        setHasChanges(false);
    };

    return (
        <Card>
            <CardHeader className="pb-3">
                <CardTitle className="text-xl flex items-center gap-2">
                    <Save className="h-5 w-5 text-primary" />
                    {t('preferences.general.title', 'General Settings')}
                </CardTitle>
                <CardDescription>
                    {t('preferences.general.description', 'Customize your local experience.')}
                </CardDescription>
                <p className="text-xs text-muted-foreground mt-1">
                    {t('preferences.general.note', 'Note: Unsaved changes (applied immediately) are valid only for the current session.')}
                </p>
            </CardHeader>
            <CardContent className="space-y-6">
                {/* Language */}
                <div className="space-y-2">
                    <div className="flex items-center gap-2">
                        <Languages className="h-4 w-4 text-muted-foreground" />
                        <Label>{t('preferences.language.label', 'Interface Language')}</Label>
                    </div>
                    <LanguageDropdown
                        currentLanguageCode={localPrefs.language}
                        onLanguageChange={(code) => handleChange('language', code)}
                        className="w-full sm:w-[280px]"
                    />
                    {isLanguageMismatch && (
                        <div className="flex items-start gap-2 mt-2 text-sm text-amber-600 dark:text-amber-500 bg-amber-50 dark:bg-amber-950/30 p-2 rounded-md border border-amber-200 dark:border-amber-900/50 animate-in fade-in slide-in-from-top-1">
                            <AlertTriangle className="h-4 w-4 mt-0.5 shrink-0" />
                            <span>
                                {t('preferences.language.mismatch', 'The language currently active ({{langName}}) is different from your saved profile language.', { langName: languages.find(l => l.code === currentLangCode)?.label || currentLangCode })}
                            </span>
                        </div>
                    )}
                </div>

                <Separator />

                {/* Theme */}
                <div className="space-y-2">
                    <div className="flex items-center gap-2">
                        <Palette className="h-4 w-4 text-muted-foreground" />
                        <Label>{t('preferences.theme.label', 'Appearance Theme')}</Label>
                    </div>
                    <ThemeDropdown
                        currentThemeName={localPrefs.preferredTheme}
                        onThemeChange={(name) => handleChange('preferredTheme', name)}
                        className="w-full sm:w-[280px]"
                    />
                    {isThemeMismatch && (
                        <div className="flex items-start gap-2 mt-2 text-sm text-amber-600 dark:text-amber-500 bg-amber-50 dark:bg-amber-950/30 p-2 rounded-md border border-amber-200 dark:border-amber-900/50 animate-in fade-in slide-in-from-top-1">
                            <AlertTriangle className="h-4 w-4 mt-0.5 shrink-0" />
                            <span>
                                {t('preferences.theme.mismatch', 'The generic theme currently active ({{themeName}}) is different from your saved profile theme.', { themeName: theme?.label || theme?.name })}
                            </span>
                        </div>
                    )}
                </div>

                <Separator />

                {/* Question Count */}
                <div className="space-y-3">
                    <div className="flex items-center gap-2">
                        <HelpCircle className="h-4 w-4 text-muted-foreground" />
                        <Label>{t('preferences.quiz.questionCount', 'Questions per Quiz')}</Label>
                    </div>
                    <RadioGroup
                        value={localPrefs.questionCount.toString()}
                        onValueChange={(val) => handleChange('questionCount', parseInt(val))}
                        className="flex gap-4"
                    >
                        {[5, 10, 15, 20].map((count) => (
                            <div key={count} className="flex items-center space-x-2">
                                <RadioGroupItem value={count.toString()} id={`q-${count}`} />
                                <Label htmlFor={`q-${count}`}>{count}</Label>
                            </div>
                        ))}
                    </RadioGroup>
                </div>

                <div className="pt-4 flex justify-end gap-2">
                    {hasChanges && (
                        <Button variant="ghost" onClick={handleCancel} disabled={isSaving}>
                            {t('common.cancel', 'Cancel')}
                        </Button>
                    )}
                    <Button onClick={handleSave} disabled={!hasChanges || isSaving}>
                        {isSaving && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                        {t('common.save', 'Save Changes')}
                    </Button>
                </div>
            </CardContent>
        </Card>
    );
}
