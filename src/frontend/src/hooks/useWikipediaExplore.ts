import { useState, useCallback, useEffect, useRef } from 'react';
import { activityApi } from '@/api/activityApi';
import { wikipediaApi, type WikipediaArticle } from '@/api/wikipediaApi';
import { useAuthStore } from '@/stores/useAuthStore';
import { useWikipediaExploreStore } from '@/stores/useWikipediaExploreStore';

export function useWikipediaExplore() {
    const { user } = useAuthStore();
    const userId = user?.id;

    const {
        exploreId, setExploreId,
        articles, setArticles,
        backlogAddsCount, setBacklogAddsCount,
        startTime, setStartTime,
        error, setError,
        reset
    } = useWikipediaExploreStore();

    const [isInitializing, setIsInitializing] = useState(false);
    const [loadingAction, setLoadingAction] = useState<string | null>(null);
    const hasAttemptedInit = useRef(false);

    const fetchArticles = useCallback(async () => {
        if (!userId) return;
        try {
            setIsInitializing(true);
            const data = await activityApi.getExploreArticles(userId);
            setArticles(data || []);
        } catch (e) {
            console.error('Failed to fetch articles', e);
            setError('Erreur lors du chargement des articles');
        } finally {
            setIsInitializing(false);
        }
    }, [userId, setArticles, setError]);

    const initExplore = useCallback(async () => {
        if (!userId) return;

        try {
            setIsInitializing(true);
            setError(null);
            hasAttemptedInit.current = true;

            const data = await activityApi.explore(userId, {
                title: "Wikipedia Exploration",
                sourceType: 1 // Wikipedia
            });

            if (data?.id) {
                setExploreId(data.id);
                setStartTime(Date.now());
                setBacklogAddsCount(0);
                await fetchArticles();
            } else {
                setError('Impossible de démarrer la session (ID manquant)');
            }
        } catch (e: any) {
            console.error('Failed to initialize explore session', e);
            setError('Erreur lors de l’initialisation de la Zone Derot');
        } finally {
            setIsInitializing(false);
        }
    }, [userId, fetchArticles, setExploreId, setStartTime, setBacklogAddsCount, setError]);

    // Initial load only if we have a user and haven't attempted yet and have no articles
    useEffect(() => {
        if (userId && !exploreId && !isInitializing && !error && !hasAttemptedInit.current && articles.length === 0) {
            initExplore();
        }
    }, [userId, exploreId, isInitializing, initExplore, error, articles.length]);

    const addToBacklog = async (article: WikipediaArticle) => {
        if (!userId) return false;
        try {
            setLoadingAction(`backlog-${article.title}`);
            await wikipediaApi.addToBacklog(userId, article);
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
        if (!userId) return null;
        try {
            setLoadingAction(`read-${article.title}`);
            const duration = startTime ? Math.floor((Date.now() - startTime) / 1000) : undefined;
            const data = await activityApi.read(userId, {
                title: article.title,
                language: article.lang,
                sourceId: article.sourceUrl, // Wikipedia uses URL as SourceId
                sourceType: 1, // Wikipedia
                originExploreId: exploreId || undefined,
                backlogAddsCount: backlogAddsCount || undefined,
                exploreDurationSeconds: duration
            });
            return data;
        } catch (e) {
            console.error('Failed to initiate reading', e);
            return null;
        } finally {
            setLoadingAction(null);
        }
    };

    const stopExplore = useCallback(async () => {
        if (!userId || !exploreId || !startTime) return;
        try {
            setLoadingAction('stop-explore');
            const duration = Math.floor((Date.now() - startTime) / 1000);
            await activityApi.stopExplore(userId, exploreId, {
                durationSeconds: duration,
                backlogAddsCount
            });
            reset();
        } catch (e) {
            console.error('Failed to stop explore session', e);
        } finally {
            setLoadingAction(null);
        }
    }, [userId, exploreId, startTime, backlogAddsCount, reset]);

    return {
        exploreId,
        articles,
        isInitializing,
        loadingAction,
        backlogAddsCount,
        error,
        addToBacklog,
        readArticle,
        stopExplore,
        refresh: initExplore
    };
}
