import { useTranslation } from 'react-i18next';
import { Layout } from '@/components/Layout';
import { type ArticleCard } from '@/components/DerotZone/DerotZone';
import { Brain } from 'lucide-react';
import { PageHeader } from '@/components/PageHeader';
import { DerotZoneSubHeader } from '@/components/DerotZone/DerotZoneSubHeader';
import { ExploreView } from '@/components/DerotZone/ExploreView';
import { ReadView } from '@/components/DerotZone/ReadView';
import { QuizView } from '@/components/DerotZone/QuizView';
import { useSearchParams, useNavigate } from 'react-router';
import { useWikipediaExplore } from '@/hooks/useWikipediaExplore';
import { useMemo } from 'react';

export function DerotPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const { isInitializing, refresh, error, stopExplore, readArticle, addToBacklog, loadingAction } = useWikipediaExplore();

  const activityId = searchParams.get('activityId');
  const paramMode = searchParams.get('mode');

  const mode = useMemo(() => {
    if (!activityId) return 'EXPLORE';
    if (paramMode === 'quiz') return 'QUIZ';
    return 'READ';
  }, [activityId, paramMode]);

  // Mock articles for the POC
  const sampleArticles: ArticleCard[] = [
    {
      title: 'Quantum computing',
      summary: 'Quantum computing is a type of computing that uses quantum-mechanical phenomena such as superposition and entanglement.',
      lang: 'en',
      sourceUrl: 'https://en.wikipedia.org/wiki/Quantum_computing'
    },
    {
      title: 'Artificial intelligence',
      summary: 'Artificial intelligence is intelligence demonstrated by machines, as opposed to the natural intelligence displayed by animals including humans.',
      lang: 'en',
      sourceUrl: 'https://en.wikipedia.org/wiki/Artificial_intelligence'
    },
    {
      title: 'Climate change',
      summary: 'Climate change includes both global warming driven by human-induced emissions of greenhouse gases and the resulting large-scale shifts in weather patterns.',
      lang: 'en',
      sourceUrl: 'https://en.wikipedia.org/wiki/Climate_change'
    },
    {
      title: 'Dopamine',
      summary: 'Dopamine is a neuromodulatory molecule that plays several important roles in cells. It is an organic chemical of the catecholamine and phenethylamine families.',
      lang: 'en',
      sourceUrl: 'https://en.wikipedia.org/wiki/Dopamine'
    },
    {
      title: 'Universal basic income',
      summary: 'Universal basic income (UBI) is a social security proposal in which all citizens of a given population regularly receive a guaranteed sum of money.',
      lang: 'en',
      sourceUrl: 'https://en.wikipedia.org/wiki/Universal_basic_income'
    },
    {
      title: 'Space exploration',
      summary: 'Space exploration is the use of astronomy and space technology to explore outer space.',
      lang: 'en',
      sourceUrl: 'https://en.wikipedia.org/wiki/Space_exploration'
    }
  ];

  return (
    <Layout>
      <div className="container max-w-7xl mx-auto py-10 px-4 space-y-12">
        <PageHeader
          title={t('derot.title', 'Derot Zone')}
          subtitle={t('derot.subtitle', 'Derot Zone')}
          description={t('derot.description', "Explorez, lisez et testez vos connaissances. C'est ici que se passe l'essentiel de votre apprentissage.")}
          icon={Brain}
        />

        <section className="space-y-6 animate-in fade-in duration-1000">
          <DerotZoneSubHeader
            mode={mode}
            onStopExplore={async () => {
              await stopExplore();
              navigate('/focus-area');
            }}
            onGoToQuiz={() => setSearchParams({ activityId: activityId!, mode: 'quiz' })}
            onSubmitQuiz={() => setSearchParams({})}
          />

          {mode === 'EXPLORE' && (
            <ExploreView
              articles={sampleArticles}
              onRefresh={refresh}
              onRead={async (article) => {
                const activity = await readArticle(article);
                if (activity?.id) {
                  setSearchParams({ activityId: activity.id });
                }
              }}
              onAddToBacklog={addToBacklog}
              loadingAction={loadingAction}
              isLoading={isInitializing}
            />
          )}

          {mode === 'READ' && (
            <ReadView activityId={activityId!} />
          )}

          {mode === 'QUIZ' && (
            <QuizView activityId={activityId!} />
          )}

          {error && (
            <div className="p-4 bg-destructive/10 text-destructive rounded-lg border border-destructive/20 text-center">
              {error}
            </div>
          )}
        </section>

        <section className="bg-primary/5 rounded-3xl p-8 md:p-12 border border-primary/10 flex flex-col md:flex-row items-center gap-8 animate-in fade-in duration-1000">
          <div className="bg-primary/10 p-6 rounded-2xl">
            <Brain className="h-16 w-16 text-primary" />
          </div>
          <div className="flex-1 space-y-4 text-center md:text-left">
            <h3 className="text-2xl font-bold">Personalized Recommendations</h3>
            <p className="text-muted-foreground max-w-xl">
              As you use Derot My Brain, we'll learn what you love and suggest articles that challenge and inspire you.
            </p>
          </div>
        </section>
      </div>
    </Layout>
  );
}
