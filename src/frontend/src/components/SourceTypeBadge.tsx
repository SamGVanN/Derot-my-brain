import React from 'react';
import { Book, FileText, Link as LinkIcon, HelpCircle } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { cn } from '@/lib/utils';
import { getSourceTypeLabel } from '@/lib/sourceUtils';
import type { SourceType } from '@/models/Enums';

interface SourceTypeBadgeProps {
    type?: string | SourceType | number;
    className?: string;
}

export const SourceTypeBadge: React.FC<SourceTypeBadgeProps> = ({ type, className }) => {
    const label = getSourceTypeLabel(type);

    const getIcon = () => {
        switch (label) {
            case 'Wikipedia': return <Book className="h-3 w-3" />;
            case 'Document': return <FileText className="h-3 w-3" />;
            case 'Link': return <LinkIcon className="h-3 w-3" />;
            default: return <HelpCircle className="h-3 w-3" />;
        }
    };

    const getStyles = () => {
        switch (label) {
            case 'Wikipedia':
                return "bg-primary/10 text-primary border-primary/20";
            case 'Document':
                return "bg-blue-500/10 text-blue-600 border-blue-500/20 dark:text-blue-400";
            case 'Link':
                return "bg-orange-500/10 text-orange-600 border-orange-500/20";
            default:
                return "bg-muted text-muted-foreground border-transparent";
        }
    };

    return (
        <Badge
            variant="outline"
            className={cn(
                "px-2 py-0.5 text-[10px] font-bold uppercase tracking-wider gap-1.5 flex items-center w-fit",
                getStyles(),
                className
            )}
        >
            {getIcon()}
            {label}
        </Badge>
    );
};
