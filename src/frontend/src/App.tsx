import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ThemeProvider } from './components/theme-provider';
import UserSelectionPage from './pages/UserSelectionPage';

const queryClient = new QueryClient();

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        <UserSelectionPage />
      </ThemeProvider>
    </QueryClientProvider>
  );
}

export default App;
