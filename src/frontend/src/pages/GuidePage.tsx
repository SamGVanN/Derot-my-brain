import { useTranslation } from 'react-i18next';
import { Layout } from '@/components/Layout';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { PageHeader } from '@/components/PageHeader';
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { BookOpen, AlertTriangle, Monitor, Cpu, ImageIcon, Compass, GraduationCap, Settings, Radar, Brain, Library, FileText, Layers, ClockAlert, BookOpenTextIcon, BookOpenText } from 'lucide-react';

export function GuidePage() {
    const { t } = useTranslation();

    return (
        <Layout>
            <div className="container max-w-4xl mx-auto py-8 space-y-8 animate-in fade-in duration-500">
                <PageHeader
                    title={t('nav.guide')}
                    subtitle={t('guide.title')}
                    icon={BookOpen}
                    badgeIcon={BookOpen}
                    description={t('guide.description')}
                />

                <Tabs defaultValue="usage" className="w-full">
                    <TabsList className="grid w-full grid-cols-3 mb-8 h-10 bg-muted/20 p-1 rounded-xl">
                        <TabsTrigger
                            value="usage"
                            className="text-sm gap-2 data-[state=active]:bg-secondary data-[state=active]:text-secondary-foreground data-[state=active]:shadow-xl data-[state=active]:scale-[1.02] transition-all duration-300 ease-out data-[state=inactive]:text-muted-foreground data-[state=inactive]:hover:bg-muted/40 rounded-lg"
                        >
                            <Compass className="w-4 h-4" />
                            {t('guide.tabs.usage')}
                        </TabsTrigger>
                        <TabsTrigger
                            value="library"
                            className="text-sm gap-2 data-[state=active]:bg-secondary data-[state=active]:text-secondary-foreground data-[state=active]:shadow-xl data-[state=active]:scale-[1.02] transition-all duration-300 ease-out data-[state=inactive]:text-muted-foreground data-[state=inactive]:hover:bg-muted/40 rounded-lg"
                        >
                            <Library className="w-4 h-4" />
                            {t('guide.tabs.library')}
                        </TabsTrigger>
                        <TabsTrigger
                            value="setup"
                            className="text-sm gap-2 data-[state=active]:bg-secondary data-[state=active]:text-secondary-foreground data-[state=active]:shadow-xl data-[state=active]:scale-[1.02] transition-all duration-300 ease-out data-[state=inactive]:text-muted-foreground data-[state=inactive]:hover:bg-muted/40 rounded-lg"
                        >
                            <Settings className="w-4 h-4" />
                            {t('guide.tabs.setup')}
                        </TabsTrigger>
                    </TabsList>

                    <TabsContent value="usage" className="space-y-6">
                        <div className="grid gap-6 md:grid-cols-2">
                            <Card>
                                <CardHeader>
                                    <CardTitle className="flex items-center gap-2 text-lg">
                                        <Radar className="h-5 w-5 text-primary" />
                                        {t('guide.usage.step1.title')}
                                    </CardTitle>
                                </CardHeader>
                                <CardContent>
                                    <p className="text-muted-foreground">
                                        {t('guide.usage.step1.description')}
                                    </p>
                                </CardContent>
                            </Card>

                            <Card>
                                <CardHeader>
                                    <CardTitle className="flex items-center gap-2 text-lg">
                                        <BookOpenText className="h-5 w-5 text-primary" />
                                        {t('guide.usage.step2.title')}
                                    </CardTitle>
                                </CardHeader>
                                <CardContent>
                                    <p className="text-muted-foreground">
                                        {t('guide.usage.step2.description')}
                                    </p>
                                </CardContent>
                            </Card>

                            <Card>
                                <CardHeader>
                                    <CardTitle className="flex items-center gap-2 text-lg">
                                        <GraduationCap className="h-5 w-5 text-primary" />
                                        {t('guide.usage.step3.title')}
                                    </CardTitle>
                                </CardHeader>
                                <CardContent>
                                    <p className="text-muted-foreground">
                                        {t('guide.usage.step3.description')}
                                    </p>
                                </CardContent>
                            </Card>

                            <Card>
                                <CardHeader>
                                    <CardTitle className="flex items-center gap-2 text-lg">
                                        <Brain className="h-5 w-5 text-primary" />
                                        {t('guide.usage.step4.title')}
                                    </CardTitle>
                                </CardHeader>
                                <CardContent>
                                    <p className="text-muted-foreground">
                                        {t('guide.usage.step4.description')}
                                    </p>
                                </CardContent>
                            </Card>
                        </div>
                    </TabsContent>

                    <TabsContent value="library" className="space-y-6">
                        <div className="grid gap-6 md:grid-cols-3">
                            <Card>
                                <CardHeader>
                                    <CardTitle className="flex items-center gap-2 text-lg">
                                        <Brain className="h-5 w-5 text-primary" />
                                        {t('guide.library.focus.title')}
                                    </CardTitle>
                                </CardHeader>
                                <CardContent>
                                    <p className="text-muted-foreground">
                                        {t('guide.library.focus.description')}
                                    </p>
                                </CardContent>
                            </Card>

                            <Card>
                                <CardHeader>
                                    <CardTitle className="flex items-center gap-2 text-lg">
                                        <Library className="h-5 w-5 text-primary" />
                                        {t('guide.library.documents.title')}
                                    </CardTitle>
                                </CardHeader>
                                <CardContent>
                                    <p className="text-muted-foreground">
                                        {t('guide.library.documents.description')}
                                    </p>
                                </CardContent>
                            </Card>

                            <Card>
                                <CardHeader>
                                    <CardTitle className="flex items-center gap-2 text-lg">
                                        <ClockAlert className="h-5 w-5 text-primary" />
                                        {t('guide.library.backlog.title')}
                                    </CardTitle>
                                </CardHeader>
                                <CardContent>
                                    <p className="text-muted-foreground">
                                        {t('guide.library.backlog.description')}
                                    </p>
                                </CardContent>
                            </Card>
                        </div>
                    </TabsContent>

                    <TabsContent value="setup">
                        <div className="grid gap-6">
                            {/* Limitations Section (Previously the main content) */}
                            <Card className="border-l-4 border-l-yellow-500/50">
                                <CardHeader>
                                    <div className="flex items-center gap-2">
                                        <Monitor className="h-5 w-5 text-yellow-500" />
                                        <CardTitle>{t('guide.limitations.title')}</CardTitle>
                                    </div>
                                    <CardDescription className="text-base">
                                        {t('guide.limitations.description')}
                                    </CardDescription>
                                </CardHeader>
                                <CardContent className="space-y-6">
                                    <div className="space-y-4">
                                        <h3 className="font-medium text-lg leading-relaxed">
                                            {t('guide.limitations.steps.intro')}
                                        </h3>
                                        <div className="grid gap-4 pl-4 border-l-2 border-border/50">
                                            <div className="space-y-1">
                                                <p className="font-medium">{t('guide.limitations.steps.step1')}</p>
                                            </div>
                                            <div className="space-y-1">
                                                <p>{t('guide.limitations.steps.step2')}</p>
                                            </div>
                                            <div className="space-y-1">
                                                <p>{t('guide.limitations.steps.step3')}</p>
                                                <p className="text-sm text-muted-foreground italic">
                                                    {t('guide.limitations.steps.step3Note')}
                                                </p>
                                            </div>
                                        </div>
                                    </div>

                                    <div className="rounded-lg bg-muted/50 p-4 border border-border/50 space-y-2">
                                        <div className="flex items-center gap-2 text-warning">
                                            <Cpu className="h-4 w-4" />
                                            <h4 className="font-semibold">{t('guide.limitations.performance.description')}</h4>
                                        </div>
                                        <p className="text-sm text-muted-foreground">
                                            {t('guide.limitations.performance.example')}
                                        </p>
                                        <div className="flex items-start gap-2 text-amber-500 bg-amber-500/10 p-2 rounded text-sm font-medium mt-2">
                                            <AlertTriangle className="h-4 w-4 shrink-0 mt-0.5" />
                                            <p>{t('guide.limitations.performance.important')}</p>
                                        </div>
                                    </div>
                                    <div className="rounded-lg bg-muted/50 p-4 border border-border/50 space-y-2">
                                        <div className="flex items-center gap-2">
                                            <ImageIcon className="h-4 w-4" />
                                            <h4 className="font-semibold">{t('guide.limitations.images.description')}</h4>
                                        </div>
                                        <p className="text-xs text-muted-foreground">
                                            * {t('guide.limitations.images.note')}
                                        </p>
                                    </div>

                                </CardContent>
                            </Card>
                        </div>
                    </TabsContent>
                </Tabs>
            </div>
        </Layout>
    );
}
