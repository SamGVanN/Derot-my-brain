import { useTranslation } from 'react-i18next';
import { Link } from 'react-router';
import { Layout } from '@/components/Layout';
import { Button } from '@/components/ui/button';
import { ArrowLeft, Library } from 'lucide-react';
import { PageHeader } from '@/components/PageHeader';

export function DocumentsPage() {
    const { t } = useTranslation();

    return (
        <Layout>
            <div className="container max-w-4xl mx-auto py-12 px-4">
                <PageHeader
                    title={t('documents.title', 'Documents')}
                    subtitle={t('documents.comingSoon', 'Coming soon in Phase 8')}
                    description={t('documents.description', 'This is where you will manage your uploaded documents : organize, tag, anotate, and create activities from them.')}
                    icon={Library}
                />

                <div className="flex flex-col items-center justify-center py-20 space-y-6 text-center">
                    <div className="relative">
                        <Library className="h-24 w-24 text-primary animate-pulse" />
                        <div className="absolute inset-0 h-24 w-24 bg-primary/20 rounded-full blur-xl animate-pulse" />
                    </div>

                    <Button asChild variant="outline" className="gap-2">
                        <Link to="/homepage">
                            <ArrowLeft className="h-4 w-4" />
                            {t('common.backToHomepage', 'Back to Homepage')}
                        </Link>
                    </Button>
                </div>
            </div>
        </Layout>
    );
}
