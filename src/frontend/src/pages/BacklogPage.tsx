import React, { useEffect, useState } from 'react';
import { Layout } from '@/components/Layout';
import { PageHeader } from '@/components/PageHeader';
import { useAuth } from '@/hooks/useAuth';
import { backlogApi, type BacklogItemDto } from '@/api/backlogApi';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Trash2, Play, BookOpen, ClockAlert, BookOpenText } from 'lucide-react';
import { useToast } from '@/hooks/use-toast';
import { useNavigate } from 'react-router';
import { SourceTypes } from '@/models/Enums';
import {
    Tooltip,
    TooltipContent,
    TooltipTrigger,
} from "@/components/ui/tooltip";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";

export const BacklogPage: React.FC = () => {
    const { user } = useAuth();
    const { toast } = useToast();
    const navigate = useNavigate();
    const [items, setItems] = useState<BacklogItemDto[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [deleteItemHash, setDeleteItemHash] = useState<string | null>(null);

    const loadBacklog = async () => {
        if (!user) return;
        setIsLoading(true);
        try {
            const data = await backlogApi.getAll(user.id);
            setItems(data);
        } catch (error) {
            console.error("Failed to load backlog", error);
            toast({ variant: "destructive", title: "Error", description: "Failed to load backlog." });
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        loadBacklog();
    }, [user]);

    const confirmRemove = (hash: string) => {
        setDeleteItemHash(hash);
    };

    const handleRemove = async () => {
        if (!user || !deleteItemHash) return;
        try {
            await backlogApi.remove(user.id, deleteItemHash);
            toast({ title: "Removed", description: "Item removed from backlog." });
            setDeleteItemHash(null);
            loadBacklog();
        } catch (error) {
            console.error("Failed to remove", error);
            toast({ variant: "destructive", title: "Error", description: "Failed to remove item." });
        }
    };

    const handleStart = (item: BacklogItemDto) => {
        navigate(`/derot?start=true&type=${item.sourceType}&id=${encodeURIComponent(item.sourceId)}&hash=${item.sourceHash}`);
    };

    return (
        <Layout>
            <div className="container max-w-7xl mx-auto py-10 px-4 space-y-12">
                <PageHeader
                    title="Backlog"
                    subtitle="Learning Backlog"
                    description="Items you have committed to learn later. Start an activity directly from here."
                    icon={ClockAlert}
                    badgeIcon={ClockAlert}
                />

                <Card className="border-border/30 bg-card/30 backdrop-blur-xl rounded-3xl overflow-hidden shadow-xl">
                    <CardHeader className="p-8 border-b border-border/20">
                        <CardTitle className="text-2xl font-bold tracking-tight">Committed Learning</CardTitle>
                    </CardHeader>
                    <CardContent>
                        {isLoading ? (
                            <div>Loading...</div>
                        ) : items.length === 0 ? (
                            <div className="text-muted-foreground text-center py-8">Your backlog is empty. Add items from Wikipedia or Documents.</div>
                        ) : (
                            <div className="w-full overflow-auto">
                                <table className="w-full caption-bottom text-sm">
                                    <thead className="[&_tr]:border-b">
                                        <tr className="border-b transition-colors hover:bg-muted/50 data-[state=selected]:bg-muted">
                                            <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Title</th>
                                            <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Type</th>
                                            <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Added</th>
                                            <th className="h-12 px-4 text-right align-middle font-medium text-muted-foreground">Actions</th>
                                        </tr>
                                    </thead>
                                    <tbody className="[&_tr:last-child]:border-0">
                                        {items.map((item) => (
                                            <tr key={item.id} className="border-b transition-colors hover:bg-muted/50 data-[state=selected]:bg-muted">
                                                <td className="p-4 align-middle font-medium flex items-center gap-2">
                                                    {item.sourceType === SourceTypes.Document ? <BookOpen className="h-4 w-4" /> : <Play className="h-4 w-4" />}
                                                    {item.title}
                                                </td>
                                                <td className="p-4 align-middle">{item.sourceType}</td>
                                                <td className="p-4 align-middle">{new Date(item.addedAt).toLocaleDateString()}</td>
                                                <td className="p-4 align-middle text-right">
                                                    <div className="flex justify-end gap-1">
                                                        <Tooltip>
                                                            <TooltipTrigger asChild>
                                                                <Button
                                                                    variant="ghost"
                                                                    size="icon"
                                                                    onClick={() => handleStart(item)}
                                                                    className="text-primary hover:text-primary/80 hover:bg-primary/10"
                                                                >
                                                                    {item.sourceType === SourceTypes.Document ? <BookOpenText className="h-4 w-4" /> : <Play className="h-4 w-4" />}
                                                                </Button>
                                                            </TooltipTrigger>
                                                            <TooltipContent>
                                                                <p>{item.sourceType === SourceTypes.Document ? 'Read' : 'Start Activity'}</p>
                                                            </TooltipContent>
                                                        </Tooltip>

                                                        <Tooltip>
                                                            <TooltipTrigger asChild>
                                                                <Button
                                                                    variant="ghost"
                                                                    size="icon"
                                                                    onClick={() => confirmRemove(item.sourceHash)}
                                                                    className="text-destructive hover:text-destructive/90 hover:bg-destructive/10"
                                                                >
                                                                    <Trash2 className="h-4 w-4" />
                                                                </Button>
                                                            </TooltipTrigger>
                                                            <TooltipContent>
                                                                <p>Remove from Backlog</p>
                                                            </TooltipContent>
                                                        </Tooltip>
                                                    </div>
                                                </td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                            </div>
                        )}
                    </CardContent>
                </Card>

                <Dialog open={!!deleteItemHash} onOpenChange={(open) => !open && setDeleteItemHash(null)}>
                    <DialogContent>
                        <DialogHeader>
                            <DialogTitle>Remove from Backlog?</DialogTitle>
                            <DialogDescription>
                                This will remove this item from your backlog. You can find it again in the source library if you need to.
                            </DialogDescription>
                        </DialogHeader>
                        <DialogFooter>
                            <Button variant="outline" onClick={() => setDeleteItemHash(null)}>Cancel</Button>
                            <Button variant="destructive" onClick={handleRemove}>Remove</Button>
                        </DialogFooter>
                    </DialogContent>
                </Dialog>
            </div>
        </Layout>
    );
};
