import type { LucideIcon } from 'lucide-react';
import { Sparkles } from 'lucide-react';

interface PageHeaderProps {
    title: string;
    subtitle?: string;
    description?: string;
    icon?: LucideIcon;
    badgeIcon?: LucideIcon;
}

export function PageHeader({
    title,
    subtitle,
    description,
    icon: Icon,
    badgeIcon: BadgeIcon = Sparkles
}: PageHeaderProps) {
    return (
        <div className="flex flex-col gap-4 p-6 bg-card/40 rounded-2xl border shadow-sm backdrop-blur-md relative overflow-hidden group animate-in fade-in slide-in-from-top-4 duration-700">
            {Icon && (
                <div className="absolute top-0 right-0 p-2 opacity-10 group-hover:opacity-20 transition-opacity duration-500">
                    <Icon className="w-24 h-24 text-primary" />
                </div>
            )}
            <div className="relative z-10 space-y-1">
                <div className="flex items-center gap-2 text-primary font-semibold tracking-wide uppercase text-[11px]">
                    <BadgeIcon className="w-3.5 h-3.5" />
                    <span>{title}</span>
                </div>
                {subtitle && (
                    <h1 className="text-3xl font-bold tracking-tight text-foreground">
                        {subtitle}
                    </h1>
                )}
                {description && (
                    <p className="text-muted-foreground max-w-2xl text-base leading-relaxed">
                        {description}
                    </p>
                )}
            </div>
        </div>
    );
}
