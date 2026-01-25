import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router';
import { Layout } from '@/components/Layout';
import { PageHeader } from '@/components/PageHeader';
import { useAuth } from '@/hooks/useAuth';
import { documentApi, type DocumentDto } from '@/api/documentApi'; // Added type-only import
import { DocumentUpload } from '@/components/Documents/DocumentUpload';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Trash2, FileText, Library, BookOpenText, NotebookPen } from 'lucide-react';
// import { format } from 'date-fns'; // Removed
import { useToast } from '@/hooks/use-toast'; // Updated path
import {
    Tooltip,
    TooltipContent,
    TooltipProvider,
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
// Table imports removed

export const DocumentsPage: React.FC = () => {
    const { user } = useAuth();
    const { toast } = useToast();
    const navigate = useNavigate();
    const [documents, setDocuments] = useState<DocumentDto[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [deleteDocId, setDeleteDocId] = useState<string | null>(null);

    const loadDocuments = async () => {
        if (!user) return;
        setIsLoading(true);
        try {
            const data = await documentApi.getAll(user.id);
            setDocuments(data);
        } catch (error) {
            console.error("Failed to load documents", error);
            toast({ variant: "destructive", title: "Error", description: "Failed to load documents." });
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        loadDocuments();
    }, [user]);

    const confirmDelete = (docId: string) => {
        setDeleteDocId(docId);
    };

    const handleDelete = async () => {
        if (!user || !deleteDocId) return;
        try {
            await documentApi.delete(user.id, deleteDocId);
            toast({ title: "Deleted", description: "Document deleted successfully." });
            setDeleteDocId(null);
            loadDocuments();
        } catch (error) {
            console.error("Failed to delete", error);
            toast({ variant: "destructive", title: "Error", description: "Failed to delete document." });
        }
    };

    const handleRead = (doc: DocumentDto) => {
        navigate(`/zone?start=true&type=Document&id=${encodeURIComponent(doc.sourceHash)}`);
    };

    const handleQuiz = (doc: DocumentDto) => {
        navigate(`/zone?start=true&type=Document&mode=quiz&id=${encodeURIComponent(doc.sourceHash)}`);
    };

    return (
        <Layout>
            <div className="container mx-auto py-8 space-y-8">
                <PageHeader
                    title="My Documents"
                    subtitle="Document Library"
                    description="Upload and manage your learning resources. Documents extracted here are available for adding to your backlog."
                    icon={Library}
                />

                <section className="max-w-xl">
                    <DocumentUpload onUploadComplete={loadDocuments} />
                </section>

                <Card>
                    <CardHeader>
                        <CardTitle>Library</CardTitle>
                    </CardHeader>
                    <CardContent>
                        {isLoading ? (
                            <div>Loading...</div>
                        ) : documents.length === 0 ? (
                            <div className="text-muted-foreground text-center py-8">No documents uploaded yet.</div>
                        ) : (
                            <div className="w-full overflow-auto">
                                <table className="w-full caption-bottom text-sm">
                                    <thead className="[&_tr]:border-b">
                                        <tr className="border-b transition-colors hover:bg-muted/50 data-[state=selected]:bg-muted">
                                            <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Title</th>
                                            <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Type</th>
                                            <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Size</th>
                                            <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Uploaded</th>
                                            <th className="h-12 px-4 text-right align-middle font-medium text-muted-foreground">Actions</th>
                                        </tr>
                                    </thead>
                                    <tbody className="[&_tr:last-child]:border-0">
                                        {documents.map((doc) => (
                                            <tr key={doc.id} className="border-b transition-colors hover:bg-muted/50 data-[state=selected]:bg-muted">
                                                <td className="p-4 align-middle font-medium flex items-center gap-2">
                                                    <FileText className="h-4 w-4" />
                                                    {doc.displayTitle}
                                                </td>
                                                <td className="p-4 align-middle">{doc.fileType}</td>
                                                <td className="p-4 align-middle">{(doc.fileSize / 1024).toFixed(1)} KB</td>
                                                <td className="p-4 align-middle">{new Date(doc.uploadDate).toLocaleDateString()}</td>
                                                <td className="p-4 align-middle text-right">
                                                    <div className="flex justify-end gap-1">
                                                        <TooltipProvider>
                                                            <Tooltip>
                                                                <TooltipTrigger asChild>
                                                                    <Button
                                                                        variant="ghost"
                                                                        size="icon"
                                                                        onClick={() => handleRead(doc)}
                                                                        className="text-primary hover:text-primary/80 hover:bg-primary/10"
                                                                    >
                                                                        <BookOpenText className="h-4 w-4" />
                                                                    </Button>
                                                                </TooltipTrigger>
                                                                <TooltipContent>
                                                                    <p>Read Document</p>
                                                                </TooltipContent>
                                                            </Tooltip>
                                                        </TooltipProvider>

                                                        <TooltipProvider>
                                                            <Tooltip>
                                                                <TooltipTrigger asChild>
                                                                    <Button
                                                                        variant="ghost"
                                                                        size="icon"
                                                                        onClick={() => handleQuiz(doc)}
                                                                        className="text-primary hover:text-primary/80 hover:bg-primary/10"
                                                                    >
                                                                        <NotebookPen className="h-4 w-4" />
                                                                    </Button>
                                                                </TooltipTrigger>
                                                                <TooltipContent>
                                                                    <p>Start Quiz</p>
                                                                </TooltipContent>
                                                            </Tooltip>
                                                        </TooltipProvider>

                                                        <TooltipProvider>
                                                            <Tooltip>
                                                                <TooltipTrigger asChild>
                                                                    <Button
                                                                        variant="ghost"
                                                                        size="icon"
                                                                        onClick={() => confirmDelete(doc.id)}
                                                                        className="text-destructive hover:text-destructive/90 hover:bg-destructive/10"
                                                                    >
                                                                        <Trash2 className="h-4 w-4" />
                                                                    </Button>
                                                                </TooltipTrigger>
                                                                <TooltipContent>
                                                                    <p>Delete Document</p>
                                                                </TooltipContent>
                                                            </Tooltip>
                                                        </TooltipProvider>
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

                <Dialog open={!!deleteDocId} onOpenChange={(open) => !open && setDeleteDocId(null)}>
                    <DialogContent>
                        <DialogHeader>
                            <DialogTitle>Are you sure?</DialogTitle>
                            <DialogDescription>
                                This action cannot be undone. This will permanently delete the document and remove it from your library.
                            </DialogDescription>
                        </DialogHeader>
                        <DialogFooter>
                            <Button variant="outline" onClick={() => setDeleteDocId(null)}>Cancel</Button>
                            <Button variant="destructive" onClick={handleDelete}>Delete</Button>
                        </DialogFooter>
                    </DialogContent>
                </Dialog>
            </div>
        </Layout>
    );
};
