import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Separator } from '@/components/ui/separator';
import { Wifi, WifiOff, Loader2, Server, CheckCircle2 } from 'lucide-react';
import type { AppConfiguration, LLMConfiguration } from '@/models/Configuration';

interface LLMConfigurationFormProps {
    config: AppConfiguration | null;
    onSave: (llmConfig: LLMConfiguration) => Promise<boolean>;
    onTestConnection: (llmConfig: LLMConfiguration) => Promise<{ success: boolean; message: string }>;
    isLoading: boolean;
}

export function LLMConfigurationForm({ config, onSave, onTestConnection, isLoading }: LLMConfigurationFormProps) {
    const { t } = useTranslation();

    // Local state for form fields
    const [localConfig, setLocalConfig] = useState<LLMConfiguration>({
        url: 'http://localhost:11434',
        port: 11434,
        provider: 'ollama',
        defaultModel: 'llama3:8b',
        timeoutSeconds: 30
    });

    const [hasChanges, setHasChanges] = useState(false);
    const [isSaving, setIsSaving] = useState(false);
    const [isTesting, setIsTesting] = useState(false);
    const [testResult, setTestResult] = useState<{ success: boolean; message: string } | null>(null);

    // Sync from props
    useEffect(() => {
        if (config?.llm) {
            setLocalConfig(config.llm);
            setHasChanges(false);
        }
    }, [config]);

    const handleChange = (key: keyof LLMConfiguration, value: string | number) => {
        setLocalConfig(prev => {
            const next = { ...prev, [key]: value };

            // Check changes against original config
            if (config?.llm) {
                const isDifferent =
                    next.url !== config.llm.url ||
                    next.port !== config.llm.port ||
                    next.provider !== config.llm.provider ||
                    next.defaultModel !== config.llm.defaultModel ||
                    next.timeoutSeconds !== config.llm.timeoutSeconds;
                setHasChanges(isDifferent);
            }

            return next;
        });
        // Clear previous test result on change
        setTestResult(null);
    };

    const handleTest = async () => {
        setIsTesting(true);
        setTestResult(null);
        try {
            const result = await onTestConnection(localConfig);
            setTestResult(result);
        } catch (error) {
            setTestResult({ success: false, message: 'Connection test failed unexpectedly' });
        } finally {
            setIsTesting(false);
        }
    };

    const handleSave = async () => {
        setIsSaving(true);
        try {
            const success = await onSave(localConfig);
            if (success) {
                setHasChanges(false);
                // Optionally re-test connection on save implies success
            }
        } finally {
            setIsSaving(false);
        }
    };

    const handleCancel = () => {
        if (config?.llm) {
            setLocalConfig(config.llm);
            setHasChanges(false);
            setTestResult(null);
        }
    };

    if (isLoading && !config) {
        return (
            <Card>
                <CardContent className="p-6 flex justify-center">
                    <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
                </CardContent>
            </Card>
        );
    }

    return (
        <Card className="border-blue-200 dark:border-blue-900 shadow-sm">
            <CardHeader className="bg-blue-50/50 dark:bg-blue-950/10 pb-4 border-b border-blue-100 dark:border-blue-900/50">
                <CardTitle className="text-xl flex items-center gap-2 text-blue-900 dark:text-blue-100">
                    <Server className="h-5 w-5" />
                    {t('config.llm.title', 'AI Engine Configuration')}
                </CardTitle>
                <CardDescription className="text-blue-700/80 dark:text-blue-300/80">
                    {t('config.llm.description', 'Configure the local AI model (LLM) used for generating quizzes.')}
                </CardDescription>
            </CardHeader>
            <CardContent className="space-y-6 pt-6">

                {/* Connection Status Alert */}
                {testResult && (
                    <div className={`p-4 rounded-md border flex items-start gap-3 ${testResult.success
                        ? "border-green-200 bg-green-50 dark:border-green-900 dark:bg-green-900/20 text-green-900 dark:text-green-100"
                        : "border-red-200 bg-red-50 dark:border-red-900 dark:bg-red-900/20 text-red-900 dark:text-red-100"
                        }`}>
                        {testResult.success ? <CheckCircle2 className="h-5 w-5 mt-0.5" /> : <WifiOff className="h-5 w-5 mt-0.5" />}
                        <div className="space-y-1">
                            <h5 className="font-medium leading-none tracking-tight">
                                {testResult.success ? t('common.success', 'Connected') : t('common.error', 'Connection Failed')}
                            </h5>
                            <div className="text-sm opacity-90">
                                {testResult.message}
                            </div>
                        </div>
                    </div>
                )}

                {/* API Key Warning for OpenAI Compatible - Hidden if successful test */}
                {localConfig.provider === 'openai' && !testResult?.success && (
                    <div className="rounded-lg bg-yellow-50 dark:bg-yellow-950/30 border border-yellow-200 dark:border-yellow-900/50 p-4">
                        <p className="text-sm text-yellow-800 dark:text-yellow-200">
                            {t('configuration.ai.valid_api_key_required')}
                        </p>
                    </div>
                )}

                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    {/* Provider & Model */}
                    <div className="space-y-4">
                        <div className="space-y-2">
                            <Label>{t('config.llm.provider', 'Provider')}</Label>
                            <Select
                                value={localConfig.provider}
                                onValueChange={(val) => handleChange('provider', val)}
                            >
                                <SelectTrigger>
                                    <SelectValue placeholder="Select provider" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="ollama">Ollama (Local)</SelectItem>
                                    <SelectItem value="anythingllm">AnythingLLM</SelectItem>
                                    <SelectItem value="openai">OpenAI Compatible</SelectItem>
                                </SelectContent>
                            </Select>
                        </div>

                        <div className="space-y-2">
                            <Label>{t('config.llm.model', 'Default Model')}</Label>
                            <Input
                                value={localConfig.defaultModel}
                                onChange={(e) => handleChange('defaultModel', e.target.value)}
                                placeholder="e.g. llama3:8b, mistral"
                            />
                            <p className="text-xs text-muted-foreground">
                                {t('config.llm.modelHelp', 'Must match a model installed in your provider.')}
                            </p>
                        </div>
                    </div>

                    {/* Connection Details */}
                    <div className="space-y-4">
                        <div className="grid grid-cols-3 gap-4">
                            <div className="col-span-2 space-y-2">
                                <Label>{t('config.llm.url', 'Server URL')}</Label>
                                <Input
                                    value={localConfig.url}
                                    onChange={(e) => handleChange('url', e.target.value)}
                                    placeholder="http://localhost"
                                />
                            </div>
                            <div className="space-y-2">
                                <Label>{t('config.llm.port', 'Port')}</Label>
                                <Input
                                    type="number"
                                    value={localConfig.port}
                                    onChange={(e) => handleChange('port', parseInt(e.target.value))}
                                />
                            </div>
                        </div>

                        <div className="space-y-2">
                            <Label>{t('config.llm.timeout', 'Timeout (seconds)')}</Label>
                            <Input
                                type="number"
                                value={localConfig.timeoutSeconds}
                                onChange={(e) => handleChange('timeoutSeconds', parseInt(e.target.value))}
                            />
                        </div>
                    </div>
                </div>

                <Separator />

                <div className="flex justify-between items-center">
                    <Button
                        variant="outline"
                        onClick={handleTest}
                        disabled={isTesting || !localConfig.url}
                        className="gap-2"
                    >
                        {isTesting ? <Loader2 className="h-4 w-4 animate-spin" /> : <Wifi className="h-4 w-4" />}
                        {t('config.llm.testConnection', 'Test Connection')}
                    </Button>

                    <div className="flex gap-2">
                        {hasChanges && (
                            <Button variant="ghost" onClick={handleCancel} disabled={isSaving}>
                                {t('common.cancel', 'Cancel')}
                            </Button>
                        )}
                        <Button onClick={handleSave} disabled={!hasChanges || isSaving}>
                            {isSaving && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                            {t('common.save', 'Save Configuration')}
                        </Button>
                    </div>
                </div>
            </CardContent>
        </Card>
    );
}
