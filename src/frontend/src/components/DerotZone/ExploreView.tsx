import DerotZone, { type ArticleCard } from './DerotZone';
import { TrendingUp, RefreshCw } from 'lucide-react';
import { Button } from '@/components/ui/button';

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
    onRead,
    onAddToBacklog,
    loadingAction,
    isLoading
}: ExploreViewProps) {
    return (
        <div className="space-y-8 animate-in fade-in duration-1000">
            <div className="flex items-center justify-between border-b pb-4 border-border/40 pt-4">
                <div className="flex items-center gap-2">
                    <TrendingUp className="h-5 w-5 text-primary" />
                    <h2 className="text-2xl font-bold tracking-tight">Trending Topics</h2>
                </div>
                <Button
                    variant="outline"
                    size="sm"
                    onClick={onRefresh}
                    disabled={isLoading}
                    className="gap-2"
                >
                    <RefreshCw className={`h-4 w-4 ${isLoading ? 'animate-spin' : ''}`} />
                    Rafraîchir
                </Button>
            </div>

            <DerotZone
                articles={articles}
                onRead={onRead}
                onAddToBacklog={onAddToBacklog}
                loadingAction={loadingAction}
            />
        </div>
    );
}
