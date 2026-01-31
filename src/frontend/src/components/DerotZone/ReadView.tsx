import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { FileText, Loader2, AlertCircle } from 'lucide-react';
import { useEffect, useState } from 'react';
import { useActivities } from '@/hooks/useActivities';
import { SourceTypes } from '@/models/Enums';
import type { UserActivity } from '@/models/UserActivity';

interface ReadViewProps {
    activityId?: string;
}

export function ReadView({ activityId }: ReadViewProps) {
    const { getActivity } = useActivities();
    const [activity, setActivity] = useState<UserActivity | null>(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (activityId) {
            setLoading(true);
            setError(null);
            getActivity(activityId)
                .then(setActivity)
                .catch((err) => {
                    console.error('Failed to fetch activity content', err);
                    setError("Impossible de charger le contenu de l'article.");
                })
                .finally(() => setLoading(false));
        }
    }, [activityId, getActivity]);

    if (loading) {
        return (
            <div className="flex flex-col items-center justify-center py-20 space-y-4">
                <Loader2 className="h-10 w-10 animate-spin text-primary/40" />
                <p className="text-muted-foreground animate-pulse">Chargement de l'article...</p>
            </div>
        );
    }

    if (error) {
        return (
            <div className="flex flex-col items-center justify-center py-20 space-y-4 text-destructive">
                <AlertCircle className="h-10 w-10" />
                <p>{error}</p>
            </div>
        );
    }

    const title = activity?.title || "...";
    const content = activity?.articleContent || (activity as any)?.payload || "Aucun contenu disponible.";

    return (
        <Card className="border-border/30 bg-card/30 backdrop-blur-xl rounded-3xl overflow-hidden shadow-xl animate-in fade-in slide-in-from-bottom-4 duration-700">
            <CardHeader className="flex flex-row items-center gap-6 border-b border-border/20 p-8">
                <div className="p-4 rounded-2xl bg-primary/10 text-primary shadow-sm">
                    <FileText className="h-8 w-8" />
                </div>
                <div className="space-y-1.5 flex-1">
                    <div className="flex items-center justify-between gap-4">
                        <CardTitle className="text-3xl font-bold tracking-tight">{title}</CardTitle>
                        <Badge variant="outline" className="px-3 py-1 text-xs font-semibold uppercase tracking-wider bg-background/50 border-primary/20 text-primary">
                            {activity?.sourceType === SourceTypes.Wikipedia ? 'Wikipedia' : 'Document'}
                        </Badge>
                    </div>
                    {activity?.displayTitle && (
                        <p className="text-base font-medium text-foreground/70">{activity.displayTitle}</p>
                    )}
                    {activityId && <p className="text-[10px] font-mono text-muted-foreground/40 uppercase tracking-widest pt-1">ID : {activityId}</p>}
                </div>
            </CardHeader>
            <CardContent className="p-8 prose prose-neutral dark:prose-invert max-w-none">
                <div className="text-xl leading-relaxed text-foreground/90 whitespace-pre-wrap font-serif">
                    {content}
                </div>
            </CardContent>
        </Card>
    );
}
