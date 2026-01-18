import type { ReactNode } from "react";
import { Brain } from "lucide-react";
import { ThemeSelector } from "./theme-selector";

interface LayoutProps {
    children: ReactNode;
}

export function Layout({ children }: LayoutProps) {
    return (
        <div className="min-h-screen flex flex-col font-sans text-foreground selection:bg-primary/20">

            {/* Header */}
            <header className="sticky top-0 z-50 w-full border-b border-border/40 bg-background/80 backdrop-blur-md supports-[backdrop-filter]:bg-background/60">
                <div className="container mx-auto px-4 h-16 flex items-center justify-between max-w-5xl">
                    <div className="flex items-center gap-2">
                        <div className="p-1.5 bg-primary/10 rounded-lg">
                            <Brain className="h-6 w-6 text-primary" />
                        </div>
                        <span className="text-xl font-bold bg-gradient-to-r from-primary to-violet-600 bg-clip-text text-transparent">
                            Derot My Brain
                        </span>
                    </div>
                    <nav className="text-sm font-medium text-muted-foreground flex gap-4 items-center">
                        <ThemeSelector />
                    </nav>
                </div>
            </header>

            {/* Main Content */}
            <main className="flex-1 container mx-auto px-4 py-8 max-w-5xl animate-in fade-in duration-500">
                {children}
            </main>

            {/* Footer */}
            <footer className="border-t border-border/40 bg-background/50 backdrop-blur-sm mt-auto">
                <div className="container mx-auto px-4 py-6 max-w-5xl flex flex-col md:flex-row justify-between items-center gap-4 text-sm text-muted-foreground">
                    <p>© 2026 Derot My Brain. Local Learning.</p>
                    <div className="flex gap-4">
                        <span>v0.1.0</span>
                    </div>
                </div>
            </footer>
        </div>
    );
}
