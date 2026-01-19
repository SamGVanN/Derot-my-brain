import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { AlertTriangle } from 'lucide-react';

interface DeleteAccountModalProps {
    isOpen: boolean;
    onClose: () => void;
    onConfirm: () => void;
    userName: string;
}

export function DeleteAccountModal({ isOpen, onClose, onConfirm, userName }: DeleteAccountModalProps) {
    const { t } = useTranslation();
    const [confirmationText, setConfirmationText] = useState('');

    const isConfirmationValid = confirmationText === userName;

    const handleConfirm = () => {
        if (isConfirmationValid) {
            onConfirm();
            setConfirmationText('');
            onClose();
        }
    };

    const handleClose = () => {
        setConfirmationText('');
        onClose();
    };

    return (
        <Dialog open={isOpen} onOpenChange={handleClose}>
            <DialogContent className="sm:max-w-md">
                <DialogHeader>
                    <div className="flex items-center gap-2">
                        <AlertTriangle className="h-6 w-6 text-destructive" />
                        <DialogTitle className="text-destructive">{t('profile.deleteAccount')}</DialogTitle>
                    </div>
                    <DialogDescription className="space-y-3 pt-2">
                        <p className="font-semibold text-destructive">
                            ⚠️ {t('profile.deleteAccountWarning')}
                        </p>
                        <p>{t('profile.deleteAccountDetails')}</p>
                        <ul className="list-disc list-inside space-y-1 text-sm">
                            <li>{t('profile.deleteAccountList.profile')}</li>
                            <li>{t('profile.deleteAccountList.history')}</li>
                            <li>{t('profile.deleteAccountList.tracked')}</li>
                            <li>{t('profile.deleteAccountList.preferences')}</li>
                        </ul>
                    </DialogDescription>
                </DialogHeader>

                <div className="space-y-2">
                    <Label htmlFor="confirmation">
                        {t('profile.deleteAccountConfirm')}
                    </Label>
                    <Input
                        id="confirmation"
                        value={confirmationText}
                        onChange={(e) => setConfirmationText(e.target.value)}
                        placeholder={userName}
                        className="font-mono"
                    />
                </div>

                <DialogFooter className="gap-2 sm:gap-0">
                    <Button variant="outline" onClick={handleClose}>
                        {t('common.cancel')}
                    </Button>
                    <Button
                        variant="destructive"
                        onClick={handleConfirm}
                        disabled={!isConfirmationValid}
                    >
                        {t('profile.deleteAccountButton')}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}
