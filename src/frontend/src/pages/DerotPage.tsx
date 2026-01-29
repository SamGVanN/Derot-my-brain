import { useTranslation } from 'react-i18next';
import { Layout } from '@/components/Layout';
import { BrainCircuit } from 'lucide-react';
import { PageHeader } from '@/components/PageHeader';
import { DerotZoneSubHeader } from '@/components/DerotZone/DerotZoneSubHeader';
import { ExploreView } from '@/components/DerotZone/ExploreView';
import { ReadView } from '@/components/DerotZone/ReadView';
import { QuizView } from '@/components/DerotZone/QuizView';
import { useSearchParams, useNavigate } from 'react-router';
import { useWikipediaExplore } from '@/hooks/useWikipediaExplore';
import { useMemo, useEffect, useRef } from 'react';
import { useActivities } from '@/hooks/useActivities';
import { useToast } from '@/hooks/use-toast';

export function DerotPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { toast } = useToast();
  const [searchParams, setSearchParams] = useSearchParams();
  const { articles, isInitializing, refresh, error, stopExplore, readArticle, addToBacklog, loadingAction, initExplore, exploreId } = useWikipediaExplore();
  const { updateActivity } = useActivities();

  const activityId = searchParams.get('activityId');
  const paramMode = searchParams.get('mode');

  // Use refs to avoid stale closures in cleanup
  const currentActivityIdRef = useRef<string | null>(null);
  const startTimeRef = useRef<number | null>(null);
  const stopExploreRef = useRef(stopExplore);
  const updateActivityRef = useRef(updateActivity);

  useEffect(() => {
    stopExploreRef.current = stopExplore;
    updateActivityRef.current = updateActivity;
  }, [stopExplore, updateActivity]);

  const mode = useMemo(() => {
    if (!activityId) return 'EXPLORE';
    if (paramMode === 'quiz') return 'QUIZ';
    return 'READ';
  }, [activityId, paramMode]);

  const saveCurrentDuration = async () => {
    if (currentActivityIdRef.current && startTimeRef.current) {
      const duration = Math.floor((Date.now() - startTimeRef.current) / 1000);
      if (duration > 0) {
        try {
          await updateActivityRef.current(currentActivityIdRef.current, {
            durationSeconds: duration,
            sessionDateEnd: new Date().toISOString(),
            isCompleted: true
          });
        } catch (err) {
          console.error('Failed to save activity duration', err);
        }
      }
      startTimeRef.current = null;
    }
  };

  useEffect(() => {
    // If we're changing activity or mode, save the previous one's duration
    if (currentActivityIdRef.current && currentActivityIdRef.current !== activityId) {
      saveCurrentDuration();
    }

    currentActivityIdRef.current = activityId;

    if (activityId && (mode === 'READ' || mode === 'QUIZ')) {
      startTimeRef.current = Date.now();
    } else {
      startTimeRef.current = null;
    }
  }, [activityId, mode]);

  // Handle 'start' parameters (e.g. from Backlog)
  useEffect(() => {
    const start = searchParams.get('start');
    const id = searchParams.get('id');

    if (start === 'true' && id) {
      // Clear start param to avoid re-triggering
      const newParams = new URLSearchParams(searchParams);
      newParams.delete('start');
      setSearchParams(newParams);

      const handleRead = async () => {
        const article: any = {
          title: searchParams.get('title') || "Article",
          sourceUrl: id,
          lang: searchParams.get('lang') || undefined
        };
        const activity = await readArticle(article);
        const activityId = activity?.id || (activity as any)?.Id;
        if (activityId) {
          setSearchParams({ activityId });
        }
      };

      handleRead();
    }
  }, [searchParams, setSearchParams, readArticle]);


  // Cleanup: Quitting DerotZone is quitting the UserSession and saving duration
  useEffect(() => {
    return () => {
      // This runs on unmount.
      saveCurrentDuration();
      stopExploreRef.current();
    };
  }, []); // Only once on mount/unmount

  return (
    <Layout>
      <div className="container max-w-7xl mx-auto py-10 px-4 space-y-12">
        <PageHeader
          title={t('derot.title', 'Derot Zone')}
          subtitle={t('derot.subtitle', 'Derot Zone')}
          description={t('derot.description', "Explorez, lisez et testez vos connaissances. C'est ici que se passe l'essentiel de votre apprentissage.")}
          icon={BrainCircuit}
        />

        <section className="space-y-6 animate-in fade-in duration-1000">
          {(mode !== 'EXPLORE' || !!exploreId) && (
            <DerotZoneSubHeader
              mode={mode}
              onStopExplore={async () => {
                await saveCurrentDuration();
                await stopExplore();
                navigate('/focus-area');
              }}
              onGoToQuiz={async () => {
                await saveCurrentDuration();
                setSearchParams({ activityId: activityId!, mode: 'quiz' });
              }}
              onSubmitQuiz={async () => {
                await saveCurrentDuration();
                setSearchParams({});
              }}
            />
          )}

          {mode === 'EXPLORE' && (
            <ExploreView
              articles={articles}
              onRefresh={refresh}
              onStartExplore={initExplore}
              onRead={async (article) => {
                const activity = await readArticle(article);
                const id = activity?.id || (activity as any)?.Id;
                if (id) {
                  setSearchParams({ activityId: id });
                }
              }}
              onAddToBacklog={async (article) => {
                const success = await addToBacklog(article);
                if (success) {
                  toast({
                    title: "Backlog",
                    description: `"${article.title}" ajouté à votre backlog pour plus tard.`,
                  });
                }
                return success;
              }}
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

        {/* 
      //Not implemented -> Not worth showing in that particular case
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
        </section> */}
      </div>
    </Layout>
  );
}
