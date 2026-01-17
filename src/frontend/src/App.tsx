import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import UserSelectionPage from './pages/UserSelectionPage';

const queryClient = new QueryClient();

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <UserSelectionPage />
    </QueryClientProvider>
  );
}

export default App;
