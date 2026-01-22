import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router';
import { Layout } from '@/components/Layout';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Label } from '@/components/ui/label';
import { Separator } from '@/components/ui/separator';
import { AlertTriangle, Edit, Save, X } from 'lucide-react';
import { userApi } from '@/api/userApi';
import { useAuth } from '@/hooks/useAuth';
import { DeleteAccountModal } from '@/components/profile/DeleteAccountModal';

export function UserProfilePage() {
    const { t } = useTranslation();
    const { user, logout, updateUser } = useAuth();
    const navigate = useNavigate();
    const [isEditing, setIsEditing] = useState(false);
    const [editedName, setEditedName] = useState(user?.name || '');
    const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
    const [isLoading, setIsLoading] = useState(false);

    if (!user) {
        navigate('/');
        return null;
    }

    const handleSave = async () => {
        if (!editedName.trim()) return;

        try {
            setIsLoading(true);
            const updatedUser = await userApi.updateUser(user.id, { name: editedName });
            updateUser(updatedUser);
            setIsEditing(false);
        } catch (error) {
            console.error('Failed to update user name:', error);
            // Ideally show a toast here
        } finally {
            setIsLoading(false);
        }
    };

    const handleCancel = () => {
        setEditedName(user.name);
        setIsEditing(false);
    };

    const handleDeleteAccount = async () => {
        // TODO: Call API to delete user
        console.log('Deleting account for user:', user.id);
        logout();
        navigate('/');
    };

    const formatDate = (date: string) => {
        return new Date(date).toLocaleDateString(undefined, {
            year: 'numeric',
            month: 'long',
            day: 'numeric',
        });
    };

    return (
        <Layout>
            <div className="container max-w-4xl mx-auto py-8 px-4 space-y-6">
                {/* Header */}
                <div className="space-y-2">
                    <h1 className="text-3xl font-bold tracking-tight">{t('profile.title')}</h1>
                    <p className="text-muted-foreground">{t('profile.subtitle', 'Manage your account information')}</p>
                </div>

                {/* Profile Information Card */}
                <Card>
                    <CardHeader>
                        <div className="flex items-center justify-between">
                            <div>
                                <CardTitle>{t('profile.title')}</CardTitle>
                                <CardDescription>{t('profile.description', 'Your personal information')}</CardDescription>
                            </div>
                            {!isEditing && (
                                <Button variant="outline" size="sm" onClick={() => setIsEditing(true)} className="gap-2">
                                    <Edit className="h-4 w-4" />
                                    {t('common.edit')}
                                </Button>
                            )}
                        </div>
                    </CardHeader>
                    <CardContent className="space-y-4">
                        {/* Name Field */}
                        <div className="space-y-2">
                            <Label htmlFor="name">{t('profile.name')}</Label>
                            {isEditing ? (
                                <div className="flex gap-2">
                                    <Input
                                        id="name"
                                        value={editedName}
                                        onChange={(e) => setEditedName(e.target.value)}
                                        placeholder={t('profile.name')}
                                    />
                                    <Button size="icon" variant="default" onClick={handleSave} disabled={isLoading}>
                                        <Save className="h-4 w-4" />
                                    </Button>
                                    <Button size="icon" variant="ghost" onClick={handleCancel}>
                                        <X className="h-4 w-4" />
                                    </Button>
                                </div>
                            ) : (
                                <p className="text-lg font-medium">{user.name}</p>
                            )}
                        </div>

                        <Separator />

                        {/* User ID */}
                        <div className="space-y-2">
                            <Label>{t('profile.userId')}</Label>
                            <p className="text-sm text-muted-foreground font-mono">{user.id}</p>
                        </div>

                        {/* Created At */}
                        <div className="space-y-2">
                            <Label>{t('profile.createdAt')}</Label>
                            <p className="text-sm">{formatDate(user.createdAt)}</p>
                        </div>

                        {/* Last Connection */}
                        <div className="space-y-2">
                            <Label>{t('profile.lastConnection')}</Label>
                            <p className="text-sm">{user.lastConnectionAt ? formatDate(user.lastConnectionAt) : 'N/A'}</p>
                        </div>
                    </CardContent>
                </Card>

                {/* Statistics Card */}
                <Card>
                    <CardHeader>
                        <CardTitle>{t('profile.statistics')}</CardTitle>
                        <CardDescription>{t('profile.statsDescription', 'Your learning progress')}</CardDescription>
                    </CardHeader>
                    <CardContent className="grid grid-cols-2 gap-4">
                        <div className="space-y-1">
                            <p className="text-sm text-muted-foreground">{t('profile.totalActivities')}</p>
                            <p className="text-2xl font-bold">0</p>
                        </div>
                        <div className="space-y-1">
                            <p className="text-sm text-muted-foreground">{t('profile.trackedTopicsCount')}</p>
                            <p className="text-2xl font-bold">0</p>
                        </div>
                    </CardContent>
                </Card>

                {/* Danger Zone */}
                <Card className="border-destructive">
                    <CardHeader>
                        <div className="flex items-center gap-2">
                            <AlertTriangle className="h-5 w-5 text-destructive" />
                            <CardTitle className="text-destructive">{t('profile.dangerZone')}</CardTitle>
                        </div>
                        <CardDescription>{t('profile.dangerZoneDescription', 'Irreversible actions')}</CardDescription>
                    </CardHeader>
                    <CardContent>
                        <Button
                            variant="destructive"
                            onClick={() => setIsDeleteModalOpen(true)}
                        >
                            {t('profile.deleteAccount')}
                        </Button>
                    </CardContent>
                </Card>

                {/* Delete Account Modal */}
                <DeleteAccountModal
                    isOpen={isDeleteModalOpen}
                    onClose={() => setIsDeleteModalOpen(false)}
                    onConfirm={handleDeleteAccount}
                    userName={user.name}
                />
            </div>
        </Layout>
    );
}
