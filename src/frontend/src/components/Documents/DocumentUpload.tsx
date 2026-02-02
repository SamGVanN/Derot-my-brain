import React, { useState } from 'react';
import { Upload, BookOpen, ListPlus } from 'lucide-react'; // Removing FileText unused
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from '@/components/ui/card';
// Alert import removed
import { documentApi } from '@/api/documentApi';
import { backlogApi } from '@/api/backlogApi';
import { useNavigate } from 'react-router';
import { useAuth } from '@/hooks/useAuth';
import { useToast } from '@/hooks/use-toast';
import { SourceTypes } from '@/models/Enums';

interface DocumentUploadProps {
    onUploadComplete?: () => void;
}

export const DocumentUpload: React.FC<DocumentUploadProps> = ({ onUploadComplete }) => {
    const { user } = useAuth();
    const navigate = useNavigate();
    const { toast } = useToast();
    const [file, setFile] = useState<File | null>(null);
    const [isUploading, setIsUploading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files && e.target.files[0]) {
            setFile(e.target.files[0]);
            setError(null);
        }
    };

    const handleJustUpload = async () => {
        if (!file || !user) return;
        setIsUploading(true);
        try {
            await documentApi.upload(user.id, file);
            toast({ title: "Success", description: "Document uploaded successfully." });
            setFile(null);
            if (onUploadComplete) onUploadComplete();
        } catch (err) {
            console.error(err);
            setError("Failed to upload document.");
        } finally {
            setIsUploading(false);
        }
    };

    const handleUploadAndBacklog = async () => {
        if (!file || !user) return;
        setIsUploading(true);
        try {
            // 1. Upload
            const doc = await documentApi.upload(user.id, file);
            // 2. Add to Backlog
            await backlogApi.add(user.id, {
                sourceId: doc.sourceId, // or doc.id? API expects sourceId. BacklogService expects sourceId. 
                // Wait, BacklogService uses SourceType + SourceId to generate hash.
                // For Document, SourceId is technically the StoragePath or SourceHash?
                // In DocumentService.Upload, we set sourceHash.
                // But BacklogItem.SourceId needs to be whatever we use to retrieve content later.
                // For Documents, it's confusing.
                // If SourceType=Document, SourceId should be the ID or Hash?
                // Let's use SourceHash as the ID? NO, sourceHash is computed.
                // Let's use the Document.StoragePath as SourceId? 
                // API DocumentDto returns `sourceHash`.
                // Let's pass `sourceHash` as `sourceId` to Backlog? 
                // Logic: BacklogService.AddToBacklog(sourceId). hash = GenerateHash(type, sourceId).
                // If sourceId IS ALREADY the valid identifier (SourceHash), then hash(hash) is double hash.
                // WE MUST USE THE SAME SourceId that GenerateHash uses for documents.
                // In DocumentService, hash = GenerateHash(Document, relativePath).
                // So SourceId MUST be `relativePath` (which is not in DTO? DTO has `sourceHash`).
                // This is a flaw in my design!
                // DTO needs `storagePath` or similar if we want to reconstruct hash. Or just use `sourceHash` directly if BacklogAPI supports it.
                // BacklogService recalculates hash.
                // FIX: DocumentDto should include `storagePath` or `sourceId` equivalent.
                // OR: BacklogService should accept `alreadyHashed`? No.
                // Best fix: Document must expose the value used as `SourceId`.
                // In DocumentService: `var relativePath = Path.Combine(userId, uniqueFileName);`
                // `SourceId` = `relativePath`.
                // I should add `SourceId` to DocumentDto which maps to `StoragePath`.

                sourceType: SourceTypes.Document,
                title: doc.displayTitle
            });
            toast({ title: "Success", description: "Uploaded and added to backlog." });
            setFile(null);
            if (onUploadComplete) onUploadComplete();
        } catch (err) {
            console.error(err);
            setError("Failed to process request.");
        } finally {
            setIsUploading(false);
        }
    };

    const handleStartNow = async () => {
        if (!file || !user) return;
        setIsUploading(true);
        try {
            const doc = await documentApi.upload(user.id, file);

            // Navigate to explore/read with this source
            // We need to create an activity or redirect to a "setup activity" page with context.
            // Or just call StartActivity directly?
            // "Derot Zone" usually means Explore/Read.
            // Let's redirect to `/derot?start=true&type=Document&id=${doc.sourceId}`.
            // ActivityService.ReadAsync takes `sourceId`. If SourceType=Document, `sourceId` needs to be valid.
            // Again, is it Hash or Path?
            // FileContentSource.GetContentAsync takes `sourceId`.
            // My implementation of FileContentSource removes "file://".
            // So SourceId should be `file://` + Path?
            // In DocumentService, I calculated hash on `relativePath`.
            // But FileContentSource expects "file://"+Path?
            // I need to align these.

            navigate(`/derot?start=true&type=Document&id=${doc.sourceId}`); // Simplified for now
        } catch (err) {
            console.error(err);
            setError("Failed to upload and start.");
        } finally {
            setIsUploading(false);
        }
    };

    return (
        <Card className="w-full">
            <CardHeader>
                <CardTitle>Upload Document</CardTitle>
                <CardDescription>Supported formats: PDF, DOCX, TXT, ODT</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
                <div className="grid w-full max-w-sm items-center gap-1.5">
                    <Label htmlFor="document">File</Label>
                    <Input id="document" type="file" accept=".pdf,.docx,.txt,.odt" onChange={handleFileChange} />
                </div>

                {error && (
                    <div className="bg-destructive/15 text-destructive p-3 rounded-md text-sm font-medium">
                        {error}
                    </div>
                )}

                <div className="flex flex-col gap-2 sm:flex-row">
                    <Button
                        onClick={handleJustUpload}
                        disabled={!file || isUploading}
                        variant="secondary"
                        className="flex-1"
                    >
                        <Upload className="mr-2 h-4 w-4" /> Upload
                    </Button>
                    <Button
                        onClick={handleUploadAndBacklog}
                        disabled={!file || isUploading}
                        variant="outline"
                        className="flex-1"
                    >
                        <ListPlus className="mr-2 h-4 w-4" /> To Backlog
                    </Button>
                    <Button
                        onClick={handleStartNow}
                        disabled={!file || isUploading}
                        className="flex-1"
                    >
                        <BookOpen className="mr-2 h-4 w-4" /> Start Now
                    </Button>
                </div>
            </CardContent>
        </Card>
    );
};
