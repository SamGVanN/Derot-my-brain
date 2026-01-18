import { useTranslation } from 'react-i18next';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { BookOpen, BrainCircuit, History, Library } from 'lucide-react';

export const GuideContent = () => {
    const { t } = useTranslation();

    return (
        <div className="space-y-6">
            <div className="text-center space-y-4">
                <BrainCircuit className="w-16 h-16 mx-auto text-primary" />
                <h2 className="text-2xl font-bold">{t('welcome.guide.purposeTitle')}</h2>
                <p className="text-muted-foreground max-w-lg mx-auto">
                    {t('welcome.guide.purposeText')}
                </p>
            </div>

            <div className="grid gap-4 md:grid-cols-2">
                <Card>
                    <CardHeader className="flex flex-row items-center gap-4 space-y-0 pb-2">
                        <BookOpen className="w-8 h-8 text-blue-500" />
                        <CardTitle className="text-base font-semibold">
                            {t('welcome.guide.feature1Title')}
                        </CardTitle>
                    </CardHeader>
                    <CardContent>
                        <p className="text-sm text-muted-foreground">
                            {t('welcome.guide.feature1Text')}
                        </p>
                    </CardContent>
                </Card>

                <Card>
                    <CardHeader className="flex flex-row items-center gap-4 space-y-0 pb-2">
                        <BrainCircuit className="w-8 h-8 text-purple-500" />
                        <CardTitle className="text-base font-semibold">
                            {t('welcome.guide.feature2Title')}
                        </CardTitle>
                    </CardHeader>
                    <CardContent>
                        <p className="text-sm text-muted-foreground">
                            {t('welcome.guide.feature2Text')}
                        </p>
                    </CardContent>
                </Card>

                <Card>
                    <CardHeader className="flex flex-row items-center gap-4 space-y-0 pb-2">
                        <History className="w-8 h-8 text-green-500" />
                        <CardTitle className="text-base font-semibold">
                            {t('welcome.guide.feature3Title')}
                        </CardTitle>
                    </CardHeader>
                    <CardContent>
                        <p className="text-sm text-muted-foreground">
                            {t('welcome.guide.feature3Text')}
                        </p>
                    </CardContent>
                </Card>

                <Card>
                    <CardHeader className="flex flex-row items-center gap-4 space-y-0 pb-2">
                        <Library className="w-8 h-8 text-orange-500" />
                        <CardTitle className="text-base font-semibold">
                            {t('welcome.guide.feature4Title')}
                        </CardTitle>
                    </CardHeader>
                    <CardContent>
                        <p className="text-sm text-muted-foreground">
                            {t('welcome.guide.feature4Text')}
                        </p>
                    </CardContent>
                </Card>
            </div>
        </div>
    );
};
