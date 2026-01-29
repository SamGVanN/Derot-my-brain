import DerotZone, { type ArticleCard } from './DerotZone';
import { Radar, RefreshCw } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { useTranslation } from 'react-i18next';

interface ExploreViewProps {
    articles: ArticleCard[];
    onRefresh: () => void;
    onRead: (article: ArticleCard) => Promise<void>;
    onAddToBacklog: (article: ArticleCard) => Promise<boolean>;
    loadingAction: string | null;
    isLoading?: boolean;
}

export function ExploreView({
    articles,
    onRefresh,
    onStartExplore,
    onRead,
    onAddToBacklog,
    loadingAction,
    isLoading
}: ExploreViewProps & { onStartExplore: () => Promise<void> }) {
    const { t } = useTranslation();

    return (
        <div className="space-y-8 animate-in fade-in duration-1000">
            {articles.length > 0 && (
                <div className="flex items-center justify-between border-b pb-4 border-border/40 pt-4">
                    <div className="flex items-center gap-2">
                        <Radar className="h-5 w-5 text-primary" />
                        <h2 className="text-2xl font-bold tracking-tight">{t('derot.explore.title', "Suggestions d'exploration")}</h2>
                    </div>
                    <Button
                        variant="outline"
                        size="sm"
                        onClick={onRefresh}
                        disabled={isLoading}
                        className="gap-2"
                    >
                        <RefreshCw className={`h-4 w-4 ${isLoading ? 'animate-spin' : ''}`} />
                        {t('derot.explore.refresh', 'Rafraîchir')}
                    </Button>
                </div>
            )}

            <DerotZone
                articles={articles}
                onRead={onRead}
                onAddToBacklog={onAddToBacklog}
                onStartExplore={onStartExplore}
                loadingAction={loadingAction}
                isLoading={isLoading}
            />
        </div>
    );
}
