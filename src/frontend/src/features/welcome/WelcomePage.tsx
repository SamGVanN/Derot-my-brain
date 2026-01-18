import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';
import { ScrollArea } from '@/components/ui/scroll-area';
import { GuideContent } from './GuideContent';
import type { User } from '@/models/User';

interface WelcomePageProps {
    user: User;
    onProceed: () => void;
    onDismiss: () => void;
}

export const WelcomePage = ({ user, onProceed, onDismiss }: WelcomePageProps) => {
    const { t } = useTranslation();
    const [showGuide, setShowGuide] = useState(false);

    if (showGuide) {
        return (
            <div className="min-h-screen bg-background flex items-center justify-center p-4">
                <Card className="w-full max-w-3xl max-h-[90vh] flex flex-col">
                    <CardHeader>
                        <CardTitle className="text-2xl text-center">
                            {t('welcome.guide.title')}
                        </CardTitle>
                    </CardHeader>
                    <CardContent className="flex-1 overflow-hidden">
                        <ScrollArea className="h-full pr-4">
                            <GuideContent />
                        </ScrollArea>
                    </CardContent>
                    <CardFooter className="flex justify-between border-t p-6">
                        <Button variant="ghost" onClick={() => setShowGuide(false)}>
                            {t('common.back')}
                        </Button>
                        <div className="flex gap-2">
                            <Button variant="outline" onClick={onProceed}>
                                {t('welcome.startApp')}
                            </Button>
                            <Button onClick={onDismiss}>
                                {t('welcome.dontShowAgain')}
                            </Button>
                        </div>
                    </CardFooter>
                </Card>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-background flex items-center justify-center p-4">
            <Card className="w-full max-w-lg text-center">
                <CardHeader className="space-y-4">
                    <CardTitle className="text-3xl font-bold">
                        {t('welcome.title')} {user.name}!
                    </CardTitle>
                    <p className="text-muted-foreground text-lg">
                        {t('welcome.intro')}
                    </p>
                </CardHeader>
                <CardContent className="space-y-4">
                    <Button
                        size="lg"
                        className="w-full"
                        onClick={() => setShowGuide(true)}
                    >
                        {t('welcome.readGuide')}
                    </Button>
                    <div className="relative">
                        <div className="absolute inset-0 flex items-center">
                            <span className="w-full border-t" />
                        </div>
                        <div className="relative flex justify-center text-xs uppercase">
                            <span className="bg-background px-2 text-muted-foreground">
                                {t('common.or')}
                            </span>
                        </div>
                    </div>
                    <Button
                        variant="outline"
                        size="lg"
                        className="w-full"
                        onClick={onProceed}
                    >
                        {t('welcome.startApp')}
                    </Button>
                </CardContent>
                <CardFooter className="justify-center">
                    <Button
                        variant="link"
                        size="sm"
                        className="text-muted-foreground"
                        onClick={onDismiss}
                    >
                        {t('welcome.dontShowAgain')}
                    </Button>
                </CardFooter>
            </Card>
        </div>
    );
};
