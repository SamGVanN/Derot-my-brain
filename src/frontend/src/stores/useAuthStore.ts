import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { User } from '../models/User';

interface AuthState {
    user: User | null;
    isAuthenticated: boolean;
    isInitializing: boolean;
    setInitializing: (isInitializing: boolean) => void;
    login: (user: User) => void;
    logout: () => void;
    updateUser: (user: User) => void;
}

export const useAuthStore = create<AuthState>()(
    persist(
        (set) => ({
            user: null,
            isAuthenticated: false,
            isInitializing: true,
            setInitializing: (isInitializing) => set({ isInitializing }),
            login: (user) => set({ user, isAuthenticated: true }),
            logout: () => set({ user: null, isAuthenticated: false }),
            updateUser: (user) => set({ user }),
        }),
        {
            name: 'auth-storage',
            partialize: (state) => ({ user: state.user, isAuthenticated: state.isAuthenticated }),
        }
    )
);
