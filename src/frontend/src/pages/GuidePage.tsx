import { useTranslation } from 'react-i18next';
import { Link } from 'react-router';
import { Layout } from '@/components/Layout';
import { Button } from '@/components/ui/button';
import { ArrowLeft, BookOpen } from 'lucide-react';

export function GuidePage() {
    const { t } = useTranslation();

    return (
        <Layout>
            <div className="container max-w-4xl mx-auto py-12 px-4">
                <div className="flex flex-col items-center justify-center min-h-[60vh] space-y-6 text-center">
                    <div className="relative">
                        <BookOpen className="h-24 w-24 text-primary animate-pulse" />
                        <div className="absolute inset-0 h-24 w-24 bg-primary/20 rounded-full blur-xl animate-pulse" />
                    </div>

                    <div className="space-y-2">
                        <h1 className="text-4xl font-bold tracking-tight">
                            {t('guide.title', 'User Guide')}
                        </h1>
                        <p className="text-xl text-muted-foreground">
                            {t('guide.comingSoon', 'Coming soon in Phase 8')}
                        </p>
                    </div>

                    <p className="text-muted-foreground max-w-md">
                        {t('guide.description', 'This is where you will find helpful guides and tutorials on how to use Derot My Brain.')}
                    </p>

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
