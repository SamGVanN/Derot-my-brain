import { useState, useEffect, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { UserService } from '../services/UserService';
import type { User } from '../models/User';

import { categoryApi } from '@/api/categoryApi';
import { Layout } from '@/components/Layout';
import { Card, CardContent, CardDescription, CardHeader, CardTitle, CardFooter } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
import { Checkbox } from '@/components/ui/checkbox';
import { Loader2, CheckCircle2, AlertCircle, Globe, Palette, Check, ChevronDown } from 'lucide-react';
import { Separator } from '@/components/ui/separator';
import { GuideContent } from '../features/welcome/GuideContent';
import { ScrollArea } from '@/components/ui/scroll-area';
import { themes } from '@/lib/themes';
import { cn } from '@/lib/utils';
import { useTheme } from '@/components/theme-provider';


interface UserPreferencesPageProps {
    user: User;
    onUserUpdated: (user: User) => void;
    onCancel: () => void;
}

const languages = [
    { code: 'en', label: 'English' },
    { code: 'fr', label: 'Français' }
];

export default function UserPreferencesPage({ user, onUserUpdated }: UserPreferencesPageProps) {
    const { t, i18n } = useTranslation();
    const queryClient = useQueryClient();
    const { setTheme } = useTheme();

    // Separate state for general preferences and categories
    const [generalPreferences, setGeneralPreferences] = useState({
        language: user.preferences.language,
        preferredTheme: user.preferences.preferredTheme,
        questionCount: user.preferences.questionCount
    });
    const [selectedCategories, setSelectedCategories] = useState<string[]>(user.preferences.selectedCategories || []);

    // Separate success/error states for each form
    const [generalSaveSuccess, setGeneralSaveSuccess] = useState(false);
    const [generalSaveError, setGeneralSaveError] = useState<string | null>(null);
    const [categorySaveSuccess, setCategorySaveSuccess] = useState(false);
    const [categorySaveError, setCategorySaveError] = useState<string | null>(null);

    const [showGuide, setShowGuide] = useState(false);

    // Dropdown states
    const [isLanguageDropdownOpen, setIsLanguageDropdownOpen] = useState(false);
    const [isThemeDropdownOpen, setIsThemeDropdownOpen] = useState(false);
    const languageDropdownRef = useRef<HTMLDivElement>(null);
    const themeDropdownRef = useRef<HTMLDivElement>(null);

    // Fetch categories
    const { data: categories, isLoading: isLoadingCategories } = useQuery({
        queryKey: ['categories'],
        queryFn: async () => {
            return categoryApi.getAllCategories();
        }
    });

    // Update local state when user prop changes
    useEffect(() => {
        setGeneralPreferences({
            language: user.preferences.language,
            preferredTheme: user.preferences.preferredTheme,
            questionCount: user.preferences.questionCount
        });
        setSelectedCategories(user.preferences.selectedCategories || []);
    }, [user]);

    // Close dropdowns when clicking outside
    useEffect(() => {
        function handleClickOutside(event: MouseEvent) {
            if (languageDropdownRef.current && !languageDropdownRef.current.contains(event.target as Node)) {
                setIsLanguageDropdownOpen(false);
            }
            if (themeDropdownRef.current && !themeDropdownRef.current.contains(event.target as Node)) {
                setIsThemeDropdownOpen(false);
            }
        }
        document.addEventListener("mousedown", handleClickOutside);
        return () => {
            document.removeEventListener("mousedown", handleClickOutside);
        };
    }, []);

    // Mutation for general preferences
    const generalMutation = useMutation({
        mutationFn: (prefs: { language: string; preferredTheme: string; questionCount: number }) =>
            UserService.updateGeneralPreferences(user.id, prefs),
        onSuccess: (updatedUser) => {
            onUserUpdated(updatedUser);
            queryClient.invalidateQueries({ queryKey: ['users'] });
            setGeneralSaveSuccess(true);
            setTimeout(() => setGeneralSaveSuccess(false), 3000);
        },
        onError: (err) => {
            console.error(err);
            setGeneralSaveError(t('preferences.saveError') || 'Failed to save general preferences');
        }
    });

    // Mutation for category preferences
    const categoryMutation = useMutation({
        mutationFn: (categories: string[]) =>
            UserService.updateCategoryPreferences(user.id, categories),
        onSuccess: (updatedUser) => {
            onUserUpdated(updatedUser);
            queryClient.invalidateQueries({ queryKey: ['users'] });
            setCategorySaveSuccess(true);
            setTimeout(() => setCategorySaveSuccess(false), 3000);
        },
        onError: (err) => {
            console.error(err);
            setCategorySaveError(t('preferences.saveError') || 'Failed to save categories');
        }
    });

    const handleSaveGeneral = () => {
        setGeneralSaveError(null);
        setGeneralSaveSuccess(false);
        generalMutation.mutate(generalPreferences);
    };

    const handleCancelGeneral = () => {
        // Restore original values
        setGeneralPreferences({
            language: user.preferences.language,
            preferredTheme: user.preferences.preferredTheme,
            questionCount: user.preferences.questionCount
        });
        // Restore visual state
        i18n.changeLanguage(user.preferences.language);
        setTheme(user.preferences.preferredTheme);
    };

    const handleSaveCategories = () => {
        setCategorySaveError(null);
        setCategorySaveSuccess(false);
        categoryMutation.mutate(selectedCategories);
    };

    const handleCancelCategories = () => {
        setSelectedCategories(user.preferences.selectedCategories || []);
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
                                {/* Language Selector */}
                                <div>
                                    <Label className="mb-3 block">{t('preferences.language')}</Label>
                                    <div className="relative inline-block text-left w-full" ref={languageDropdownRef}>
                                        <Button
                                            variant="outline"
                                            size="sm"
                                            onClick={() => setIsLanguageDropdownOpen(!isLanguageDropdownOpen)}
                                            className="flex items-center justify-between gap-2 h-9 w-full border-border/60 bg-background/50 backdrop-blur-sm group"
                                        >
                                            <div className="flex items-center gap-2">
                                                <Globe className="h-4 w-4 text-primary group-hover:text-accent-foreground transition-colors" />
                                                <span className="font-medium">
                                                    {languages.find(l => l.code === generalPreferences.language)?.label || 'English'}
                                                </span>
                                            </div>
                                            <ChevronDown className={cn("h-3 w-3 text-muted-foreground transition-transform duration-200", isLanguageDropdownOpen && "rotate-180")} />
                                        </Button>

                                        {isLanguageDropdownOpen && (
                                            <div className="absolute left-0 mt-2 w-full origin-top-left rounded-md border border-border bg-popover p-1 shadow-md ring-1 ring-black ring-opacity-5 focus:outline-none animate-in fade-in zoom-in-95 duration-100 z-50">
                                                <div className="space-y-1">
                                                    {languages.map((lang) => {
                                                        const isSelected = generalPreferences.language === lang.code;
                                                        return (
                                                            <button
                                                                key={lang.code}
                                                                onClick={() => {
                                                                    setGeneralPreferences({ ...generalPreferences, language: lang.code });
                                                                    i18n.changeLanguage(lang.code); // Apply immediately for preview
                                                                    setIsLanguageDropdownOpen(false);
                                                                }}
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
                                </div>

                                {/* Theme Selector */}
                                <div>
                                    <Label className="mb-3 block">{t('preferences.theme')}</Label>
                                    <div className="relative inline-block text-left w-full" ref={themeDropdownRef}>
                                        <Button
                                            variant="outline"
                                            size="sm"
                                            onClick={() => setIsThemeDropdownOpen(!isThemeDropdownOpen)}
                                            className="flex items-center justify-between gap-2 h-9 w-full border-border/60 bg-background/50 backdrop-blur-sm group"
                                        >
                                            <div className="flex items-center gap-2">
                                                <Palette className="h-4 w-4 text-primary group-hover:text-accent-foreground transition-colors" />
                                                <span className="font-medium">
                                                    {themes[generalPreferences.preferredTheme]?.label || 'Theme'}
                                                </span>
                                            </div>
                                            <ChevronDown className={cn("h-3 w-3 text-muted-foreground transition-transform duration-200", isThemeDropdownOpen && "rotate-180")} />
                                        </Button>

                                        {isThemeDropdownOpen && (
                                            <div className="absolute left-0 mt-2 w-full origin-top-left rounded-md border border-border bg-popover p-1 shadow-md ring-1 ring-black ring-opacity-5 focus:outline-none animate-in fade-in zoom-in-95 duration-100 z-50">
                                                <div className="space-y-1">
                                                    {Object.values(themes).map((theme) => {
                                                        const isSelected = generalPreferences.preferredTheme === theme.name;
                                                        return (
                                                            <button
                                                                key={theme.name}
                                                                onClick={() => {
                                                                    setGeneralPreferences({ ...generalPreferences, preferredTheme: theme.name });
                                                                    setTheme(theme.name); // Apply immediately for preview
                                                                    setIsThemeDropdownOpen(false);
                                                                }}
                                                                className={cn(
                                                                    "w-full flex items-center justify-between rounded-sm px-3 py-2.5 text-sm transition-colors",
                                                                    isSelected
                                                                        ? "bg-accent text-accent-foreground font-medium"
                                                                        : "text-popover-foreground hover:bg-muted hover:text-foreground"
                                                                )}
                                                            >
                                                                <span className="font-medium mr-4">{theme.label}</span>
                                                                <div className="flex items-center gap-3">
                                                                    <div className="flex items-center gap-1 bg-muted/40 p-1 rounded-md border border-border/40">
                                                                        {/* Color Preview Blocks */}
                                                                        <div className="h-3 w-3 rounded-full shadow-sm ring-1 ring-inset ring-black/10 dark:ring-white/10" style={{ backgroundColor: theme.colors.background }} title="Background" />
                                                                        <div className="h-3 w-3 rounded-full shadow-sm ring-1 ring-inset ring-black/10 dark:ring-white/10" style={{ backgroundColor: theme.colors.primary }} title="Primary" />
                                                                        <div className="h-3 w-3 rounded-full shadow-sm ring-1 ring-inset ring-black/10 dark:ring-white/10" style={{ backgroundColor: theme.colors.secondary }} title="Secondary" />
                                                                        <div className="h-3 w-3 rounded-full shadow-sm ring-1 ring-inset ring-black/10 dark:ring-white/10" style={{ backgroundColor: theme.colors.accent }} title="Accent" />
                                                                    </div>
                                                                    {isSelected && <Check className="h-4 w-4 text-primary" />}
                                                                </div>
                                                            </button>
                                                        );
                                                    })}
                                                </div>
                                            </div>
                                        )}
                                    </div>
                                </div>
                            </div>

                            <Separator />

                            <div>
                                <Label className="mb-3 block">{t('preferences.questionCount')}</Label>
                                <RadioGroup
                                    value={generalPreferences.questionCount?.toString()}
                                    onValueChange={(val: string) => setGeneralPreferences({ ...generalPreferences, questionCount: parseInt(val) })}
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
                        <CardFooter className="flex justify-end gap-3 border-t pt-4">
                            <Button
                                variant="outline"
                                onClick={handleCancelGeneral}
                                disabled={generalMutation.isPending}
                            >
                                {t('common.cancel')}
                            </Button>
                            <Button
                                onClick={handleSaveGeneral}
                                disabled={generalMutation.isPending}
                            >
                                {generalMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                                {t('common.save')}
                            </Button>
                        </CardFooter>
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
                        <CardFooter className="flex justify-end gap-3 border-t pt-4">
                            <Button
                                variant="outline"
                                onClick={handleCancelCategories}
                                disabled={categoryMutation.isPending}
                            >
                                {t('common.cancel')}
                            </Button>
                            <Button
                                onClick={handleSaveCategories}
                                disabled={!isFormValid || categoryMutation.isPending}
                            >
                                {categoryMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                                {t('common.save')}
                            </Button>
                        </CardFooter>
                    </Card>
                </div>

                {/* General Settings Success/Error Notifications */}
                {generalSaveSuccess && (
                    <div className="fixed bottom-4 right-4 bg-green-500 text-white px-4 py-2 rounded-md shadow-lg flex items-center gap-2 animate-in fade-in slide-in-from-bottom-2">
                        <CheckCircle2 className="h-4 w-4" />
                        {t('preferences.saveSuccess')}
                    </div>
                )}

                {generalSaveError && (
                    <div className="fixed bottom-4 right-4 bg-destructive text-destructive-foreground px-4 py-2 rounded-md shadow-lg flex items-center gap-2 animate-in fade-in slide-in-from-bottom-2">
                        <AlertCircle className="h-4 w-4" />
                        {generalSaveError}
                    </div>
                )}

                {/* Category Settings Success/Error Notifications */}
                {categorySaveSuccess && (
                    <div className="fixed bottom-4 right-4 bg-green-500 text-white px-4 py-2 rounded-md shadow-lg flex items-center gap-2 animate-in fade-in slide-in-from-bottom-2">
                        <CheckCircle2 className="h-4 w-4" />
                        {t('preferences.saveSuccess')}
                    </div>
                )}

                {categorySaveError && (
                    <div className="fixed bottom-4 right-4 bg-destructive text-destructive-foreground px-4 py-2 rounded-md shadow-lg flex items-center gap-2 animate-in fade-in slide-in-from-bottom-2">
                        <AlertCircle className="h-4 w-4" />
                        {categorySaveError}
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
