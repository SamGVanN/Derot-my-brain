import { NavLink } from 'react-router';
import { useTranslation } from 'react-i18next';
import {
    BrainCircuit,
    Brain,
    User,
    Settings,
    BookOpen,
    LogOut,
    Menu,
    X,
    History,
    Home,
    ChartColumn,
    Library,
    ClockAlert
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { useAuth } from '@/hooks/useAuth';
import { useState } from 'react';
import { cn } from '@/lib/utils';

type NavigationItem = {
    path: string;
    icon: React.ElementType;
    labelKey: string;
    label: string;
    disabled?: boolean;
};

type NavigationGroup = {
    title?: string;
    items: NavigationItem[];
};

const navigationGroups: NavigationGroup[] = [
    {
        title: 'Welcome',
        items: [
            { path: '/homepage', icon: Home, labelKey: 'nav.home', label: 'Homepage' },
            { path: '/guide', icon: BookOpen, labelKey: 'nav.guide', label: 'Guide' },
        ]
    },
    {
        title: 'Derot my brain',
        items: [
            { path: '/derot', icon: BrainCircuit, labelKey: 'nav.derot', label: 'Derot zone' },
            { path: '/history', icon: History, labelKey: 'nav.history', label: 'History' },
        ]
    },
    {
        title: 'Knowledge',
        items: [
            { path: '/focus-area', icon: Brain, labelKey: 'nav.focusArea', label: 'My Focus Area' },
            { path: '/documents', icon: Library, labelKey: 'nav.documents', label: 'Documents' },
            { path: '/backlog', icon: ClockAlert, labelKey: 'nav.backlog', label: 'Backlog' },
        ]
    },
    {
        title: 'Personal',
        items: [
            { path: '/dashboard', icon: ChartColumn, labelKey: 'nav.dashboard', label: 'Dashboard' },
            { path: '/profile', icon: User, labelKey: 'nav.profile', label: 'Profile' },
            { path: '/preferences', icon: Settings, labelKey: 'nav.preferences', label: 'Preferences' },
        ]
    }
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
                    {/* Navigation Groups */}
                    <div className="flex-1 space-y-6 overflow-y-auto">
                        {navigationGroups.map((group, groupIndex) => (
                            <div key={groupIndex} className="space-y-2">
                                {group.title && (
                                    <h3 className="px-3 text-xs font-semibold text-muted-foreground uppercase tracking-wider">
                                        {group.title}
                                    </h3>
                                )}
                                <div className="space-y-1">
                                    {group.items.map((item) => (
                                        <NavLink
                                            key={item.path}
                                            to={item.disabled ? '#' : item.path}
                                            onClick={(e) => {
                                                if (item.disabled) {
                                                    e.preventDefault();
                                                    return;
                                                }
                                                closeMobileMenu();
                                            }}
                                            className={({ isActive }) =>
                                                cn(
                                                    "flex items-center gap-3 px-3 py-2 rounded-lg transition-colors",
                                                    item.disabled && "opacity-50 cursor-not-allowed",
                                                    !item.disabled && "hover:bg-accent hover:text-accent-foreground",
                                                    !item.disabled && isActive && "bg-accent text-accent-foreground font-medium"
                                                )
                                            }
                                        >
                                            <item.icon className="h-5 w-5" />
                                            <span>{t(item.labelKey, item.label)}</span>
                                        </NavLink>
                                    ))}
                                </div>
                            </div>
                        ))}
                    </div>

                    {/* Logout Button */}
                    <div className="border-t pt-4 mt-auto">
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
