import React from 'react';
import { useTranslation } from 'react-i18next';
import { Search } from 'lucide-react';
import { Input } from './ui/input';
import { Button } from './ui/button';
import { cn } from '@/lib/utils';

export type FocusFilter = 'all' | 'pinned' | 'active' | 'archived';

interface FocusAreaFiltersProps {
    searchQuery: string;
    setSearchQuery: (query: string) => void;
    activeFilter: FocusFilter;
    setActiveFilter: (filter: FocusFilter) => void;
}

export const FocusAreaFilters: React.FC<FocusAreaFiltersProps> = ({
    searchQuery,
    setSearchQuery,
    activeFilter,
    setActiveFilter
}) => {
    const { t } = useTranslation();

    const filters: { id: FocusFilter; label: string }[] = [
        { id: 'all', label: t('focusArea.filterAll') },
        { id: 'active', label: t('focusArea.filterActive') },
        { id: 'pinned', label: t('focusArea.filterPinned') },
        { id: 'archived', label: t('focusArea.filterArchived') },
    ];

    return (
        <div className="flex flex-col md:flex-row gap-4 items-center justify-between mb-8">
            <div className="relative w-full md:max-w-sm">
                <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                <Input
                    placeholder={t('focusArea.searchPlaceholder')}
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    className="pl-9 bg-background/50 border-border/40 focus:bg-background transition-all"
                />
            </div>

            <div className="flex items-center gap-1 p-1 bg-muted/40 rounded-lg border border-border/40 w-full md:w-auto overflow-x-auto no-scrollbar">
                {filters.map((filter) => (
                    <Button
                        key={filter.id}
                        variant={activeFilter === filter.id ? "secondary" : "ghost"}
                        size="sm"
                        onClick={() => setActiveFilter(filter.id)}
                        className={cn(
                            "flex-1 md:flex-none text-xs h-8 px-4 rounded-md transition-all",
                            activeFilter === filter.id ? "bg-background shadow-sm text-foreground font-semibold" : "text-muted-foreground hover:text-foreground hover:bg-transparent"
                        )}
                    >
                        {filter.label}
                    </Button>
                ))}
            </div>
        </div>
    );
};
