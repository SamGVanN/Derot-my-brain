import { useState, useEffect, useCallback } from 'react';
import { wikipediaApi, type WikipediaArticle } from '@/api/wikipediaApi';

export function useWikipediaExplore() {
    const [exploreId, setExploreId] = useState<string | null>(null);
    const [backlogAddsCount, setBacklogAddsCount] = useState(0);
    const [isInitializing, setIsInitializing] = useState(true);
    const [loadingAction, setLoadingAction] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);

    const initExplore = useCallback(async () => {
        try {
            setIsInitializing(true);
            setError(null);
            const data = await wikipediaApi.explore();
            if (data?.id) {
                setExploreId(data.id);
            }
        } catch (e: any) {
            console.error('Failed to initialize explore session', e);
            setError('Failed to initialize session');
        } finally {
            setIsInitializing(false);
        }
    }, []);

    useEffect(() => {
        initExplore();
    }, [initExplore]);

    const addToBacklog = async (article: WikipediaArticle) => {
        try {
            setLoadingAction(`backlog-${article.title}`);
            await wikipediaApi.addToBacklog(article);
            setBacklogAddsCount((c) => c + 1);
            return true;
        } catch (e) {
            console.error('Failed to add to backlog', e);
            return false;
        } finally {
            setLoadingAction(null);
        }
    };

    const readArticle = async (article: WikipediaArticle) => {
        try {
            setLoadingAction(`read-${article.title}`);
            const data = await wikipediaApi.read({
                title: article.title,
                language: article.lang,
                sourceUrl: article.sourceUrl,
                originExploreId: exploreId || undefined,
                backlogAddsCount: backlogAddsCount || undefined
            });
            return data?.activity;
        } catch (e) {
            console.error('Failed to initiate reading', e);
            return null;
        } finally {
            setLoadingAction(null);
        }
    };

    return {
        exploreId,
        isInitializing,
        loadingAction,
        error,
        addToBacklog,
        readArticle,
        refresh: initExplore
    };
}
