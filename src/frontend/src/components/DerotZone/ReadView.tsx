import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { FileText, Loader2, AlertCircle } from 'lucide-react';
import { useEffect, useState } from 'react';
import { useActivities } from '@/hooks/useActivities';
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
        <Card className="border-border/50 bg-card/50 backdrop-blur-sm animate-in fade-in slide-in-from-bottom-4 duration-700">
            <CardHeader className="flex flex-row items-center gap-4 border-b border-border/40 pb-6">
                <div className="p-3 rounded-xl bg-blue-500/10 text-blue-500">
                    <FileText className="h-6 w-6" />
                </div>
                <div className="space-y-1">
                    <CardTitle className="text-2xl">{title}</CardTitle>
                    {activityId && <p className="text-xs text-muted-foreground opacity-50">ID : {activityId}</p>}
                </div>
            </CardHeader>
            <CardContent className="pt-8 prose prose-invert max-w-none">
                <div className="text-lg leading-relaxed text-foreground/90 whitespace-pre-wrap">
                    {content}
                </div>
            </CardContent>
        </Card>
    );
}
