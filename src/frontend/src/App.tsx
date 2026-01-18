import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useEffect } from 'react';
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
import { UserContext } from './contexts/UserContext';
import { useAuthStore } from './stores/useAuthStore';
import { usePreferencesStore } from './stores/usePreferencesStore';

const queryClient = new QueryClient();



function AppContentRefactored() {
  const { i18n, t } = useTranslation();

  // Auth Store
  const { user, login, logout, updateUser, isInitializing, setInitializing } = useAuthStore();

  // Preferences Store
  const { hasSeenWelcome, setHasSeenWelcome, setPreferences } = usePreferencesStore();

  // View State
  const [currentView, setCurrentView] = useState<'home' | 'preferences'>('home');
  const [isOnPreferencesPage, setIsOnPreferencesPage] = useState(false);

  // Restore Session (Validation against backend)
  useEffect(() => {
    const validateSession = async () => {
      if (user) {
        try {
          // Validate if user still exists in backend
          const refreshedUser = await UserService.getUserById(user.id);
          // Update store with fresh data
          login(refreshedUser);
          // Sync preferences
          if (refreshedUser.preferences) {
            setPreferences(refreshedUser.preferences);
            // i18n sync
            if (refreshedUser.preferences.language && refreshedUser.preferences.language !== 'auto') {
              i18n.changeLanguage(refreshedUser.preferences.language);
            }
          }
        } catch (error) {
          console.error('Session validation failed:', error);
          logout();
        }
      }
      setInitializing(false);
    };

    validateSession();
  }, [user?.id]); // Only runs on mount if user is persisted, or if ID changes

  const handleUserSelected = (selectedUser: User) => {
    login(selectedUser);

    // Apply preferences immediately
    if (selectedUser.preferences) {
      setPreferences(selectedUser.preferences);
      if (selectedUser.preferences.language && selectedUser.preferences.language !== 'auto') {
        i18n.changeLanguage(selectedUser.preferences.language);
      }
    }
  };

  const handleLogout = () => {
    logout();
    setCurrentView('home');
    // Note: We do NOT reset preferences here, reverting to "Anonymous" state (last active settings)
  };

  const handleUserUpdated = (updatedUser: User) => {
    updateUser(updatedUser);

    // Update preferences store
    if (updatedUser.preferences) {
      setPreferences(updatedUser.preferences);
      if (updatedUser.preferences.language && updatedUser.preferences.language !== 'auto') {
        i18n.changeLanguage(updatedUser.preferences.language);
      }
    }
  };

  const handlePreferencesOpen = () => {
    setCurrentView('preferences');
    setIsOnPreferencesPage(true);
  };

  const handlePreferencesClose = () => {
    setCurrentView('home');
    setIsOnPreferencesPage(false);
  };

  // Welcome Page Logic
  const showWelcome = !hasSeenWelcome;



  // Re-implementing logic with local override
  const [welcomeHiddenSession, setWelcomeHiddenSession] = useState(false);
  const shouldDisplayWelcome = showWelcome && !welcomeHiddenSession;

  return (
    <UserContext.Provider value={{ currentUser: user, setCurrentUser: updateUser as any, isOnPreferencesPage, setIsOnPreferencesPage }}>
      {isInitializing ? (
        <div className="flex items-center justify-center min-h-screen bg-background">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
        </div>
      ) : user ? (
        shouldDisplayWelcome ? (
          <WelcomePage
            user={user}
            onProceed={() => setWelcomeHiddenSession(true)}
            onDismiss={() => setHasSeenWelcome(true)}
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
              user={user}
              onUserUpdated={handleUserUpdated}
              onCancel={handlePreferencesClose}
            />
          </div>
        ) : (
          <Layout>
            <div className="max-w-4xl mx-auto py-8 px-4 space-y-8">
              <div className="flex justify-between items-center">
                <div className="space-y-1">
                  <h1 className="text-3xl font-bold text-foreground">{t('welcome.title')} {user.name}!</h1>
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
              <HistoryView user={user} />
            </div>
          </Layout>
        )
      ) : (
        <UserSelectionPage onUserSelected={handleUserSelected} />
      )}
    </UserContext.Provider>
  );
}

// Need to define useState outside in the real function
import { useState } from 'react';

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        <AppContentRefactored />
      </ThemeProvider>
    </QueryClientProvider>
  );
}

export default App;

