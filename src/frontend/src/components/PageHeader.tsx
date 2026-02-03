import type { LucideIcon } from 'lucide-react';
import { Sparkles } from 'lucide-react';
import { cn } from '@/lib/utils';

interface PageHeaderProps {
    title: string;
    subtitle?: string;
    description?: string;
    icon?: LucideIcon;
    badgeIcon?: LucideIcon;
    className?: string;
}

export function PageHeader({
    title,
    subtitle,
    description,
    icon: Icon,
    badgeIcon: BadgeIcon = Sparkles,
    className
}: PageHeaderProps) {
    return (
        <div className={cn(
            "p-6 bg-card/40 backdrop-blur-xl rounded-2xl border border-border/60 shadow-md relative overflow-hidden group animate-in fade-in slide-in-from-bottom-2 duration-400 fill-mode-both",
            className
        )}>
            {/* Background Decoration */}
            {Icon && (
                <div className="absolute top-1/2 -translate-y-1/2 -right-6 p-2 opacity-10 group-hover:opacity-20 transition-opacity duration-700 ease-in-out pointer-events-none">
                    <Icon className="w-48 h-48 text-primary rotate-6" />
                </div>
            )}

            <div className="relative z-10 space-y-1.5 focus-within:z-20">
                <div className="flex items-center gap-1.5 text-primary font-bold tracking-widest uppercase text-[10px] animate-in fade-in slide-in-from-left-2 duration-300 fill-mode-backwards">
                    <div className="p-0.5 rounded-sm bg-primary/10">
                        <BadgeIcon className="w-3 h-3" />
                    </div>
                    <span>{title}</span>
                </div>
                {subtitle && (
                    <h1 className="text-2xl font-extrabold tracking-tight text-foreground animate-in fade-in slide-in-from-left-3 duration-400 delay-75 fill-mode-backwards leading-tight">
                        {subtitle}
                    </h1>
                )}
                {description && (
                    <p className="text-muted-foreground/90 max-w-2xl text-base leading-snug animate-in fade-in slide-in-from-left-4 duration-500 delay-150 fill-mode-backwards">
                        {description}
                    </p>
                )}
            </div>
        </div>
    );
}
