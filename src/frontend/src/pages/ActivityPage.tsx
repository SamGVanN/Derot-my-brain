import { Layout } from '@/components/Layout';
import { ActivityView } from '@/components/ActivityView';

export function ActivityPage() {
    return (
        <Layout>
            <div className="container h-[calc(100vh-4rem)] max-w-7xl mx-auto py-6 px-4 animate-in fade-in duration-500">
                <ActivityView />
            </div>
        </Layout>
    );
}
