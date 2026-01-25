import { useState, useEffect, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { Layout } from '@/components/Layout';
import { useAuth } from '@/hooks/useAuth';
import { userFocusApi } from '@/api/userFocusApi';
import type { UserFocus } from '@/models/UserFocus';
import { FocusAreaCard } from '@/components/FocusAreaCard';
import { FocusAreaFilters, type FocusFilter } from '@/components/FocusAreaFilters';
import { Target, Loader2 } from 'lucide-react';
import { PageHeader } from '@/components/PageHeader';

export function MyFocusAreaPage() {
    const { t } = useTranslation();
    const { user } = useAuth();
    const [focuses, setFocuses] = useState<UserFocus[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [searchQuery, setSearchQuery] = useState('');
    const [activeFilter, setActiveFilter] = useState<FocusFilter>('active');

    useEffect(() => {
        const fetchFocuses = async () => {
            if (!user) return;
            setIsLoading(true);
            try {
                const data = await userFocusApi.getUserFocuses(user.id);
                setFocuses(data);
            } catch (error) {
                console.error('Failed to fetch focus areas:', error);
            } finally {
                setIsLoading(false);
            }
        }
        fetchFocuses();
    }, [user]);

    const handleUntrack = async (focus: UserFocus) => {
        if (!user) return;
        try {
            await userFocusApi.untrackTopic(user.id, focus.sourceHash);
            setFocuses(prev => prev.filter(f => f.sourceHash !== focus.sourceHash));
        } catch (error) {
            console.error('Failed to untrack focus area:', error);
        }
    };

    const handleTogglePin = async (focus: UserFocus) => {
        if (!user) return;
        try {
            const updated = await userFocusApi.togglePin(user.id, focus.sourceHash);
            setFocuses(prev => prev.map(f => f.sourceHash === focus.sourceHash ? updated : f));
        } catch (error) {
            console.error('Failed to toggle pin:', error);
        }
    };

    const handleToggleArchive = async (focus: UserFocus) => {
        if (!user) return;
        try {
            const updated = await userFocusApi.toggleArchive(user.id, focus.sourceHash);
            setFocuses(prev => prev.map(f => f.sourceHash === focus.sourceHash ? updated : f));
        } catch (error) {
            console.error('Failed to toggle archive:', error);
        }
    };

    const filteredFocuses = useMemo(() => {
        return focuses.filter(f => {
            const matchesSearch = f.displayTitle.toLowerCase().includes(searchQuery.toLowerCase());
            const matchesFilter =
                activeFilter === 'all' ? true :
                    activeFilter === 'pinned' ? f.isPinned :
                        activeFilter === 'archived' ? f.isArchived :
                            activeFilter === 'active' ? (!f.isArchived) : true;

            return matchesSearch && matchesFilter;
        });
    }, [focuses, searchQuery, activeFilter]);

    if (!user) return null;

    return (
        <Layout>
            <div className="container max-w-7xl mx-auto py-10 px-4 space-y-12 animate-in fade-in duration-700">
                <PageHeader
                    title={t('focusArea.title')}
                    subtitle={t('focusArea.subtitle')}
                    description={t('focusArea.description')}
                    icon={Target}
                />

                {/* Filters */}
                <FocusAreaFilters
                    searchQuery={searchQuery}
                    setSearchQuery={setSearchQuery}
                    activeFilter={activeFilter}
                    setActiveFilter={setActiveFilter}
                />

                {/* Content */}
                {isLoading ? (
                    <div className="flex flex-col items-center justify-center py-20 space-y-4">
                        <Loader2 className="w-10 h-10 animate-spin text-primary/40" />
                        <p className="text-muted-foreground animate-pulse">{t('common.loading')}</p>
                    </div>
                ) : focuses.length === 0 ? (
                    <div className="flex flex-col items-center justify-center py-20 text-center space-y-6 bg-muted/20 rounded-2xl border-2 border-dashed border-border/40">
                        <div className="p-4 bg-background rounded-full shadow-sm border">
                            <Target className="w-12 h-12 text-muted-foreground/40" />
                        </div>
                        <div className="space-y-2">
                            <h3 className="text-xl font-semibold">{t('focusArea.title')}</h3>
                            <p className="text-muted-foreground max-w-sm mx-auto">
                                {t('focusArea.empty')}
                            </p>
                        </div>
                    </div>
                ) : filteredFocuses.length === 0 ? (
                    <div className="flex flex-col items-center justify-center py-20 text-center space-y-4">
                        <p className="text-muted-foreground text-lg italic">
                            {t('focusArea.noResults')}
                        </p>
                    </div>
                ) : (
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                        {filteredFocuses.map(focus => (
                            <FocusAreaCard
                                key={focus.sourceHash}
                                focus={focus}
                                onUntrack={handleUntrack}
                                onTogglePin={handleTogglePin}
                                onToggleArchive={handleToggleArchive}
                            />
                        ))}
                    </div>
                )}
            </div>
        </Layout>
    );
}
