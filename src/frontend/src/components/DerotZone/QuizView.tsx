import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { GraduationCap, Loader2, CheckCircle2 } from 'lucide-react';

interface QuizViewProps {
    activityId?: string;
    isLoading?: boolean;
}

export function QuizView({ activityId, isLoading }: QuizViewProps) {
    if (isLoading) {
        return (
            <div className="flex flex-col items-center justify-center py-20 space-y-4">
                <Loader2 className="h-10 w-10 animate-spin text-primary/40" />
                <p className="text-muted-foreground animate-pulse">Génération du quiz en cours...</p>
            </div>
        );
    }

    return (
        <Card className="border-border/50 bg-card/50 backdrop-blur-sm animate-in fade-in slide-in-from-bottom-4 duration-700">
            <CardHeader className="flex flex-row items-center gap-4 border-b border-border/40 pb-6">
                <div className="p-3 rounded-xl bg-purple-500/10 text-purple-500">
                    <GraduationCap className="h-6 w-6" />
                </div>
                <div className="space-y-1">
                    <CardTitle className="text-2xl">Mode Quiz : Teste tes connaissances</CardTitle>
                    <p className="text-sm text-muted-foreground">Basé sur l'activité : {activityId}</p>
                </div>
            </CardHeader>
            <CardContent className="pt-8 space-y-8">
                {[1, 2, 3].map((i) => (
                    <div key={i} className="space-y-4 p-6 rounded-xl bg-accent/10 border border-accent/20">
                        <h4 className="font-bold flex items-center gap-2">
                            <span className="flex items-center justify-center w-6 h-6 rounded-full bg-primary/20 text-primary text-xs">
                                {i}
                            </span>
                            Question de démonstration n°{i} ?
                        </h4>
                        <div className="grid grid-cols-1 gap-3">
                            {[1, 2, 3, 4].map((opt) => (
                                <div key={opt} className="flex items-center gap-3 p-3 rounded-lg bg-background/50 border border-border/40 hover:border-primary/40 hover:bg-primary/5 cursor-pointer transition-all">
                                    <div className="w-4 h-4 rounded-full border-2 border-primary/40" />
                                    <span className="text-sm text-foreground/80">Option de réponse {opt}</span>
                                </div>
                            ))}
                        </div>
                    </div>
                ))}

                <div className="flex items-center gap-2 text-primary/60 text-sm justify-center py-4 italic">
                    <CheckCircle2 className="h-4 w-4" />
                    Répondez à toutes les questions pour voir votre score final.
                </div>
            </CardContent>
        </Card>
    );
}
