import React from 'react';
import { Loader2, CheckCircle2, AlertCircle, Clock } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import {
    Tooltip,
    TooltipContent,
    TooltipTrigger,
} from '@/components/ui/tooltip';
import { ContentExtractionStatus } from '@/models/ContentExtractionStatus';

interface ExtractionStatusBadgeProps {
    status: ContentExtractionStatus;
    error?: string | null;
}

export const ExtractionStatusBadge: React.FC<ExtractionStatusBadgeProps> = ({ status, error }) => {
    const getStatusConfig = () => {
        switch (status) {
            case ContentExtractionStatus.Pending:
                return {
                    label: 'Pending',
                    variant: 'secondary' as const,
                    icon: <Clock className="h-3 w-3" />,
                    className: 'bg-yellow-500/10 text-yellow-600 dark:text-yellow-400 border-yellow-500/20'
                };
            case ContentExtractionStatus.Processing:
                return {
                    label: 'Extracting...',
                    variant: 'secondary' as const,
                    icon: <Loader2 className="h-3 w-3 animate-spin" />,
                    className: 'bg-blue-500/10 text-blue-600 dark:text-blue-400 border-blue-500/20'
                };
            case ContentExtractionStatus.Completed:
                return {
                    label: 'Ready',
                    variant: 'secondary' as const,
                    icon: <CheckCircle2 className="h-3 w-3" />,
                    className: 'bg-green-500/10 text-green-600 dark:text-green-400 border-green-500/20'
                };
            case ContentExtractionStatus.Failed:
                return {
                    label: 'Error',
                    variant: 'destructive' as const,
                    icon: <AlertCircle className="h-3 w-3" />,
                    className: 'bg-red-500/10 text-red-600 dark:text-red-400 border-red-500/20'
                };
            default:
                return {
                    label: 'Unknown',
                    variant: 'secondary' as const,
                    icon: null,
                    className: ''
                };
        }
    };

    const config = getStatusConfig();

    const badge = (
        <Badge variant={config.variant} className={`flex items-center gap-1 ${config.className}`}>
            {config.icon}
            <span>{config.label}</span>
        </Badge>
    );

    // Show tooltip with error message if failed
    if (status === ContentExtractionStatus.Failed && error) {
        return (
            <Tooltip>
                <TooltipTrigger asChild>
                    {badge}
                </TooltipTrigger>
                <TooltipContent>
                    <p className="max-w-xs">{error}</p>
                </TooltipContent>
            </Tooltip>
        );
    }

    return badge;
};
