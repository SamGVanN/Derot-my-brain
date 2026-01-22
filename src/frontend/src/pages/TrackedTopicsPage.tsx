import { useTranslation } from 'react-i18next';
import { Link } from 'react-router';
import { Layout } from '@/components/Layout';
import { Button } from '@/components/ui/button';
import { ArrowLeft, Brain } from 'lucide-react';

export function TrackedTopicsPage() {
    const { t } = useTranslation();

    return (
        <Layout>
            <div className="container max-w-4xl mx-auto py-12 px-4">
                <div className="flex flex-col items-center justify-center min-h-[60vh] space-y-6 text-center">
                    <div className="relative">
                        <Brain className="h-24 w-24 text-primary animate-pulse" />
                        <div className="absolute inset-0 h-24 w-24 bg-primary/20 rounded-full blur-xl animate-pulse" />
                    </div>

                    <div className="space-y-2">
                        <h1 className="text-4xl font-bold tracking-tight">
                            {t('trackedTopics.title', 'Tracked Topics')}
                        </h1>
                        <p className="text-xl text-muted-foreground">
                            {t('trackedTopics.comingSoon', 'Coming soon in Phase 5')}
                        </p>
                    </div>

                    <p className="text-muted-foreground max-w-md">
                        {t('trackedTopics.description', 'This is where you will manage your tracked topics and favorite articles to revisit later.')}
                    </p>

                    <Button asChild variant="outline" className="gap-2">
                        <Link to="/history">
                            <ArrowLeft className="h-4 w-4" />
                            {t('common.backToHistory', 'Back to History')}
                        </Link>
                    </Button>
                </div>
            </div>
        </Layout>
    );
}
