import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { usePreferences } from '../hooks/usePreferences';

import type { User } from '../models/User';
import { GeneralPreferencesForm } from '@/components/preferences/GeneralPreferencesForm';
import { CategoryPreferencesForm } from '@/components/preferences/CategoryPreferencesForm';
import { LLMConfigurationForm } from '@/components/preferences/LLMConfigurationForm';
import { useAppConfig } from '@/hooks/useAppConfig';
import { Button } from '@/components/ui/button';
import { Separator } from '@/components/ui/separator';
import { ArrowLeft } from 'lucide-react';

interface UserPreferencesPageProps {
    user: User;
    onUserUpdated: (user: User) => void; // Kept for compatibility, though hook handles store update
    onCancel: () => void;
}

export default function UserPreferencesPage({ user, onCancel }: UserPreferencesPageProps) {
    const { t } = useTranslation();
    const { updateGenericPreferences } = usePreferences();
    const { config: appConfig, loading: configLoading, updateLLMConfig, testLLMConnection } = useAppConfig();
    const [isSaving, setIsSaving] = useState(false);

    // Ensure we have preferences object even if partial
    const preferences = user.preferences || {
        questionCount: 10,
        preferredTheme: 'derot-brain',
        language: 'en',
        selectedCategories: []
    };

    const handleGeneralSave = async (prefs: any) => {
        setIsSaving(true);
        try {
            await updateGenericPreferences(prefs);
            // Optional: Success toast
        } catch (error) {
            console.error(error);
            // Optional: Error toast
        } finally {
            setIsSaving(false);
        }
    };

    const handleCategoriesSave = async (categories: string[]) => {
        setIsSaving(true);
        try {
            await updateGenericPreferences({ selectedCategories: categories });
        } catch (error) {
            console.error(error);
        } finally {
            setIsSaving(false);
        }
    };

    return (
        <div className="container max-w-4xl mx-auto py-8 px-4 space-y-8 animate-in fade-in duration-500">
            {/* Header with Back Button */}
            <div className="flex items-center gap-4">
                <Button variant="ghost" size="icon" onClick={onCancel}>
                    <ArrowLeft className="h-5 w-5" />
                </Button>
                <div>
                    <h1 className="text-3xl font-bold tracking-tight">{t('preferences.title')}</h1>
                    <p className="text-muted-foreground">
                        {t('preferences.subtitle', 'Manage your application settings and interests.')}
                    </p>
                </div>
            </div>

            <Separator />

            {/* About / Help Section - Now at the top */}
            <div className="rounded-lg bg-blue-50 dark:bg-blue-950/30 border border-blue-200 dark:border-blue-900/50 p-4">
                <h3 className="font-semibold text-blue-900 dark:text-blue-100 flex items-center gap-2 mb-1">
                    {t('preferences.about.title', 'About Settings')}
                </h3>
                <p className="text-sm text-blue-700 dark:text-blue-300">
                    {t('preferences.about.description', 'Customize your experience. These settings will be automatically loaded every time you log in.')}
                </p>
            </div>

            <div className="space-y-8">
                {/* General Settings Section */}
                <section>
                    <h2 className="text-lg font-semibold mb-4">{t('preferences.section.general', 'General Settings')}</h2>
                    <GeneralPreferencesForm
                        preferences={preferences}
                        onSave={handleGeneralSave}
                        isSaving={isSaving}
                    />
                </section>

                {/* Categories Section */}
                <section>
                    <h2 className="text-lg font-semibold mb-4">{t('preferences.section.interests', 'Interests & Topics')}</h2>
                    <CategoryPreferencesForm
                        selectedCategories={preferences.selectedCategories || []}
                        onSave={handleCategoriesSave}
                        isSaving={isSaving}
                    />
                </section>

                <Separator />

                {/* LLM Configuration Section (Global) */}
                <section>
                    <h2 className="text-lg font-semibold mb-4 flex items-center gap-2">
                        {t('preferences.section.ai', 'AI Engine Configuration')}
                        <span className="text-xs font-normal text-muted-foreground bg-secondary px-2 py-0.5 rounded-full">
                            {t('common.global', 'Global/System')}
                        </span>
                    </h2>
                    <LLMConfigurationForm
                        config={appConfig}
                        onSave={updateLLMConfig}
                        onTestConnection={testLLMConnection}
                        isLoading={configLoading}
                    />
                </section>
            </div>
        </div>
    );
}
