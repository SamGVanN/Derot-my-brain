import React from 'react';
import { useTranslation } from 'react-i18next';
import {
    BookOpen,
    Filter,
    ChevronDown,
    Settings2
} from 'lucide-react';
import { Button } from '@/components/ui/button';

export const ActivityView: React.FC = () => {
    const { t } = useTranslation();

    return (
        <div className="flex flex-col h-full space-y-6">
            {/* POC Filters Section */}
            <div className="sticky top-0 z-10 p-4 bg-card/80 backdrop-blur-md rounded-xl border border-border shadow-sm flex flex-wrap gap-4 items-center justify-between">
                <div className="flex items-center gap-4">
                    <div className="flex items-center gap-2 text-primary font-medium">
                        <Filter className="w-4 h-4" />
                        <span>Filters:</span>
                    </div>

                    {/* Mock Topic Selector */}
                    <div className="relative group">
                        <Button variant="outline" size="sm" className="flex items-center gap-2">
                            Topic: Quantum Physics
                            <ChevronDown className="w-4 h-4 opacity-50" />
                        </Button>
                    </div>

                    {/* Mock Difficulty */}
                    <div className="flex items-center bg-muted/50 rounded-lg p-1">
                        <Button variant="ghost" size="sm" className="h-7 text-xs bg-background shadow-sm text-foreground">Beginner</Button>
                        <Button variant="ghost" size="sm" className="h-7 text-xs text-muted-foreground hover:text-foreground">Intermediate</Button>
                        <Button variant="ghost" size="sm" className="h-7 text-xs text-muted-foreground hover:text-foreground">Advanced</Button>
                    </div>
                </div>

                <div className="flex items-center gap-2">
                    <Button variant="ghost" size="icon" className="h-8 w-8">
                        <Settings2 className="w-4 h-4" />
                    </Button>
                </div>
            </div>

            {/* Main Reading Container */}
            <div className="flex-1 overflow-auto rounded-xl border border-border bg-card/30">
                <div className="max-w-3xl mx-auto p-8 md:p-12">
                    <div className="prose dark:prose-invert max-w-none">
                        <h1 className="text-4xl md:text-5xl font-bold tracking-tight mb-6 bg-clip-text text-transparent bg-gradient-to-r from-primary to-violet-500">
                            Quantum Mechanics
                        </h1>

                        <div className="flex items-center gap-2 text-sm text-muted-foreground mb-8 pb-8 border-b border-border/50">
                            <BookOpen className="w-4 h-4" />
                            <span>Read time: ~15 min</span>
                            <span className="mx-2">•</span>
                            <span>Source: Wikipedia</span>
                        </div>

                        <div className="space-y-6 text-lg leading-relaxed text-foreground/90 font-serif md:font-sans md:text-xl md:leading-8">
                            <p className="first-letter:text-5xl first-letter:font-bold first-letter:mr-3 first-letter:float-left first-letter:text-primary">
                                Quantum mechanics is a fundamental theory in physics that provides a description of the physical properties of nature at the scale of atoms and subatomic particles. It is the foundation of all quantum physics including quantum chemistry, quantum field theory, quantum technology, and quantum information science.
                            </p>

                            <p>
                                Classical physics, the collection of theories that existed before the advent of quantum mechanics, describes many aspects of nature at an ordinary (macroscopic) scale, but is not sufficient for describing them at small (atomic and subatomic) scales. Most theories in classical physics can be derived from quantum mechanics as an approximation valid at large (macroscopic) scale.
                            </p>

                            <h3 className="text-2xl font-semibold mt-8 mb-4 text-foreground/80">History</h3>

                            <p>
                                Quantum mechanics was developed in the early 20th century, with the work of Max Planck and Albert Einstein being pivotal. Planck's quantum hypothesis states that energy is radiated and absorbed in discrete "quanta," or energy packets. Einstein later used this concept to explain the photoelectric effect, proposing that light itself is made of particles called photons.
                            </p>

                            <blockquote className="border-l-4 border-primary/50 pl-6 italic text-muted-foreground my-8 text-xl">
                                "Those who are not shocked when they first come across quantum theory cannot possibly have understood it."
                                <footer className="text-sm font-medium mt-2 text-primary">- Niels Bohr</footer>
                            </blockquote>

                            <p>
                                The wave-particle duality is a central concept of quantum mechanics, addressing the inability of the classical concepts "particle" or "wave" to fully describe the behavior of quantum-scale objects. As Albert Einstein wrote: "It seems as though we must use sometimes the one theory and sometimes the other, while at times we may use either."
                            </p>

                            <h3 className="text-2xl font-semibold mt-8 mb-4 text-foreground/80">Mathematical formulations</h3>

                            <p>
                                In the mathematically rigorous formulation of quantum mechanics, the state of a quantum mechanical system is a vector belonging to a complex separable Hilbert space. This vector is postulated to be normalized under the inner product of the Hilbert space.
                            </p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};
