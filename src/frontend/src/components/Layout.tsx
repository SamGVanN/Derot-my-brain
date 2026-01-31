import type { ReactNode } from "react";
import { Header } from "./navigation/Header";
import { NavigationMenu } from "./navigation/NavigationMenu";
import { useAuth } from "@/hooks/useAuth";
import { useTranslation } from 'react-i18next';
import { TooltipProvider } from "./ui/tooltip";

interface LayoutProps {
    children: ReactNode;
}

export function Layout({ children }: LayoutProps) {
    const { user } = useAuth();
    const { t } = useTranslation();

    return (
        <TooltipProvider>
            <div className="min-h-screen flex flex-col w-full font-sans text-foreground selection:bg-primary/20">
                {/* Header */}
                <Header />

                <div className="flex flex-1 overflow-hidden">
                    {/* Navigation Menu (only for authenticated users) */}
                    {user && <NavigationMenu />}

                    <div className="flex flex-col flex-1 overflow-y-auto">
                        {/* Main Content */}
                        <main className="flex-1 animate-in fade-in duration-500 p-3 md:px-6 md:pb-6 md:pt-2">
                            {children}
                        </main>

                        {/* Footer */}
                        <footer className="border-t border-border/40 bg-background/50 backdrop-blur-sm mt-auto">
                            <div className="container mx-auto px-4 py-6 max-w-5xl flex flex-col md:flex-row justify-between items-center gap-4 text-sm text-muted-foreground">
                                <p>
                                    {t('layout.footer', '© 2026 Derot My Brain. Local Learning.')}
                                </p>
                                <div className="flex gap-4">
                                    <span>{t('layout.version', 'v0.7.2')}</span>
                                </div>
                            </div>
                        </footer>
                    </div>
                </div>
            </div>
        </TooltipProvider>
    );
}
