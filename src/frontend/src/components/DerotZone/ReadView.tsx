import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { FileText, Loader2 } from 'lucide-react';

interface ReadViewProps {
    activityId?: string;
    isLoading?: boolean;
}

export function ReadView({ activityId, isLoading }: ReadViewProps) {
    if (isLoading) {
        return (
            <div className="flex flex-col items-center justify-center py-20 space-y-4">
                <Loader2 className="h-10 w-10 animate-spin text-primary/40" />
                <p className="text-muted-foreground animate-pulse">Chargement de l'article...</p>
            </div>
        );
    }

    return (
        <Card className="border-border/50 bg-card/50 backdrop-blur-sm animate-in fade-in slide-in-from-bottom-4 duration-700">
            <CardHeader className="flex flex-row items-center gap-4 border-b border-border/40 pb-6">
                <div className="p-3 rounded-xl bg-blue-500/10 text-blue-500">
                    <FileText className="h-6 w-6" />
                </div>
                <div className="space-y-1">
                    <CardTitle className="text-2xl">Mode Lecture : [Titre de l'article]</CardTitle>
                    <p className="text-sm text-muted-foreground">ID Activité : {activityId}</p>
                </div>
            </CardHeader>
            <CardContent className="pt-8 prose prose-invert max-w-none">
                <p className="text-lg leading-relaxed text-foreground/90">
                    Le contenu complet de l'article Wikipedia s'affichera ici une fois connecté au backend.
                    Vous pourrez lire l'article en détail avant de passer au quiz de mémorisation.
                </p>
                <div className="h-64 flex items-center justify-center border-2 border-dashed border-border/40 rounded-xl bg-accent/5 my-8">
                    <p className="text-muted-foreground italic text-center px-4">
                        [Zone de contenu article Wikipedia enrichi]
                    </p>
                </div>
            </CardContent>
        </Card>
    );
}
