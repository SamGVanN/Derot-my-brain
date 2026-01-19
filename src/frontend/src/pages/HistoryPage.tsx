import { useTranslation } from 'react-i18next';
import { Layout } from '@/components/Layout';
import { HistoryView } from '@/components/history-view';
import { useAuth } from '@/hooks/useAuth';

export function HistoryPage() {
    const { t } = useTranslation();
    const { user } = useAuth();

    if (!user) {
        return null;
    }

    return (
        <Layout>
            <div className="container max-w-5xl mx-auto py-8 px-4 space-y-8 animate-in fade-in duration-700">
                <div className="flex flex-col gap-4 p-6 bg-card/50 rounded-xl border shadow-sm backdrop-blur-sm">
                    <div className="space-y-1">
                        <h1 className="text-3xl font-bold tracking-tight bg-clip-text text-transparent bg-gradient-to-r from-primary to-violet-500">
                            {t('welcome.title', 'Welcome')} {user.name}!
                        </h1>
                        <p className="text-muted-foreground">{t('welcome.intro', 'Track your learning journey')}</p>
                    </div>
                </div>

                <HistoryView user={user} />
            </div>
        </Layout>
    );
}
