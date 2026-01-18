import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useState, useEffect, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { UserService } from './services/UserService';
import { ThemeProvider, useTheme } from './components/theme-provider';
import { Button } from '@/components/ui/button';
import UserSelectionPage from './pages/UserSelectionPage';
import UserPreferencesPage from './pages/UserPreferencesPage';
import { WelcomePage } from './features/welcome/WelcomePage';
import { HistoryView } from './components/history-view';
import { Layout } from './components/Layout';
import type { User } from './models/User';
import { Settings, Home } from 'lucide-react';
import { UserContext } from './contexts/UserContext';

const queryClient = new QueryClient();

function AppContent() {
  const { i18n, t } = useTranslation();
  const { setTheme } = useTheme();
  const [currentUser, setCurrentUser] = useState<User | null>(null);
  const [isInitializing, setIsInitializing] = useState(true);
  const [currentView, setCurrentView] = useState<'home' | 'preferences'>('home');
  const [showWelcome, setShowWelcome] = useState(true);
  const [isOnPreferencesPage, setIsOnPreferencesPage] = useState(false);

  const sessionRestored = useRef(false);

  useEffect(() => {
    if (sessionRestored.current) return;

    const restoreSession = async () => {
      sessionRestored.current = true;
      const storedUserId = localStorage.getItem('userId');
      if (storedUserId) {
        try {
          const user = await UserService.getUserById(storedUserId);
          setCurrentUser(user);
          if (user.preferences?.language && user.preferences.language !== 'auto') {
            i18n.changeLanguage(user.preferences.language);
          }
          if (user.preferences?.preferredTheme) {
            setTheme(user.preferences.preferredTheme);
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
  }, [i18n, setTheme]);

  const checkWelcomeStatus = () => {
    const hasSeenWelcome = localStorage.getItem('hasSeenWelcome');
    setShowWelcome(hasSeenWelcome !== 'true');
  };

  const handleUserSelected = (user: User) => {
    localStorage.setItem('userId', user.id);
    setCurrentUser(user);
    if (user.preferences?.language && user.preferences.language !== 'auto') {
      i18n.changeLanguage(user.preferences.language);
    }
    if (user.preferences?.preferredTheme) {
      setTheme(user.preferences.preferredTheme);
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

  const handlePreferencesOpen = () => {
    setCurrentView('preferences');
    setIsOnPreferencesPage(true);
  };

  const handlePreferencesClose = () => {
    setCurrentView('home');
    setIsOnPreferencesPage(false);
  };

  const handleWelcomeProceed = () => {
    setShowWelcome(false);
  };

  const handleWelcomeDismiss = () => {
    localStorage.setItem('hasSeenWelcome', 'true');
    setShowWelcome(false);
  };

  return (
    <UserContext.Provider value={{ currentUser, setCurrentUser, isOnPreferencesPage, setIsOnPreferencesPage }}>
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
              onClick={handlePreferencesClose}
            >
              <Home className="h-4 w-4" />
              {t('nav.derot')}
            </Button>
            <UserPreferencesPage
              user={currentUser}
              onUserUpdated={handleUserUpdated}
              onCancel={handlePreferencesClose}
            />
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
                    onClick={handlePreferencesOpen}
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
    </UserContext.Provider>
  );
}

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        <AppContent />
      </ThemeProvider>
    </QueryClientProvider>
  );
}

export default App;
