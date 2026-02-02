import { useTranslation } from 'react-i18next';
import { LLMConfigurationForm } from '@/components/preferences/LLMConfigurationForm';
import { useAppConfig } from '@/hooks/useAppConfig';
import { Button } from '@/components/ui/button';
import { Separator } from '@/components/ui/separator';
import { ArrowLeft } from 'lucide-react';
import type { User } from '../models/User';

interface LLMConfigurationPageProps {
    user: User; // Kept for consistency
    onUserUpdated: (user: User) => void;
    onCancel: () => void;
}

export default function LLMConfigurationPage({ onCancel }: LLMConfigurationPageProps) {
    const { t } = useTranslation();
    const { config: appConfig, loading: configLoading, updateLLMConfig, testLLMConnection, resetConfig } = useAppConfig();

    return (
        <div className="container max-w-4xl mx-auto py-8 px-4 space-y-8 animate-in fade-in duration-500">
            {/* Header with Back Button */}
            <div className="flex items-center gap-4">
                <Button variant="ghost" size="icon" onClick={onCancel}>
                    <ArrowLeft className="h-5 w-5" />
                </Button>
                <div>
                    <h1 className="text-3xl font-bold tracking-tight">{t('configuration.title', 'Configuration')}</h1>
                    <p className="text-muted-foreground">
                        {t('configuration.subtitle', 'Global configuration')}
                    </p>
                </div>
            </div>

            <Separator />

            <div className="space-y-8">
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
                        onReset={resetConfig}
                        onTestConnection={testLLMConnection}
                        isLoading={configLoading}
                    />
                </section>
            </div>
        </div>
    );
}
