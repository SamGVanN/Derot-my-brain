import { createContext, useContext } from 'react';
import type { User } from '@/models/User';

type UserContextType = {
    currentUser: User | null;
    setCurrentUser: (user: User | null) => void;
    isOnPreferencesPage: boolean;
    setIsOnPreferencesPage: (isOn: boolean) => void;
};

const UserContext = createContext<UserContextType | undefined>(undefined);

export function useUserContext() {
    const context = useContext(UserContext);
    if (!context) {
        throw new Error('useUserContext must be used within UserProvider');
    }
    return context;
}

export { UserContext };
