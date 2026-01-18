import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { UserService } from './services/UserService';
import { ThemeProvider } from './components/theme-provider';
import { Button } from '@/components/ui/button';
import UserSelectionPage from './pages/UserSelectionPage';
import { HistoryView } from './components/history-view';
import { Layout } from './components/Layout';
import type { User } from './models/User';

const queryClient = new QueryClient();

function App() {
  const { i18n, t } = useTranslation();
  const [currentUser, setCurrentUser] = useState<User | null>(null);
  const [isInitializing, setIsInitializing] = useState(true);

  useEffect(() => {
    const restoreSession = async () => {
      const storedUserId = localStorage.getItem('userId');
      if (storedUserId) {
        try {
          const user = await UserService.getUserById(storedUserId);
          setCurrentUser(user);
          if (user.preferences?.language && user.preferences.language !== 'auto') {
            i18n.changeLanguage(user.preferences.language);
          }
        } catch (error) {
          console.error('Failed to restore session:', error);
          localStorage.removeItem('userId');
        }
      }
      setIsInitializing(false);
    };

    restoreSession();
  }, []);

  useEffect(() => {
    const updatePreferences = async () => {
      if (currentUser && i18n.language && i18n.language !== currentUser.preferences?.language) {
        // Update local state to avoid strict check loops
        const updatedPrefs = { ...currentUser.preferences, language: i18n.language };
        const updatedUser = { ...currentUser, preferences: updatedPrefs };
        setCurrentUser(updatedUser);

        // Persist to backend
        try {
          await UserService.updatePreferences(currentUser.id, updatedPrefs);
        } catch (err) {
          console.error("Failed to persist language preference", err);
        }
      }
    };
    updatePreferences();
  }, [i18n.language, currentUser]);

  const handleUserSelected = (user: User) => {
    localStorage.setItem('userId', user.id);
    setCurrentUser(user);
    if (user.preferences?.language && user.preferences.language !== 'auto') {
      i18n.changeLanguage(user.preferences.language);
    }
  };

  const handleLogout = () => {
    localStorage.removeItem('userId');
    setCurrentUser(null);
  };

  if (isInitializing) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-background">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
      </div>
    );
  }

  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        {currentUser ? (
          <Layout>
            <div className="max-w-4xl mx-auto py-8 px-4 space-y-8">
              <div className="flex justify-between items-center">
                <div className="space-y-1">
                  <h1 className="text-3xl font-bold text-foreground">{t('welcome.title')} {currentUser.name}!</h1>
                  <p className="text-muted-foreground">{t('welcome.intro')}</p>
                </div>
                <Button
                  onClick={handleLogout}
                  variant="outline"
                  className="gap-2"
                >
                  {t('nav.logout')}
                </Button>
              </div>
              <HistoryView user={currentUser} />
            </div>
          </Layout>
        ) : (
          <UserSelectionPage onUserSelected={handleUserSelected} />
        )}
      </ThemeProvider>
    </QueryClientProvider>
  );
}

export default App;
