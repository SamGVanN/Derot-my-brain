import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { usePreferences } from '../hooks/usePreferences';
import { GeneralPreferencesForm } from '@/components/preferences/GeneralPreferencesForm';
import { DerotZonePreferencesForm } from '@/components/preferences/DerotZonePreferencesForm';
import { Layout } from '@/components/Layout';
import type { User } from '../models/User';

interface PreferencesPageProps {
    user: User;
    onUserUpdated: (user: User) => void;
}

export function PreferencesPage({ user }: PreferencesPageProps) {
    const { t } = useTranslation();
    const { updateGenericPreferences } = usePreferences();
    const [isSaving, setIsSaving] = useState(false);

    // Ensure we have preferences object even if partial
    const preferences = user.preferences || {
        questionCount: 10,
        preferredTheme: 'derot-brain',
        language: 'en',
        selectedCategories: []
    };

    const handleSavePreferences = async (prefs: any) => {
        setIsSaving(true);
        try {
            await updateGenericPreferences(prefs);
        } catch (error) {
            console.error(error);
        } finally {
            setIsSaving(false);
        }
    };

    return (
        <Layout>
            <div className="container max-w-4xl mx-auto py-8 px-4 space-y-6 animate-in fade-in duration-500">
                {/* Header */}
                <div className="space-y-2">
                    <h1 className="text-3xl font-bold tracking-tight">{t('preferences.title')}</h1>
                    <p className="text-muted-foreground">{t('preferences.subtitle', 'Manage your application settings and interests.')}</p>
                </div>

                <div className="space-y-6">
                    {/* General Settings */}
                    <GeneralPreferencesForm
                        preferences={preferences}
                        onSave={handleSavePreferences}
                        isSaving={isSaving}
                    />

                    {/* Derot Zone Settings */}
                    <DerotZonePreferencesForm
                        preferences={preferences}
                        onSave={handleSavePreferences}
                        isSaving={isSaving}
                    />
                </div>
            </div>
        </Layout>
    );
}
