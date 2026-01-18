import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { UserService } from '../services/UserService';
import type { User } from '../models/User';
import { Layout } from '@/components/Layout';

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { ScrollArea } from '@/components/ui/scroll-area';
import { User as UserIcon, ArrowRight, Loader2 } from 'lucide-react';


interface UserSelectionPageProps {
    onUserSelected: (user: User) => void;
}

export default function UserSelectionPage({ onUserSelected }: UserSelectionPageProps) {

    const [username, setUsername] = useState('');
    const queryClient = useQueryClient();

    // Query to get all users
    const { data: users, isLoading, error } = useQuery({
        queryKey: ['users'],
        queryFn: UserService.getAllUsers,
    });

    // Mutation to create/select user
    const mutation = useMutation({
        mutationFn: UserService.createOrSelectUser,
        onSuccess: (user) => {
            onUserSelected(user);
            // Refetch users list

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
            mutation.mutate(username);
        }
    };

    const handleUserClick = (name: string) => {
        mutation.mutate(name);
    };

    return (
        <Layout>
            <div className="flex flex-col items-center justify-center min-h-[60vh] gap-8">

                <div className="text-center space-y-2">
                    <h1 className="text-4xl font-extrabold tracking-tight lg:text-5xl bg-clip-text text-transparent bg-gradient-to-r from-primary to-violet-500">
                        Ready to learn?
                    </h1>
                    <p className="text-muted-foreground text-lg">
                        Pick up where you left off or start a new journey.
                    </p>
                </div>

                <Card className="w-full max-w-md shadow-2xl border-t-4 border-t-primary/80 dark:bg-card/50 backdrop-blur-sm">
                    <CardHeader className="text-center">
                        <CardTitle className="text-xl">Identify Yourself</CardTitle>
                        <CardDescription>Enter your name to access your brain.</CardDescription>
                    </CardHeader>
                    <CardContent className="space-y-6">

                        {/* New User Form */}
                        <form onSubmit={handleSubmit} className="flex gap-3">
                            <Input
                                placeholder="Your name..."
                                value={username}
                                onChange={(e) => setUsername(e.target.value)}
                                className="flex-1 h-11 text-lg"
                            />
                            <Button type="submit" disabled={mutation.isPending} size="lg" className="px-6">
                                {mutation.isPending ? <Loader2 className="animate-spin" /> : <ArrowRight className="h-5 w-5" />}
                            </Button>
                        </form>

                        {/* Existing Users List */}
                        <div className="space-y-3 pt-4 border-t border-border/50">
                            <h3 className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">Existing Users</h3>

                            {isLoading && <div className="flex justify-center p-4"><Loader2 className="h-6 w-6 animate-spin text-primary" /></div>}
                            {error && <p className="text-xs text-destructive text-center">Error loading users</p>}

                            <ScrollArea className="h-[200px] pr-4">
                                {users?.length === 0 && (
                                    <p className="text-center text-sm text-muted-foreground py-8">No users found. Be the first!</p>
                                )}

                                <div className="grid gap-2">
                                    {users?.map((user) => (
                                        <Button
                                            key={user.name}
                                            variant="outline"
                                            className="w-full justify-start h-12 text-base font-normal hover:border-primary/50 hover:bg-primary/5 transition-all group"
                                            onClick={() => handleUserClick(user.name)}
                                        >
                                            <div className="p-1 bg-muted rounded-full mr-3 group-hover:bg-primary/10 group-hover:text-primary transition-colors">
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
