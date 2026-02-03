import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { Layout } from '@/components/Layout';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { PageHeader } from '@/components/PageHeader';
import { Home, Bug, Github, Activity, BookOpen, Layers, CheckCircle } from 'lucide-react';
import { sessionApi } from '@/api/sessionApi';
import { useAuth } from '@/hooks/useAuth';
import { ActivityTypes } from '@/models/Enums';
// import { Link } from 'react-router'; // Unused if using asChild anchor

export function HomepagePage() {
    const { t } = useTranslation();
    const { user } = useAuth();

    const { data: sessions, isLoading, isError } = useQuery({
        queryKey: ['sessions', user?.id],
        queryFn: () => sessionApi.getSessions(user!.id),
        enabled: !!user?.id,
    });

    // Compute stats
    const last3Sessions = sessions
        ?.sort((a, b) => new Date(b.startedAt).getTime() - new Date(a.startedAt).getTime())
        .slice(0, 3) || [];

    const stats = {
        explorations: 0,
        readings: 0,
        quizzes: 0,
        backlogAdds: 0,
        totalQuizScore: 0,
        quizCountForAvg: 0
    };

    last3Sessions.forEach(session => {
        session.activities.forEach(activity => {
            if (activity.type === ActivityTypes.Explore) {
                stats.explorations++;
                if (activity.backlogAddsCount) {
                    stats.backlogAdds += activity.backlogAddsCount;
                }
            } else if (activity.type === ActivityTypes.Read) {
                stats.readings++;
            } else if (activity.type === ActivityTypes.Quiz) {
                stats.quizzes++;
                if (activity.scorePercentage !== undefined) {
                    stats.totalQuizScore += activity.scorePercentage;
                    stats.quizCountForAvg++;
                }
            }
        });
    });

    const avgScore = stats.quizCountForAvg > 0
        ? Math.round(stats.totalQuizScore / stats.quizCountForAvg)
        : null;

    return (
        <Layout>
            <div className="container max-w-4xl mx-auto py-8 space-y-8 animate-in fade-in duration-500">
                <PageHeader
                    title={t('homepage.title')}
                    icon={Home}
                    description={t('homepage.description')}
                />

                <div className="grid gap-6 md:grid-cols-2">
                    {/* Stats Card */}
                    <Card className="col-span-2 shadow-sm">
                        <CardHeader>
                            <CardTitle className="flex items-center gap-2">
                                <Activity className="h-5 w-5 text-primary" />
                                {t('homepage.stats.title')}
                            </CardTitle>
                        </CardHeader>
                        <CardContent>
                            {isLoading ? (
                                <div className="text-center py-4 text-muted-foreground">{t('common.loading')}</div>
                            ) : isError ? (
                                <div className="text-center py-4 text-destructive">{t('common.error')}</div>
                            ) : last3Sessions.length === 0 ? (
                                <div className="text-center py-4 text-muted-foreground">{t('history.empty')}</div>
                            ) : (
                                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                                    <div className="flex flex-col items-center justify-center p-4 bg-muted/40 rounded-lg border border-border/50">
                                        <div className="flex items-center gap-2 mb-2 text-muted-foreground">
                                            <Layers className="h-4 w-4" />
                                            <span className="text-sm font-medium">{t('homepage.stats.explorations')}</span>
                                        </div>
                                        <span className="text-2xl font-bold">{stats.explorations}</span>
                                    </div>
                                    <div className="flex flex-col items-center justify-center p-4 bg-muted/40 rounded-lg border border-border/50">
                                        <div className="flex items-center gap-2 mb-2 text-muted-foreground">
                                            <BookOpen className="h-4 w-4" />
                                            <span className="text-sm font-medium">{t('homepage.stats.readings')}</span>
                                        </div>
                                        <span className="text-2xl font-bold">{stats.readings}</span>
                                    </div>
                                    <div className="flex flex-col items-center justify-center p-4 bg-muted/40 rounded-lg border border-border/50">
                                        <div className="flex items-center gap-2 mb-2 text-muted-foreground">
                                            <CheckCircle className="h-4 w-4" />
                                            <span className="text-sm font-medium">{t('homepage.stats.quizzes')}</span>
                                        </div>
                                        <span className="text-2xl font-bold">{stats.quizzes}</span>
                                    </div>
                                    <div className="flex flex-col items-center justify-center p-4 bg-muted/40 rounded-lg border border-border/50">
                                        <div className="flex items-center gap-2 mb-2 text-muted-foreground">
                                            <span className="text-sm font-medium">{t('homepage.stats.avgScore')}</span>
                                        </div>
                                        <span className="text-2xl font-bold">
                                            {avgScore !== null ? `${avgScore}%` : '-'}
                                        </span>
                                    </div>

                                    <div className="flex flex-col col-span-2 md:col-span-4 items-center justify-center p-2 bg-secondary/20 rounded-lg mt-2 border border-secondary/20">
                                        <span className="text-sm text-foreground/80">
                                            {t('homepage.stats.backlog')}: <span className="font-bold text-foreground">{stats.backlogAdds}</span>
                                        </span>
                                    </div>
                                </div>
                            )}
                        </CardContent>
                    </Card>

                    {/* Actions */}
                    <Card className="shadow-sm hover:shadow-md transition-shadow">
                        <CardHeader>
                            <CardTitle>{t('homepage.actions.github')}</CardTitle>
                        </CardHeader>
                        <CardContent className="flex justify-center">
                            <Button variant="outline" className="w-full gap-2" asChild>
                                <a href="https://github.com/SamGVanN/Derot-my-brain" target="_blank" rel="noopener noreferrer">
                                    <Github className="h-4 w-4" />
                                    GitHub Repo
                                </a>
                            </Button>
                        </CardContent>
                    </Card>

                    <Card className="shadow-sm hover:shadow-md transition-shadow">
                        <CardHeader>
                            <CardTitle>{t('homepage.actions.bug')}</CardTitle>
                        </CardHeader>
                        <CardContent className="flex justify-center">
                            <Button variant="secondary" className="w-full gap-2 text-destructive-foreground bg-destructive/10 hover:bg-destructive/20 border-destructive/20 hover:border-destructive/30" asChild>
                                <a href="https://github.com/SamGVanN/Derot-my-brain/issues/new" target="_blank" rel="noopener noreferrer">
                                    <Bug className="h-4 w-4 text-destructive" />
                                    {t('homepage.actions.bug')}
                                </a>
                            </Button>
                        </CardContent>
                    </Card>
                </div>
            </div>
        </Layout>
    );
}
