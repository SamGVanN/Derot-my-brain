import { useTranslation } from 'react-i18next';
import { Layout } from '@/components/Layout';
import { HistoryView } from '@/components/history-view';
import { useAuth } from '@/hooks/useAuth';
import { History } from 'lucide-react';
import { PageHeader } from '@/components/PageHeader';

export function HistoryPage() {
    const { t } = useTranslation();
    const { user } = useAuth();

    if (!user) {
        return null;
    }

    return (
        <Layout>
            <div className="container max-w-5xl mx-auto py-8 px-4 space-y-8 animate-in fade-in duration-700">
                <PageHeader
                    title={t('history.title', 'Learning History')}
                    subtitle={`${t('welcome.title', 'Welcome')} ${user.name}!`}
                    description={t('welcome.intro', 'Track your learning journey')}
                    icon={History}
                />

                <HistoryView user={user} />
            </div>
        </Layout>
    );
}
