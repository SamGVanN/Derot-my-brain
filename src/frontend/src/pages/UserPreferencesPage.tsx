import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { UserService } from '../services/UserService';
import type { User, UserPreferences } from '../models/User';
import type { WikipediaCategory } from '../models/Category';
import { Layout } from '@/components/Layout';
import { Card, CardContent, CardDescription, CardHeader, CardTitle, CardFooter } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
import { Checkbox } from '@/components/ui/checkbox';
import { Loader2, CheckCircle2 } from 'lucide-react';
import { ThemeSelector } from '@/components/theme-selector';
import { LanguageSwitcher } from '@/components/LanguageSwitcher';
import { Separator } from '@/components/ui/separator';
import { GuideContent } from '../features/welcome/GuideContent';
import { ScrollArea } from '@/components/ui/scroll-area';

interface UserPreferencesPageProps {
    user: User;
    onUserUpdated: (user: User) => void;
}

export default function UserPreferencesPage({ user, onUserUpdated }: UserPreferencesPageProps) {
    const { t, i18n } = useTranslation();
    const queryClient = useQueryClient();
    const [preferences, setPreferences] = useState<UserPreferences>(user.preferences);
    const [selectedCategories, setSelectedCategories] = useState<string[]>(user.preferences.selectedCategories || []);
    const [saveSuccess, setSaveSuccess] = useState(false);
    const [showGuide, setShowGuide] = useState(false);

    // Fetch categories
    const { data: categories, isLoading: isLoadingCategories } = useQuery({
        queryKey: ['categories'],
        queryFn: async () => {
            const response = await fetch('http://localhost:5077/api/categories');
            if (!response.ok) throw new Error('Failed to fetch categories');
            return response.json() as Promise<WikipediaCategory[]>;
        }
    });

    // Update local state when user prop changes
    useEffect(() => {
        setPreferences(user.preferences);
        setSelectedCategories(user.preferences.selectedCategories || []);
    }, [user]);

    // Mutation to update preferences
    const mutation = useMutation({
        mutationFn: (updatedPrefs: UserPreferences) => UserService.updatePreferences(user.id, updatedPrefs),
        onSuccess: (updatedUser) => {
            onUserUpdated(updatedUser);
            queryClient.invalidateQueries({ queryKey: ['users'] });
            setSaveSuccess(true);
            setTimeout(() => setSaveSuccess(false), 3000);
        },
        onError: (err) => {
            console.error(err);
        }
    });

    const handleSave = () => {
        const updatedAppPreferences = {
            ...preferences,
            selectedCategories: selectedCategories
        };
        mutation.mutate(updatedAppPreferences);
    };

    const handleCategoryToggle = (categoryId: string, checked: boolean) => {
        if (checked) {
            setSelectedCategories([...selectedCategories, categoryId]);
        } else {
            // Check if at least one is selected
            if (selectedCategories.length <= 1 && selectedCategories.includes(categoryId)) {
                // Prevent deselecting the last one? Or just define validation logic on save.
                // Spec says "At least 1 category must be checked".
                // We can allow unchecked here but disable save button or show error.
                // Let's implement immediate feedback or prevent it.
                // Let's allow it but disable save validly.
            }
            setSelectedCategories(selectedCategories.filter(id => id !== categoryId));
        }
    };

    const handleSelectAll = () => {
        if (categories) {
            setSelectedCategories(categories.map(c => c.id));
        }
    };

    const handleDeselectAll = () => {
        setSelectedCategories([]);
    };

    const isFormValid = selectedCategories.length > 0;

    return (
        <Layout>
            <div className="max-w-4xl mx-auto py-8 px-4">
                <div className="flex justify-between items-center mb-6">
                    <h1 className="text-3xl font-bold">{t('preferences.title')}</h1>
                </div>

                <div className="grid gap-6">
                    {/* General Settings */}
                    <Card>
                        <CardHeader>
                            <CardTitle>General Settings</CardTitle>
                        </CardHeader>
                        <CardContent className="space-y-6">
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                                <div>
                                    <Label className="mb-3 block">{t('preferences.language')}</Label>
                                    <div>
                                        <LanguageSwitcher />
                                    </div>
                                </div>
                                <div>
                                    <Label className="mb-3 block">{t('preferences.theme')}</Label>
                                    <div>
                                        <ThemeSelector />
                                    </div>
                                </div>
                            </div>

                            <Separator />

                            <div>
                                <Label className="mb-3 block">{t('preferences.questionCount')}</Label>
                                <RadioGroup
                                    value={preferences.questionCount?.toString()}
                                    onValueChange={(val: string) => setPreferences({ ...preferences, questionCount: parseInt(val) })}
                                    className="flex space-x-4"
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

                            <div>
                                <Label className="mb-3 block">{t('welcome.guide.title')}</Label>
                                <div className="flex flex-wrap gap-4">
                                    <Button
                                        variant="outline"
                                        onClick={() => setShowGuide(true)}
                                    >
                                        {t('welcome.readGuide')}
                                    </Button>
                                </div>
                            </div>
                        </CardContent>
                    </Card>

                    {/* Wikipedia Categories */}
                    <Card>
                        <CardHeader>
                            <div className="flex justify-between items-center">
                                <div>
                                    <CardTitle>{t('preferences.categories')}</CardTitle>
                                    <CardDescription>{t('preferences.categoriesDescription')}</CardDescription>
                                </div>
                                <div className="text-sm text-muted-foreground">
                                    {t('preferences.selected', { count: selectedCategories.length })}
                                </div>
                            </div>
                        </CardHeader>
                        <CardContent>
                            <div className="flex gap-2 mb-4">
                                <Button variant="outline" size="sm" onClick={handleSelectAll}>
                                    {t('preferences.selectAll')}
                                </Button>
                                <Button variant="outline" size="sm" onClick={handleDeselectAll}>
                                    {t('preferences.deselectAll')}
                                </Button>
                            </div>

                            {isLoadingCategories ? (
                                <div className="flex justify-center p-4">
                                    <Loader2 className="animate-spin h-6 w-6 text-primary" />
                                </div>
                            ) : (
                                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
                                    {categories?.sort((a, b) => a.order - b.order).map((category) => (
                                        <div key={category.id} className="flex items-start space-x-2 p-2 rounded-md hover:bg-muted/50">
                                            <Checkbox
                                                id={`cat-${category.id}`}
                                                checked={selectedCategories.includes(category.id)}
                                                onCheckedChange={(checked: boolean) => handleCategoryToggle(category.id, checked)}
                                            />
                                            <Label
                                                htmlFor={`cat-${category.id}`}
                                                className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70 cursor-pointer pt-0.5"
                                            >
                                                {i18n.language.startsWith('fr') ? category.nameFr : category.name}
                                            </Label>
                                        </div>
                                    ))}
                                </div>
                            )}

                            {!isFormValid && (
                                <p className="text-destructive text-sm mt-4 font-medium">
                                    ⚠️ Please select at least one category.
                                </p>
                            )}
                        </CardContent>
                    </Card>

                    {/* Actions */}
                    <div className="flex justify-end gap-4">
                        <Button
                            size="lg"
                            onClick={handleSave}
                            disabled={!isFormValid || mutation.isPending}
                        >
                            {mutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                            {t('common.save')}
                        </Button>
                    </div>
                </div>

                {saveSuccess && (
                    <div className="fixed bottom-4 right-4 bg-green-500 text-white px-4 py-2 rounded-md shadow-lg flex items-center gap-2 animate-in fade-in slide-in-from-bottom-2">
                        <CheckCircle2 className="h-4 w-4" />
                        {t('preferences.saveSuccess')}
                    </div>
                )}

                {/* Guide Modal */}
                {showGuide && (
                    <div className="fixed inset-0 z-50 flex items-center justify-center bg-background/80 backdrop-blur-sm p-4">
                        <Card className="w-full max-w-3xl max-h-[90vh] flex flex-col shadow-xl">
                            <CardHeader>
                                <CardTitle className="text-2xl text-center">
                                    {t('welcome.guide.title')}
                                </CardTitle>
                            </CardHeader>
                            <CardContent className="flex-1 overflow-hidden">
                                <ScrollArea className="h-full pr-4">
                                    <GuideContent />
                                </ScrollArea>
                            </CardContent>
                            <CardFooter className="flex justify-end border-t p-6">
                                <Button onClick={() => setShowGuide(false)}>
                                    {t('common.back')}
                                </Button>
                            </CardFooter>
                        </Card>
                    </div>
                )}
            </div>
        </Layout>
    );
}
