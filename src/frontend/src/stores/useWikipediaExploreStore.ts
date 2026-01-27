import { create } from 'zustand';
import { type WikipediaArticle } from '../api/wikipediaApi';

interface WikipediaExploreState {
    exploreId: string | null;
    articles: WikipediaArticle[];
    backlogAddsCount: number;
    startTime: number | null;
    error: string | null;

    setExploreId: (id: string | null) => void;
    setArticles: (articles: WikipediaArticle[]) => void;
    setBacklogAddsCount: (count: number | ((prev: number) => number)) => void;
    setStartTime: (time: number | null) => void;
    setError: (error: string | null) => void;
    reset: () => void;
}

export const useWikipediaExploreStore = create<WikipediaExploreState>((set) => ({
    exploreId: null,
    articles: [],
    backlogAddsCount: 0,
    startTime: null,
    error: null,

    setExploreId: (id) => set({ exploreId: id }),
    setArticles: (articles) => set({ articles }),
    setBacklogAddsCount: (count) => set((state) => ({
        backlogAddsCount: typeof count === 'function' ? count(state.backlogAddsCount) : count
    })),
    setStartTime: (time) => set({ startTime: time }),
    setError: (error) => set({ error }),
    reset: () => set({
        exploreId: null,
        articles: [],
        backlogAddsCount: 0,
        startTime: null,
        error: null
    })
}));
