import { Link, useNavigate } from 'react-router';
import { useTranslation } from 'react-i18next';
import { Settings, LogOut, User, History, Brain, Bot } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { ThemeSelector } from '@/components/theme-selector';
import { LanguageSelector } from '@/components/LanguageSelector';
import { useAuth } from '@/hooks/useAuth';

export function Header() {
    const { t } = useTranslation();
    const { user, logout } = useAuth();
    const navigate = useNavigate();

    const handleLogout = () => {
        logout();
        navigate('/');
    };

    return (
        <header className="sticky top-0 z-50 w-full border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
            <div className="container flex h-16 items-center justify-between px-4">
                {/* Logo / Title */}
                <Link
                    to={user ? "/history" : "/"}
                    className="flex items-center space-x-2 hover:opacity-80 transition-opacity"
                >
                    <div className="h-12 w-12 overflow-hidden flex items-center justify-center">
                        <img
                            src="/images/logo.png"
                            alt="Derot My Brain Logo"
                            className="h-full w-full object-contain"
                        />
                    </div>
                    <span className="font-bold text-xl hidden sm:inline-block tracking-tight">
                        Derot My Brain
                    </span>
                </Link>

                {/* Right Side - Authentication State */}
                <div className="flex items-center gap-2">
                    {!user ? (
                        // Unauthenticated State: Language + Theme Selectors
                        <>
                            <LanguageSelector />
                            <ThemeSelector />
                        </>
                    ) : (
                        // Authenticated State: Settings + User Menu + Logout
                        <>
                            {/* Settings Button (Configuration) */}
                            <Button
                                variant="ghost"
                                size="icon"
                                onClick={() => navigate('/configuration')}
                                title={t('nav.configuration', 'Configuration')}
                            >
                                <Bot className="h-5 w-5" />
                            </Button>

                            {/* User Menu Dropdown */}
                            <DropdownMenu>
                                <DropdownMenuTrigger asChild>
                                    <Button variant="ghost" className="gap-2">
                                        <User className="h-5 w-5" />
                                        <span className="hidden sm:inline-block">{user.name}</span>
                                    </Button>
                                </DropdownMenuTrigger>
                                <DropdownMenuContent align="end" className="w-56">
                                    <DropdownMenuLabel>{t('nav.myAccount', 'My Account')}</DropdownMenuLabel>
                                    <DropdownMenuSeparator />
                                    <DropdownMenuItem onClick={() => navigate('/profile')}>
                                        <User className="mr-2 h-4 w-4" />
                                        {t('nav.profile', 'Profile')}
                                    </DropdownMenuItem>
                                    <DropdownMenuItem onClick={() => navigate('/history')}>
                                        <History className="mr-2 h-4 w-4" />
                                        {t('nav.history', 'History')}
                                    </DropdownMenuItem>
                                    <DropdownMenuItem onClick={() => navigate('/tracked-topics')}>
                                        <Brain className="mr-2 h-4 w-4" />
                                        {t('nav.focusArea', 'Mes focus')}
                                    </DropdownMenuItem>
                                    <DropdownMenuSeparator />
                                    <DropdownMenuItem onClick={handleLogout} className="text-destructive focus:text-destructive">
                                        <LogOut className="mr-2 h-4 w-4" />
                                        {t('nav.logout', 'Logout')}
                                    </DropdownMenuItem>
                                </DropdownMenuContent>
                            </DropdownMenu>

                            {/* Quick Logout Button */}
                            <Button
                                variant="ghost"
                                size="icon"
                                onClick={handleLogout}
                                title={t('nav.logout', 'Logout')}
                                className="text-muted-foreground hover:text-destructive hover:bg-destructive/10"
                            >
                                <LogOut className="h-5 w-5" />
                            </Button>
                        </>
                    )}
                </div>
            </div>
        </header>
    );
}
