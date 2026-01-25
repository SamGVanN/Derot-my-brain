import { useState, useCallback, useEffect, useRef } from 'react';
import { activityApi } from '@/api/activityApi';
import { wikipediaApi, type WikipediaArticle } from '@/api/wikipediaApi';
import { useAuthStore } from '@/stores/useAuthStore';

export function useWikipediaExplore() {
    const { user } = useAuthStore();
    const userId = user?.id;
    const [exploreId, setExploreId] = useState<string | null>(null);
    const [backlogAddsCount, setBacklogAddsCount] = useState(0);
    const [isInitializing, setIsInitializing] = useState(false);
    const [startTime, setStartTime] = useState<number | null>(null);
    const [loadingAction, setLoadingAction] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);
    const hasAttemptedInit = useRef(false);

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
            } else {
                setError('Impossible de démarrer la session (ID manquant)');
            }
        } catch (e: any) {
            console.error('Failed to initialize explore session', e);
            setError('Erreur lors de l’initialisation de la Zone Derot');
        } finally {
            setIsInitializing(false);
        }
    }, [userId]);

    // Initial load only if we have a user and haven't attempted yet
    useEffect(() => {
        if (userId && !exploreId && !isInitializing && !error && !hasAttemptedInit.current) {
            initExplore();
        }
    }, [userId, exploreId, isInitializing, initExplore, error]);

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
            setExploreId(null);
            setStartTime(null);
        } catch (e) {
            console.error('Failed to stop explore session', e);
        } finally {
            setLoadingAction(null);
        }
    }, [userId, exploreId, startTime, backlogAddsCount]);

    return {
        exploreId,
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
