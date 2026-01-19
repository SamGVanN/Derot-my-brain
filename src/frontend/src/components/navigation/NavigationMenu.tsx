import { NavLink } from 'react-router';
import { useTranslation } from 'react-i18next';
import {
    Home,
    Sparkles,
    Star,
    User,
    Settings,
    BookOpen,
    LogOut,
    Menu,
    X
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { useAuth } from '@/hooks/useAuth';
import { useState } from 'react';
import { cn } from '@/lib/utils';

const navigationItems = [
    { path: '/history', icon: Home, labelKey: 'nav.history', label: 'History' },
    { path: '/derot', icon: Sparkles, labelKey: 'nav.derot', label: 'Derot' },
    { path: '/tracked-topics', icon: Star, labelKey: 'nav.trackedTopics', label: 'Tracked Topics' },
    { path: '/profile', icon: User, labelKey: 'nav.profile', label: 'Profile' },
    { path: '/preferences', icon: Settings, labelKey: 'nav.preferences', label: 'Preferences' },
    { path: '/guide', icon: BookOpen, labelKey: 'nav.guide', label: 'Guide' },
];

export function NavigationMenu() {
    const { t } = useTranslation();
    const { logout } = useAuth();
    const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

    const handleLogout = () => {
        logout();
        setIsMobileMenuOpen(false);
    };

    const closeMobileMenu = () => {
        setIsMobileMenuOpen(false);
    };

    return (
        <>
            {/* Mobile Hamburger Button */}
            <Button
                variant="ghost"
                size="icon"
                className="md:hidden fixed top-20 left-4 z-50"
                onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
            >
                {isMobileMenuOpen ? <X className="h-6 w-6" /> : <Menu className="h-6 w-6" />}
            </Button>

            {/* Mobile Menu Overlay */}
            {isMobileMenuOpen && (
                <div
                    className="md:hidden fixed inset-0 bg-background/80 backdrop-blur-sm z-40"
                    onClick={closeMobileMenu}
                />
            )}

            {/* Navigation Sidebar */}
            <nav
                className={cn(
                    "fixed left-0 top-16 h-[calc(100vh-4rem)] w-64 bg-card border-r z-40 transition-transform duration-300 ease-in-out",
                    "md:translate-x-0",
                    isMobileMenuOpen ? "translate-x-0" : "-translate-x-full"
                )}
            >
                <div className="flex flex-col h-full p-4">
                    {/* Navigation Links */}
                    <div className="flex-1 space-y-1">
                        {navigationItems.map((item) => (
                            <NavLink
                                key={item.path}
                                to={item.path}
                                onClick={closeMobileMenu}
                                className={({ isActive }) =>
                                    cn(
                                        "flex items-center gap-3 px-3 py-2 rounded-lg transition-colors",
                                        "hover:bg-accent hover:text-accent-foreground",
                                        isActive && "bg-accent text-accent-foreground font-medium"
                                    )
                                }
                            >
                                <item.icon className="h-5 w-5" />
                                <span>{t(item.labelKey, item.label)}</span>
                            </NavLink>
                        ))}
                    </div>

                    {/* Logout Button */}
                    <div className="border-t pt-4">
                        <Button
                            variant="ghost"
                            className="w-full justify-start gap-3 text-muted-foreground hover:text-destructive hover:bg-destructive/10"
                            onClick={handleLogout}
                        >
                            <LogOut className="h-5 w-5" />
                            <span>{t('nav.logout', 'Logout')}</span>
                        </Button>
                    </div>
                </div>
            </nav>

            {/* Spacer for desktop to prevent content from going under sidebar */}
            <div className="hidden md:block w-64 flex-shrink-0" />
        </>
    );
}
