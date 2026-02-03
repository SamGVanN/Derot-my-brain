import { type WikipediaArticle } from '@/api/wikipediaApi';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';
import { BookOpen, ClockAlert, Loader2, Radar, Brain, BrainCircuit } from 'lucide-react';
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { useTranslation } from 'react-i18next';

export type ArticleCard = WikipediaArticle;

type Props = {
  articles: ArticleCard[];
  onRead: (article: ArticleCard) => Promise<void>;
  onAddToBacklog: (article: ArticleCard) => Promise<boolean>;
  onStartExplore?: () => Promise<void>;
  loadingAction: string | null;
  isLoading?: boolean;
};

export default function DerotZone({ articles, onRead, onAddToBacklog, onStartExplore, loadingAction, isLoading }: Props) {
  const { t } = useTranslation();

  if (isLoading) {
    return (
      <div className="flex flex-col items-center justify-center py-20 space-y-4">
        <Loader2 className="w-10 h-10 animate-spin text-primary/40" />
        <p className="text-muted-foreground animate-pulse">Recherche d'articles passionnants...</p>
      </div>
    );
  }

  if (articles.length === 0) {
    if (onStartExplore) {
      return (
        <div className="flex flex-col items-center justify-center py-20 animate-in fade-in zoom-in duration-700">
          <div className="relative mb-8">
            <div className="absolute -inset-4 bg-primary/20 rounded-full blur-2xl animate-pulse" />
            <div className="relative bg-primary/10 p-8 rounded-3xl border border-primary/20">
              <BrainCircuit className="w-16 h-16 text-primary" />
            </div>
          </div>
          <h3 className="text-3xl font-bold tracking-tight mb-4">Prêt pour de nouvelles découvertes ?</h3>
          <p className="text-muted-foreground max-w-md text-center mb-8 leading-relaxed">
            Plongez dans un univers de connaissances avec des articles Wikipédia sélectionnés pour vous.
          </p>
          <Button
            size="lg"
            onClick={onStartExplore}
            className="px-8 py-6 text-lg font-semibold rounded-2xl shadow-xl shadow-primary/20 hover:shadow-primary/30 transition-all duration-300 gap-3"
          >
            <Radar className="w-6 h-6" />
            Start exploring
          </Button>
        </div>
      );
    }

    return (
      <div className="flex flex-col items-center justify-center p-12 text-center text-muted-foreground">
        <p>No articles found for the current selection.</p>
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 animate-in fade-in slide-in-from-bottom-4 duration-700">
      {articles.map((article, i) => {
        const isReading = loadingAction === `read-${article.title}`;
        const isAddingToBacklog = loadingAction === `backlog-${article.title}`;

        return (
          <Card key={i} className="group relative overflow-hidden border-border/20 bg-card/30 backdrop-blur-xl rounded-3xl hover:shadow-2xl hover:shadow-primary/10 transition-all duration-500 hover:-translate-y-2">
            <div className="absolute inset-0 bg-gradient-to-br from-primary/10 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-700 pointer-events-none" />

            {/* Image or Placeholder */}
            <div className="relative h-56 w-full overflow-hidden bg-muted/20">
              {article.imageUrl ? (
                <img
                  src={article.imageUrl}
                  alt={article.title}
                  className="w-full h-full object-cover transition-transform duration-700 group-hover:scale-110"
                />
              ) : article.sourceUrl?.includes('wikipedia.org') ? (
                <div className="w-full h-full flex items-center justify-center bg-muted/5 group-hover:scale-105 transition-transform duration-700">
                  <img
                    src="/images/wikipedia-logo.png"
                    alt="Wikipedia"
                    className="h-28 w-28 opacity-20 object-contain grayscale"
                  />
                </div>
              ) : (
                <div className="w-full h-full flex items-center justify-center bg-gradient-to-br from-primary/5 to-primary/10 group-hover:scale-105 transition-transform duration-700">
                  <Brain className="h-20 w-20 text-primary/10" />
                </div>
              )}
              <div className="absolute inset-0 bg-gradient-to-t from-background/90 via-background/20 to-transparent" />
            </div>

            <CardHeader className="-mt-14 relative z-10 p-6">
              <div className="flex justify-between items-start mb-3">
                <div className="h-10 w-10 rounded-2xl bg-primary/20 flex items-center justify-center text-primary group-hover:bg-primary group-hover:text-primary-foreground shadow-lg transition-all duration-500 backdrop-blur-xl border border-primary/20">
                  <Radar className="h-5 w-5" />
                </div>
                {article.lang && (
                  <span className="text-[10px] font-bold uppercase tracking-widest px-3 py-1 rounded-lg bg-background/80 text-primary border border-primary/10 backdrop-blur-md shadow-sm">
                    {article.lang}
                  </span>
                )}
              </div>
              <CardTitle className="text-2xl font-bold tracking-tight group-hover:text-primary transition-colors duration-300 line-clamp-2 min-h-[4rem]">
                {article.title}
              </CardTitle>
            </CardHeader>

            <CardContent className="px-6 pb-2">
              <CardDescription className="line-clamp-3 text-sm leading-relaxed text-muted-foreground/80 min-h-[4.5rem]">
                {article.summary || "No summary available for this Wikipedia article. Explore to learn more!"}
              </CardDescription>
            </CardContent>

            <CardFooter className="p-6 pt-2 flex gap-3">
              <Button
                onClick={() => onRead(article)}
                disabled={loadingAction !== null}
                className="flex-1 gap-2 rounded-xl shadow-lg shadow-primary/20 h-11 text-sm font-bold uppercase tracking-wider"
              >
                {isReading ? (
                  <Loader2 className="h-4 w-4 animate-spin" />
                ) : (
                  <BookOpen className="h-4 w-4" />
                )}
                Read
              </Button>
              <Tooltip>
                <TooltipTrigger asChild>
                  <Button
                    variant="outline"
                    size="icon"
                    onClick={() => onAddToBacklog(article)}
                    disabled={loadingAction !== null}
                    className="group/btn relative rounded-xl h-11 w-11 border-border/50 hover:border-primary/50 hover:bg-primary/5 transition-all"
                  >
                    {isAddingToBacklog ? (
                      <Loader2 className="h-4 w-4 animate-spin" />
                    ) : (
                      <ClockAlert className="h-5 w-5 group-hover/btn:scale-110 transition-transform text-muted-foreground group-hover/btn:text-primary" />
                    )}
                  </Button>
                </TooltipTrigger>
                <TooltipContent>
                  <p>{t('derot.explore.addToBacklog', 'Add to Backlog')}</p>
                </TooltipContent>
              </Tooltip>
            </CardFooter>
          </Card>
        );
      })}
    </div>
  );
}
