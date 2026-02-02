import React, { useState } from 'react';
import { useNavigate } from 'react-router';
import { Layout } from '@/components/Layout';
import { PageHeader } from '@/components/PageHeader';
import { useAuth } from '@/hooks/useAuth';
import { useDocuments } from '@/hooks/useDocuments';
import { type DocumentDto } from '@/api/documentApi';
import { DocumentUpload } from '@/components/Documents/DocumentUpload';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Trash2, FileText, Library, BookOpen, NotebookPen } from 'lucide-react';
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

export const DocumentsPage: React.FC = () => {
    const { user } = useAuth();
    const navigate = useNavigate();
    const { documents, isLoading, deleteDocument, readDocument, refresh } = useDocuments(user?.id);
    const [deleteDocId, setDeleteDocId] = useState<string | null>(null);

    const confirmDelete = (docId: string) => {
        setDeleteDocId(docId);
    };

    const handleDelete = async () => {
        if (!deleteDocId) return;
        try {
            await deleteDocument(deleteDocId);
            setDeleteDocId(null);
        } catch (error) {
            // Error handled by hook
        }
    };

    const handleRead = (doc: DocumentDto) => {
        readDocument(doc);
    };

    const handleQuiz = (doc: DocumentDto) => {
        navigate(`/derot?start=true&type=Document&mode=quiz&id=${encodeURIComponent(doc.sourceId)}`);
    };

    return (
        <Layout>
            <div className="container max-w-7xl mx-auto py-10 px-4 space-y-12">
                <PageHeader
                    title="My Documents"
                    subtitle="Document Library"
                    description="Upload and manage your learning resources. Documents extracted here are available for adding to your backlog."
                    icon={Library}
                />

                <section className="max-w-xl">
                    <DocumentUpload onUploadComplete={refresh} />
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
                                            {/* <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Path</th> */}
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
                                                {/* <td className="p-4 align-middle text-muted-foreground text-xs font-mono max-w-[200px] truncate" title={doc.storagePath}>
                                                    <div className="flex items-center gap-1">
                                                        <FolderOpen className="h-3 w-3" />
                                                        {doc.storagePath}
                                                    </div>
                                                </td> */}
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
                                                                        <BookOpen className="h-4 w-4" />
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
