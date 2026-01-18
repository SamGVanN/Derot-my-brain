import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useState } from 'react';
import { ThemeProvider } from './components/theme-provider';
import UserSelectionPage from './pages/UserSelectionPage';
import { HistoryView } from './components/history-view';
import { Layout } from './components/Layout';
import type { User } from './models/User';

const queryClient = new QueryClient();

function App() {
  const [currentUser, setCurrentUser] = useState<User | null>(null);

  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        {currentUser ? (
          <Layout>
            <div className="max-w-4xl mx-auto py-8 px-4 space-y-8">
              <div className="flex justify-between items-center">
                <div className="space-y-1">
                  <h1 className="text-3xl font-bold text-white">Welcome back, {currentUser.name}!</h1>
                  <p className="text-white/60">Manage your brain and track your progress.</p>
                </div>
                <button
                  onClick={() => setCurrentUser(null)}
                  className="px-4 py-2 rounded-lg bg-white/10 hover:bg-white/20 text-white transition-colors border border-white/10"
                >
                  Switch User
                </button>
              </div>
              <HistoryView user={currentUser} />
            </div>
          </Layout>
        ) : (
          <UserSelectionPage onUserSelected={setCurrentUser} />
        )}
      </ThemeProvider>
    </QueryClientProvider>
  );
}

export default App;
