import { create } from 'zustand';
import { type WikipediaArticle } from '../api/wikipediaApi';

interface WikipediaExploreState {
    exploreId: string | null;
    sessionId: string | null;
    articles: WikipediaArticle[];
    backlogAddsCount: number;
    refreshCount: number;
    startTime: number | null;
    error: string | null;

    setExploreId: (id: string | null) => void;
    setSessionId: (id: string | null) => void;
    setArticles: (articles: WikipediaArticle[]) => void;
    setBacklogAddsCount: (count: number | ((prev: number) => number)) => void;
    setRefreshCount: (count: number | ((prev: number) => number)) => void;
    setStartTime: (time: number | null) => void;
    setError: (error: string | null) => void;
    reset: () => void;
}

export const useWikipediaExploreStore = create<WikipediaExploreState>((set) => ({
    exploreId: null,
    sessionId: null,
    articles: [],
    backlogAddsCount: 0,
    refreshCount: 0,
    startTime: null,
    error: null,

    setExploreId: (id) => set({ exploreId: id }),
    setSessionId: (id) => set({ sessionId: id }),
    setArticles: (articles) => set({ articles }),
    setBacklogAddsCount: (count) => set((state) => ({
        backlogAddsCount: typeof count === 'function' ? count(state.backlogAddsCount) : count
    })),
    setRefreshCount: (count) => set((state) => ({
        refreshCount: typeof count === 'function' ? count(state.refreshCount) : count
    })),
    setStartTime: (time) => set({ startTime: time }),
    setError: (error) => set({ error }),
    reset: () => set({
        exploreId: null,
        sessionId: null,
        articles: [],
        backlogAddsCount: 0,
        refreshCount: 0,
        startTime: null,
        error: null
    })
}));
