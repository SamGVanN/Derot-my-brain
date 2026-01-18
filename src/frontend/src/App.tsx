import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { UserService } from './services/UserService';
import { ThemeProvider } from './components/theme-provider';
import { Button } from '@/components/ui/button';
import UserSelectionPage from './pages/UserSelectionPage';
import UserPreferencesPage from './pages/UserPreferencesPage';
import { WelcomePage } from './features/welcome/WelcomePage';
import { HistoryView } from './components/history-view';
import { Layout } from './components/Layout';
import type { User } from './models/User';
import { Settings, Home } from 'lucide-react';

const queryClient = new QueryClient();

function App() {
  const { i18n, t } = useTranslation();
  const [currentUser, setCurrentUser] = useState<User | null>(null);
  const [isInitializing, setIsInitializing] = useState(true);
  const [currentView, setCurrentView] = useState<'home' | 'preferences'>('home');
  const [showWelcome, setShowWelcome] = useState(true);

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
          checkWelcomeStatus();
        } catch (error) {
          console.error('Failed to restore session:', error);
          localStorage.removeItem('userId');
        }
      }
      setIsInitializing(false);
    };

    restoreSession();
  }, []);

  const checkWelcomeStatus = () => {
    const hasSeenWelcome = localStorage.getItem('hasSeenWelcome');
    console.log('[App] Checking welcome status. hasSeenWelcome:', hasSeenWelcome);
    setShowWelcome(hasSeenWelcome !== 'true');
  };

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
    checkWelcomeStatus();
  };

  const handleLogout = () => {
    localStorage.removeItem('userId');
    setCurrentUser(null);
    setCurrentView('home');
    setShowWelcome(false); // Reset welcome state on logout
  };

  const handleUserUpdated = (updatedUser: User) => {
    setCurrentUser(updatedUser);
    // Optionally stay on preferences or go back
  };

  const handleWelcomeProceed = () => {
    setShowWelcome(false);
  };

  const handleWelcomeDismiss = () => {
    localStorage.setItem('hasSeenWelcome', 'true');
    setShowWelcome(false);
  };

  // Render logic simplified to be inside providers
  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        {isInitializing ? (
          <div className="flex items-center justify-center min-h-screen bg-background">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
          </div>
        ) : currentUser ? (
          showWelcome ? (
            <WelcomePage
              user={currentUser}
              onProceed={handleWelcomeProceed}
              onDismiss={handleWelcomeDismiss}
            />
          ) : currentView === 'preferences' ? (
            <div className="relative">
              <Button
                variant="ghost"
                className="absolute top-20 left-4 z-50 md:left-8 gap-2"
                onClick={() => setCurrentView('home')}
              >
                <Home className="h-4 w-4" />
                {t('nav.derot')}
              </Button>
              <UserPreferencesPage user={currentUser} onUserUpdated={handleUserUpdated} />
            </div>
          ) : (
            <Layout>
              <div className="max-w-4xl mx-auto py-8 px-4 space-y-8">
                <div className="flex justify-between items-center">
                  <div className="space-y-1">
                    <h1 className="text-3xl font-bold text-foreground">{t('welcome.title')} {currentUser.name}!</h1>
                    <p className="text-muted-foreground">{t('welcome.intro')}</p>
                  </div>
                  <div className="flex gap-2">
                    <Button
                      variant="outline"
                      onClick={() => setCurrentView('preferences')}
                      className="gap-2"
                    >
                      <Settings className="h-4 w-4" />
                      {t('nav.preferences')}
                    </Button>
                    <Button
                      onClick={handleLogout}
                      variant="outline"
                      className="gap-2"
                    >
                      {t('nav.logout')}
                    </Button>
                  </div>
                </div>
                <HistoryView user={currentUser} />
              </div>
            </Layout>
          )
        ) : (
          <UserSelectionPage onUserSelected={handleUserSelected} />
        )}
      </ThemeProvider>
    </QueryClientProvider>
  );
}

export default App;
