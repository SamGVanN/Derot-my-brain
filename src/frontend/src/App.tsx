import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useEffect } from 'react';
import { Routes, Route, Navigate, useLocation } from 'react-router';
import { useTranslation } from 'react-i18next';
import { ThemeProvider } from './components/theme-provider';
import UserSelectionPage from './pages/UserSelectionPage';
import LLMConfigurationPage from './pages/LLMConfigurationPage';
import { PreferencesPage } from './pages/PreferencesPage';
import { UserProfilePage } from './pages/UserProfilePage';
import { HistoryPage } from './pages/HistoryPage';
import { DerotPage } from './pages/DerotPage';
import { MyFocusAreaPage } from './pages/MyFocusAreaPage';
import { GuidePage } from './pages/GuidePage';
import { WelcomePage } from './features/welcome/WelcomePage';
import { useAuth } from './hooks/useAuth';
import { usePreferences } from './hooks/usePreferences';
import type { User } from './models/User';
import { HomepagePage } from './pages/HomePage';
import { DocumentsPage } from './pages/DocumentsPage';
import { BacklogPage } from './pages/BacklogPage';
import { DashboardPage } from './pages/DashboardPage';

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

function WelcomeRoute({ children }: { children: React.ReactNode }) {
  const { user } = useAuth();
  const { hasSeenWelcome, setHasSeenWelcome, sessionWelcomeDismissed, dismissWelcomeSession } = usePreferences();

  if (user && !hasSeenWelcome && !sessionWelcomeDismissed) {
    return (
      <WelcomePage
        user={user}
        onProceed={dismissWelcomeSession}
        onDismiss={() => setHasSeenWelcome(true)}
      />
    );
  }

  return <>{children}</>;
}

function AppContent() {
  const { i18n } = useTranslation();
  const { user, login, validateSession, isInitializing, updateUser } = useAuth();


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

  const handleUserSelected = (data: { user: User; token: string }) => {
    login(data);
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
            <Navigate to="/homepage" replace />
          ) : (
            <UserSelectionPage onUserSelected={handleUserSelected} />
          )
        }
      />

      {/* Protected Routes */}



      <Route
        path="/homepage"
        element={<HomepagePage />}
      />

      <Route
        path="/guide"
        element={<GuidePage />}
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
        path="/focus-area"
        element={
          <ProtectedRoute>
            <MyFocusAreaPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/documents"
        element={
          <ProtectedRoute>
            <DocumentsPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/backlog"
        element={
          <ProtectedRoute>
            <BacklogPage />
          </ProtectedRoute>
        }
      />

      <Route
        path="/dashboard"
        element={
          <ProtectedRoute>
            <DashboardPage />
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
            <PreferencesPage
              user={user!}
              onUserUpdated={updateUser}
            />
          </ProtectedRoute>
        }
      />

      <Route
        path="/configuration"
        element={
          <ProtectedRoute>
            <div className="min-h-screen bg-background text-foreground animate-in fade-in slide-in-from-bottom-4 duration-500">
              <LLMConfigurationPage
                user={user!}
                onUserUpdated={updateUser}
                onCancel={() => window.history.back()}
              />
            </div>
          </ProtectedRoute>
        }
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
