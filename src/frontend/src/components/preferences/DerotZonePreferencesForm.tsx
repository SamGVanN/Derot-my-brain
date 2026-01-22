import { useTranslation } from 'react-i18next';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Label } from '@/components/ui/label';
import { Checkbox } from '@/components/ui/checkbox';
import { Button } from '@/components/ui/button';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
import { Separator } from '@/components/ui/separator';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Loader2, BrainCircuit, HelpCircle, Square, CheckSquare } from 'lucide-react';
import { useState, useEffect } from 'react';
import { useCategories } from '@/hooks/useCategories';
import type { UserPreferences } from '@/models/User';
import type { WikipediaCategory } from '@/models/SeedData';

interface DerotZonePreferencesFormProps {
    preferences: UserPreferences;
    onSave: (prefs: Partial<UserPreferences>) => Promise<void>;
    isSaving: boolean;
}

export function DerotZonePreferencesForm({ preferences, onSave, isSaving }: DerotZonePreferencesFormProps) {
    const { t, i18n } = useTranslation();
    const { categories, loading: loadingCategories, error: errorCategories } = useCategories();

    // Local state
    const [localPrefs, setLocalPrefs] = useState<{
        questionCount: number;
        selectedCategories: string[];
    }>({
        questionCount: preferences.questionCount,
        selectedCategories: preferences.selectedCategories || []
    });

    const [hasChanges, setHasChanges] = useState(false);

    // Sync from props
    useEffect(() => {
        setLocalPrefs({
            questionCount: preferences.questionCount,
            selectedCategories: preferences.selectedCategories || []
        });
        setHasChanges(false);
    }, [preferences]);

    // Handlers
    const handleQuestionCountChange = (val: string) => {
        const newVal = parseInt(val);
        setLocalPrefs(prev => {
            const next = { ...prev, questionCount: newVal };
            checkChanges(next);
            return next;
        });
    };

    const handleCategoryToggle = (categoryId: string, checked: boolean) => {
        setLocalPrefs(prev => {
            let nextCategories;
            if (checked) {
                nextCategories = [...prev.selectedCategories, categoryId];
            } else {
                nextCategories = prev.selectedCategories.filter(id => id !== categoryId);
            }
            const next = { ...prev, selectedCategories: nextCategories };
            checkChanges(next);
            return next;
        });
    };

    const handleSelectAllCategories = () => {
        const allIds = categories.map(c => c.id);
        setLocalPrefs(prev => {
            const next = { ...prev, selectedCategories: allIds };
            checkChanges(next);
            return next;
        });
    };

    const handleDeselectAllCategories = () => {
        setLocalPrefs(prev => {
            const next = { ...prev, selectedCategories: [] };
            checkChanges(next);
            return next;
        });
    };

    const checkChanges = (next: typeof localPrefs) => {
        const questionsChanged = next.questionCount !== preferences.questionCount;

        const initialCats = preferences.selectedCategories || [];
        const nextCats = next.selectedCategories;
        const categoriesChanged = nextCats.length !== initialCats.length ||
            !nextCats.every(id => initialCats.includes(id));

        setHasChanges(questionsChanged || categoriesChanged);
    };

    const handleSave = async () => {
        await onSave(localPrefs);
        setHasChanges(false);
    };

    const handleCancel = () => {
        setLocalPrefs({
            questionCount: preferences.questionCount,
            selectedCategories: preferences.selectedCategories || []
        });
        setHasChanges(false);
    };

    const currentLang = i18n.language.split('-')[0];

    return (
        <Card>
            <CardHeader className="pb-3">
                <CardTitle className="text-xl flex items-center gap-2">
                    <BrainCircuit className="h-5 w-5 text-primary" />
                    {t('preferences.section.derotZone', 'Derot Zone Settings')}
                </CardTitle>
                <CardDescription>
                    {t('preferences.derotZone.description', 'Configure your quiz experience and knowledge areas.')}
                </CardDescription>
            </CardHeader>

            <Separator className="mb-6" />

            <CardContent className="space-y-8">
                {/* Questions Per Quiz Section */}
                <div className="space-y-4">
                    <div className="flex items-center gap-2 mb-2">
                        <HelpCircle className="h-5 w-5 text-primary" />
                        <div>
                            <Label className="text-base font-semibold block">{t('preferences.quiz.questionCount', 'Questions per Quiz')}</Label>
                            <p className="text-xs text-muted-foreground font-normal">
                                {t('preferences.quiz.questionCountDescription', 'Choose how many questions to include in each quiz session.')}
                            </p>
                        </div>
                    </div>

                    <RadioGroup
                        value={localPrefs.questionCount.toString()}
                        onValueChange={handleQuestionCountChange}
                        className="flex gap-4 ml-7"
                    >
                        {[5, 10, 15, 20].map((count) => (
                            <div key={count} className="flex items-center space-x-2">
                                <RadioGroupItem value={count.toString()} id={`q-${count}`} />
                                <Label htmlFor={`q-${count}`}>{count}</Label>
                            </div>
                        ))}
                    </RadioGroup>
                </div>

                <Separator />

                {/* Knowledge Areas Section */}
                <div className="space-y-4">
                    <div className="flex md:items-center justify-between flex-col md:flex-row gap-4">
                        <div className="space-y-1">
                            <Label className="text-base font-semibold block">{t('preferences.categories.title', 'Knowledge Areas')}</Label>
                            <p className="text-sm text-muted-foreground">
                                {t('preferences.categories.description', 'Select the topics you want to explore.')}
                            </p>
                        </div>
                        <div className="flex gap-2">
                            <Button variant="outline" size="sm" onClick={handleSelectAllCategories} className="h-8 gap-1">
                                <CheckSquare className="h-3 w-3" />
                                {t('common.selectAll', 'All')}
                            </Button>
                            <Button variant="outline" size="sm" onClick={handleDeselectAllCategories} className="h-8 gap-1">
                                <Square className="h-3 w-3" />
                                {t('common.deselectAll', 'None')}
                            </Button>
                        </div>
                    </div>

                    <div className="rounded-md border p-1">
                        <ScrollArea className="h-[300px]">
                            {loadingCategories && (
                                <div className="flex justify-center p-8">
                                    <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
                                </div>
                            )}
                            {errorCategories && (
                                <p className="p-4 text-center text-destructive text-sm">{errorCategories}</p>
                            )}

                            <div className="p-4 grid gap-4 grid-cols-1 sm:grid-cols-2">
                                {categories.map((cat: WikipediaCategory) => (
                                    <div
                                        key={cat.id}
                                        className="flex items-start space-x-3 p-2 rounded hover:bg-muted/50 transition-colors cursor-pointer"
                                        onClick={() => handleCategoryToggle(cat.id, !localPrefs.selectedCategories.includes(cat.id))}
                                    >
                                        <Checkbox
                                            id={cat.id}
                                            checked={localPrefs.selectedCategories.includes(cat.id)}
                                            onCheckedChange={(checked) => handleCategoryToggle(cat.id, checked as boolean)}
                                            className="mt-1"
                                            onClick={(e) => e.stopPropagation()} // Let onCheckedChange handle it, avoid double toggle from container
                                        />
                                        <div className="grid gap-1.5 leading-none">
                                            <Label
                                                className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70 cursor-pointer"
                                            >
                                                {currentLang === 'fr' ? cat.nameFr : cat.name}
                                            </Label>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </ScrollArea>
                    </div>
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
