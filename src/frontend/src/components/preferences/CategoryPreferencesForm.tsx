import { useTranslation } from 'react-i18next';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Label } from '@/components/ui/label';
import { Checkbox } from '@/components/ui/checkbox';
import { Button } from '@/components/ui/button';
import { Loader2, Library, CheckSquare, Square } from 'lucide-react';
import { ScrollArea } from '@/components/ui/scroll-area';
import { useState, useEffect } from 'react';
import { useCategories } from '@/hooks/useCategories';
import type { WikipediaCategory } from '@/models/SeedData';

interface CategoryPreferencesFormProps {
    selectedCategories: string[];
    onSave: (categories: string[]) => Promise<void>;
    isSaving: boolean;
}

export function CategoryPreferencesForm({ selectedCategories: initialSelected, onSave, isSaving }: CategoryPreferencesFormProps) {
    const { t, i18n } = useTranslation();
    const { categories, loading: loadingCategories, error: errorCategories } = useCategories();

    // Local state for checkboxes
    const [selected, setSelected] = useState<string[]>(initialSelected);
    const [hasChanges, setHasChanges] = useState(false);

    // Sync when props change or initial load
    useEffect(() => {
        setSelected(initialSelected);
        setHasChanges(false);
    }, [initialSelected]);

    const handleToggle = (categoryId: string, checked: boolean) => {
        setSelected(prev => {
            let next;
            if (checked) {
                next = [...prev, categoryId];
            } else {
                next = prev.filter(id => id !== categoryId);
            }

            // Validate at least one category ?? (Business rule check, maybe handled upstream or here)
            // For now, allow empty but maybe show warning or disable save if empty?

            // Check change detection by comparing sorted arrays or just set true
            const isDifferent =
                next.length !== initialSelected.length ||
                !next.every(id => initialSelected.includes(id));

            setHasChanges(isDifferent);
            return next;
        });
    };

    const handleSelectAll = () => {
        const allIds = categories.map(c => c.id);
        setSelected(allIds);
        setHasChanges(allIds.length !== initialSelected.length || !initialSelected.every(id => allIds.includes(id))); // Simplify: likely changed
    };

    const handleDeselectAll = () => {
        setSelected([]);
        setHasChanges(initialSelected.length > 0);
    };

    const handleSave = async () => {
        if (selected.length === 0) {
            // Optional: Block save or warn
            // alert(t('preferences.categories.errorEmpty'));
            // allowing empty for now but backend might default it back
        }
        await onSave(selected);
        setHasChanges(false);
    };

    const handleCancel = () => {
        setSelected(initialSelected);
        setHasChanges(false);
    };

    const currentLang = i18n.language.split('-')[0]; // en, fr

    return (
        <Card>
            <CardHeader className="pb-3">
                <CardTitle className="text-xl flex items-center gap-2">
                    <Library className="h-5 w-5 text-primary" />
                    {t('preferences.categories.title', 'Knowledge Areas')}
                </CardTitle>
                <CardDescription>
                    {t('preferences.categories.description', 'Select the topics you want to explore.')}
                </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
                <div className="flex gap-2 justify-end mb-2">
                    <Button variant="outline" size="sm" onClick={handleSelectAll} className="h-8 gap-1">
                        <CheckSquare className="h-3 w-3" />
                        {t('common.selectAll', 'All')}
                    </Button>
                    <Button variant="outline" size="sm" onClick={handleDeselectAll} className="h-8 gap-1">
                        <Square className="h-3 w-3" />
                        {t('common.deselectAll', 'None')}
                    </Button>
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
                                <div key={cat.id} className="flex items-start space-x-3 p-2 rounded hover:bg-muted/50 transition-colors">
                                    <Checkbox
                                        id={cat.id}
                                        checked={selected.includes(cat.id)}
                                        onCheckedChange={(checked) => handleToggle(cat.id, checked as boolean)}
                                        className="mt-1"
                                    />
                                    <div className="grid gap-1.5 leading-none cursor-pointer" onClick={() => handleToggle(cat.id, !selected.includes(cat.id))}>
                                        <Label
                                            htmlFor={cat.id}
                                            className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70 cursor-pointer"
                                        >
                                            {currentLang === 'fr' ? cat.nameFr : cat.name}
                                        </Label>
                                        <p className="text-[0.8rem] text-muted-foreground">
                                            {/* Could add category descriptions later */}
                                        </p>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </ScrollArea>
                </div>

                <div className="pt-4 flex justify-end gap-2">
                    {hasChanges && (
                        <Button variant="ghost" onClick={handleCancel} disabled={isSaving}>
                            {t('common.cancel', 'Cancel')}
                        </Button>
                    )}
                    <Button onClick={handleSave} disabled={!hasChanges || isSaving}>
                        {isSaving && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                        {t('common.save', 'Save Categories')}
                    </Button>
                </div>
            </CardContent>
        </Card>
    );
}
