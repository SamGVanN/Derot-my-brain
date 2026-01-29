import { useState, useCallback, useRef } from 'react';
import { activityApi } from '@/api/activityApi';
import { wikipediaApi, type WikipediaArticle } from '@/api/wikipediaApi';
import { useAuthStore } from '@/stores/useAuthStore';
import { useWikipediaExploreStore } from '@/stores/useWikipediaExploreStore';
import { useTranslation } from 'react-i18next';

export function useWikipediaExplore() {
    const { user } = useAuthStore();
    const userId = user?.id;
    const { t } = useTranslation();

    const {
        exploreId, setExploreId,
        sessionId, setSessionId,
        articles, setArticles,
        backlogAddsCount, setBacklogAddsCount,
        refreshCount, setRefreshCount,
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
            setError(t('derot.errors.fetchArticles', 'Erreur lors du chargement des articles'));
        } finally {
            setIsInitializing(false);
        }
    }, [userId, setArticles, setError, t]);

    const initExplore = useCallback(async () => {
        if (!userId) return;

        try {
            setIsInitializing(true);
            setError(null);
            hasAttemptedInit.current = true;

            const data = await activityApi.explore(userId, {
                title: t('derot.explore.defaultTitle', "Wikipedia Exploration"),
                sourceType: 1, // Wikipedia
                sessionId: sessionId || undefined
            });

            if (data?.id) {
                setExploreId(data.id);
                if (data.userSessionId) {
                    setSessionId(data.userSessionId);
                }
                setStartTime(Date.now());
                setBacklogAddsCount(0);
                setRefreshCount(0);
                await fetchArticles();
            } else {
                setError(t('derot.errors.missingId', 'Impossible de démarrer la session (ID manquant)'));
            }
        } catch (e: any) {
            console.error('Failed to initialize explore session', e);
            setError(t('derot.errors.initSession', 'Erreur lors de l’initialisation de la Zone Derot'));
        } finally {
            setIsInitializing(false);
        }
    }, [userId, fetchArticles, setExploreId, setStartTime, setBacklogAddsCount, setError, t, sessionId, setSessionId, setRefreshCount]);


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
                originExploreId: exploreId || undefined,
                backlogAddsCount: backlogAddsCount || undefined,
                refreshCount: refreshCount || undefined,
                exploreDurationSeconds: duration
            });

            // Capture session ID if we're not already tracking one
            if (data?.userSessionId && !sessionId) {
                setSessionId(data.userSessionId);
            }

            return data;
        } catch (e) {
            console.error('Failed to initiate reading', e);
            return null;
        } finally {
            setLoadingAction(null);
        }
    };

    const stopExplore = useCallback(async () => {
        if (!userId) return;

        // If we have an exploration activity, stop it explicitly
        if (exploreId && startTime) {
            try {
                setLoadingAction('stop-explore');
                const duration = Math.floor((Date.now() - startTime) / 1000);
                await activityApi.stopExplore(userId, exploreId, {
                    durationSeconds: duration,
                    backlogAddsCount,
                    refreshCount
                });
            } catch (e) {
                console.error('Failed to stop explore activity', e);
            }
        }

        // ALWAYS stop the backend session if we have one
        if (sessionId) {
            try {
                await activityApi.stopSession(userId, sessionId);
            } catch (e) {
                console.error('Failed to stop session', e);
            }
        }

        reset();
        setLoadingAction(null);
    }, [userId, exploreId, startTime, backlogAddsCount, sessionId, reset]);

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
        initExplore,
        refresh: async () => {
            setRefreshCount(c => c + 1);
            await fetchArticles();
        }
    };
}
