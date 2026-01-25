import { type WikipediaArticle } from '@/api/wikipediaApi';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';
import { BookOpen, ClockAlert, Loader2, Sparkles, ExternalLink } from 'lucide-react';

export type ArticleCard = WikipediaArticle;

type Props = {
  articles: ArticleCard[];
  onRead: (article: ArticleCard) => Promise<void>;
  onAddToBacklog: (article: ArticleCard) => Promise<boolean>;
  loadingAction: string | null;
};

export default function DerotZone({ articles, onRead, onAddToBacklog, loadingAction }: Props) {

  if (articles.length === 0) {
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
          <Card key={i} className="group relative overflow-hidden border-border/50 bg-card/50 backdrop-blur-sm hover:shadow-2xl hover:shadow-primary/10 transition-all duration-300 hover:-translate-y-1">
            <div className="absolute inset-0 bg-gradient-to-br from-primary/5 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-500" />

            <CardHeader>
              <div className="flex justify-between items-start mb-2">
                <div className="h-8 w-8 rounded-full bg-primary/10 flex items-center justify-center text-primary group-hover:bg-primary group-hover:text-primary-foreground transition-colors duration-300">
                  <Sparkles className="h-4 w-4" />
                </div>
                {article.lang && (
                  <span className="text-[10px] font-bold uppercase tracking-wider px-2 py-0.5 rounded-full bg-muted text-muted-foreground">
                    {article.lang}
                  </span>
                )}
              </div>
              <CardTitle className="text-xl group-hover:text-primary transition-colors duration-300">
                {article.title}
              </CardTitle>
            </CardHeader>

            <CardContent>
              <CardDescription className="line-clamp-3 text-sm leading-relaxed min-h-[4.5rem]">
                {article.summary || "No summary available for this Wikipedia article. Explore to learn more!"}
              </CardDescription>
            </CardContent>

            <CardFooter className="flex gap-3 pt-2">
              <Button
                onClick={() => onRead(article)}
                disabled={loadingAction !== null}
                className="flex-1 gap-2 shadow-lg shadow-primary/20"
              >
                {isReading ? (
                  <Loader2 className="h-4 w-4 animate-spin" />
                ) : (
                  <BookOpen className="h-4 w-4" />
                )}
                Read
              </Button>
              <Button
                variant="outline"
                size="icon"
                onClick={() => onAddToBacklog(article)}
                disabled={loadingAction !== null}
                title="Add to Backlog"
                className="group/btn relative"
              >
                {isAddingToBacklog ? (
                  <Loader2 className="h-4 w-4 animate-spin" />
                ) : (
                  <ClockAlert className="h-4 w-4 group-hover/btn:scale-110 transition-transform" />
                )}
              </Button>
              {article.sourceUrl && (
                <Button variant="ghost" size="icon" asChild title="View on Wikipedia">
                  <a href={article.sourceUrl} target="_blank" rel="noopener noreferrer">
                    <ExternalLink className="h-4 w-4" />
                  </a>
                </Button>
              )}
            </CardFooter>
          </Card>
        );
      })}
    </div>
  );
}
