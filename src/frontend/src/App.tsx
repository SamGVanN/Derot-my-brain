import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { ThemeProvider } from './components/theme-provider';
import { Button } from '@/components/ui/button';
import UserSelectionPage from './pages/UserSelectionPage';
import UserPreferencesPage from './pages/UserPreferencesPage';
import { WelcomePage } from './features/welcome/WelcomePage';
import { HistoryView } from './components/history-view';
import { Layout } from './components/Layout';
import type { User } from './models/User';
import { Settings, LogOut } from 'lucide-react';
import { useAuth } from './hooks/useAuth';
import { usePreferences } from './hooks/usePreferences';


const queryClient = new QueryClient();

function AppContent() {
  const { t, i18n } = useTranslation();

  // Custom Hooks
  const { user, login, logout, validateSession, isInitializing } = useAuth();
  const { hasSeenWelcome, setHasSeenWelcome, language } = usePreferences();

  // Local View State (Routing substitute)
  const [currentView, setCurrentView] = useState<'home' | 'preferences'>('home');
  const [welcomeHiddenSession, setWelcomeHiddenSession] = useState(false);

  // Initialize Session
  useEffect(() => {
    validateSession();
  }, [validateSession]);

  // Sync Language with Store (e.g. after login or preference change)
  useEffect(() => {
    if (language && language !== 'auto' && language !== i18n.language) {
      i18n.changeLanguage(language);
    }
  }, [language, i18n]);

  // Derived State
  const shouldDisplayWelcome = user && !hasSeenWelcome && !welcomeHiddenSession;

  // Handlers
  const handleUserSelected = (selectedUser: User) => {
    login(selectedUser);
  };

  const handleLogout = () => {
    logout();
    setCurrentView('home');
    setWelcomeHiddenSession(false);
  };

  const handleNavigate = (view: 'home' | 'preferences') => {
    setCurrentView(view);
  };

  // Loading State
  if (isInitializing) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-background">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
      </div>
    );
  }

  // Not Authenticated
  if (!user) {
    return <UserSelectionPage onUserSelected={handleUserSelected} />;
  }

  // Welcome Screen
  if (shouldDisplayWelcome) {
    return (
      <WelcomePage
        user={user}
        onProceed={() => setWelcomeHiddenSession(true)}
        onDismiss={() => setHasSeenWelcome(true)}
      />
    );
  }

  // Preferences Page
  if (currentView === 'preferences') {
    return (
      <div className="min-h-screen bg-background text-foreground animate-in fade-in slide-in-from-bottom-4 duration-500">
        <UserPreferencesPage
          user={user}
          onUserUpdated={login} // Helper to update store if needed, though hook handles it
          onCancel={() => handleNavigate('home')}
        />
      </div>
    );
  }

  // Main Dashboard (Home)
  return (
    <Layout>
      <div className="container max-w-5xl mx-auto py-8 px-4 space-y-8 animate-in fade-in duration-700">

        {/* Dashboard Header */}
        <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 p-6 bg-card/50 rounded-xl border shadow-sm backdrop-blur-sm">
          <div className="space-y-1">
            <h1 className="text-3xl font-bold tracking-tight bg-clip-text text-transparent bg-gradient-to-r from-primary to-violet-500">
              {t('welcome.title')} {user.name}!
            </h1>
            <p className="text-muted-foreground">{t('welcome.intro')}</p>
          </div>

          <div className="flex gap-2 w-full md:w-auto">
            <Button
              variant="outline"
              onClick={() => handleNavigate('preferences')}
              className="gap-2 flex-1 md:flex-none shadow-sm hover:shadow-md transition-all"
            >
              <Settings className="h-4 w-4" />
              {t('nav.preferences')}
            </Button>
            <Button
              onClick={handleLogout}
              variant="ghost"
              className="gap-2 flex-1 md:flex-none text-muted-foreground hover:text-destructive hover:bg-destructive/10"
            >
              <LogOut className="h-4 w-4" />
              {t('nav.logout')}
            </Button>
          </div>
        </div>

        {/* Main Content Area - History View */}
        <HistoryView user={user} />

      </div>
    </Layout>
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

