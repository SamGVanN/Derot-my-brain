import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useUser } from '../hooks/useUser';
import type { User } from '../models/User';
import { useTheme } from '@/components/theme-provider';
import { Layout } from '@/components/Layout';

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { ScrollArea } from '@/components/ui/scroll-area';
import { User as UserIcon, ArrowRight, Loader2 } from 'lucide-react';


interface UserSelectionPageProps {
    onUserSelected: (data: { user: User; token: string }) => void;
}

export default function UserSelectionPage({ onUserSelected }: UserSelectionPageProps) {
    const { t, i18n } = useTranslation();
    const { theme } = useTheme();
    const [username, setUsername] = useState('');
    const queryClient = useQueryClient();

    // Custom Hooks
    const { getAllUsers, createOrSelectUser } = useUser();

    // Query to get all users
    const { data: users, isLoading, error } = useQuery({
        queryKey: ['users'],
        queryFn: getAllUsers,
    });

    // Mutation to create/select user
    const mutation = useMutation({
        mutationFn: (variables: { name: string; options?: { language?: string; preferredTheme?: string } }) =>
            createOrSelectUser(variables.name, variables.options),
        onSuccess: (data) => {
            onUserSelected(data);
            // Refetch users list (optimistic update or simple invalidate)
            queryClient.invalidateQueries({ queryKey: ['users'] });
            setUsername('');
        },

        onError: (err) => {
            alert(`Error: ${err}`);
        }
    });

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        if (username.trim()) {
            mutation.mutate({ name: username, options: { language: i18n.language, preferredTheme: theme.name } });
        }
    };

    const handleUserClick = (name: string) => {
        // For existing users, backend ignores preferences, so passing them is safe/harmless
        mutation.mutate({ name, options: { language: i18n.language, preferredTheme: theme.name } });
    };

    return (
        <Layout>
            <div className="flex flex-col items-center justify-center min-h-[60vh] gap-8">
                <div className="text-center space-y-2">
                    <h1 className="text-4xl font-extrabold tracking-tight lg:text-5xl bg-clip-text text-transparent bg-gradient-to-r from-primary to-violet-500">
                        {t('welcome.title')}
                    </h1>
                    <p className="text-muted-foreground text-lg">
                        {t('welcome.intro')}
                    </p>
                </div>

                <Card className="w-full max-w-md shadow-2xl border-t-4 border-t-primary/80 dark:bg-card/50 backdrop-blur-sm">
                    <CardHeader className="text-center">
                        <CardTitle className="text-xl">{t('userSelection.title')}</CardTitle>
                        <CardDescription>{t('userSelection.placeholder')}</CardDescription>
                    </CardHeader>
                    <CardContent className="space-y-6">

                        {/* New User Form */}
                        <form onSubmit={handleSubmit} className="flex gap-3">
                            <Input
                                placeholder={t('userSelection.placeholder')}
                                value={username}
                                onChange={(e) => setUsername(e.target.value)}
                                className="flex-1 h-11 text-lg"
                            />
                            <Button type="submit" disabled={mutation.isPending} size="lg" className="px-6">
                                {mutation.isPending ? <Loader2 className="animate-spin" /> : <ArrowRight className="h-5 w-5" />}
                            </Button>
                        </form>

                        {/* Existing Users List */}
                        <div className="space-y-4 pt-4 border-t border-border/50">
                            <h3 className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">{t('userSelection.selectProfile')}</h3>

                            {isLoading && <div className="flex justify-center p-4"><Loader2 className="h-6 w-6 animate-spin text-primary" /></div>}
                            {error && <p className="text-xs text-destructive text-center">{t('common.error')}</p>}

                            <ScrollArea className="h-[200px] pr-4">
                                {users?.length === 0 && (
                                    <p className="text-center text-sm text-muted-foreground py-8">{t('userSelection.newUser')}</p>
                                )}

                                <div className="grid gap-2">
                                    {users?.map((user) => (
                                        <Button
                                            key={user.name}
                                            variant="outline"
                                            className="w-full justify-start h-12 text-base font-normal transition-all group"
                                            onClick={() => handleUserClick(user.name)}
                                        >
                                            <div className="p-1 bg-muted rounded-full mr-3 transition-colors group-hover:bg-accent-foreground/10 group-hover:text-accent-foreground">
                                                <UserIcon className="h-4 w-4" />
                                            </div>

                                            {user.name}
                                        </Button>
                                    ))}
                                </div>
                            </ScrollArea>
                        </div>
                    </CardContent>
                </Card>
            </div>
        </Layout>
    );
}
