import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useEffect } from 'react';
import { Routes, Route, Navigate, useLocation } from 'react-router';
import { useTranslation } from 'react-i18next';
import { ThemeProvider } from './components/theme-provider';
import UserSelectionPage from './pages/UserSelectionPage';
import UserPreferencesPage from './pages/UserPreferencesPage';
import { UserProfilePage } from './pages/UserProfilePage';
import { HistoryPage } from './pages/HistoryPage';
import { DerotPage } from './pages/DerotPage';
import { TrackedTopicsPage } from './pages/TrackedTopicsPage';
import { GuidePage } from './pages/GuidePage';
import { WelcomePage } from './features/welcome/WelcomePage';
import { useAuth } from './hooks/useAuth';
import { usePreferences } from './hooks/usePreferences';
import type { User } from './models/User';

const queryClient = new QueryClient();

// Protected Route Component
function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { user, isInitializing } = useAuth();
  const location = useLocation();

  if (isInitializing) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-background">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
      </div>
    );
  }

  if (!user) {
    return <Navigate to="/" state={{ from: location }} replace />;
  }

  return <>{children}</>;
}

// Welcome Route Component (shows welcome page if needed)
function WelcomeRoute({ children }: { children: React.ReactNode }) {
  const { user } = useAuth();
  const { hasSeenWelcome, setHasSeenWelcome } = usePreferences();

  if (user && !hasSeenWelcome) {
    return (
      <WelcomePage
        user={user}
        onProceed={() => {/* Welcome dismissed for session */ }}
        onDismiss={() => setHasSeenWelcome(true)}
      />
    );
  }

  return <>{children}</>;
}

function AppContent() {
  const { i18n } = useTranslation();
  const { user, login, validateSession, isInitializing } = useAuth();
  const { language } = usePreferences();

  // Initialize Session
  useEffect(() => {
    validateSession();
  }, [validateSession]);

  // Sync Language with Store
  useEffect(() => {
    if (language && language !== 'auto' && language !== i18n.language) {
      i18n.changeLanguage(language);
    }
  }, [language, i18n]);

  const handleUserSelected = (selectedUser: User) => {
    login(selectedUser);
  };

  // Loading State
  if (isInitializing) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-background">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
      </div>
    );
  }

  return (
    <Routes>
      {/* Public Route - Login */}
      <Route
        path="/"
        element={
          user ? (
            <Navigate to="/history" replace />
          ) : (
            <UserSelectionPage onUserSelected={handleUserSelected} />
          )
        }
      />

      {/* Protected Routes */}
      <Route
        path="/history"
        element={
          <ProtectedRoute>
            <WelcomeRoute>
              <HistoryPage />
            </WelcomeRoute>
          </ProtectedRoute>
        }
      />

      <Route
        path="/profile"
        element={
          <ProtectedRoute>
            <UserProfilePage />
          </ProtectedRoute>
        }
      />

      <Route
        path="/preferences"
        element={
          <ProtectedRoute>
            <div className="min-h-screen bg-background text-foreground animate-in fade-in slide-in-from-bottom-4 duration-500">
              <UserPreferencesPage
                user={user!}
                onUserUpdated={login}
                onCancel={() => window.history.back()}
              />
            </div>
          </ProtectedRoute>
        }
      />

      <Route
        path="/derot"
        element={
          <ProtectedRoute>
            <DerotPage />
          </ProtectedRoute>
        }
      />

      <Route
        path="/tracked-topics"
        element={
          <ProtectedRoute>
            <TrackedTopicsPage />
          </ProtectedRoute>
        }
      />

      <Route
        path="/guide"
        element={<GuidePage />}
      />

      {/* Fallback - Redirect to home */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
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
