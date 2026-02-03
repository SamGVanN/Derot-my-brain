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
import { useMemo, useEffect, useRef, useState } from 'react';
import { useActivities } from '@/hooks/useActivities';
import { useToast } from '@/hooks/use-toast';
import { HistoryTimeline } from '@/components/HistoryTimeline';
import { useUserFocus } from '@/hooks/useUserFocus';
import { useQuiz } from '@/hooks/useQuiz';
import { useAuth } from '@/hooks/useAuth';
import { activityApi } from '@/api/activityApi';

export function DerotPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { toast } = useToast();
  const [searchParams, setSearchParams] = useSearchParams();
  const { user } = useAuth();
  const { articles, isInitializing, refresh, error, stopExplore, readArticle, addToBacklog, loadingAction, initExplore, exploreId } = useWikipediaExplore();
  const { activities, updateActivity, refresh: refreshActivities, getActivity } = useActivities();
  const { userFocuses, trackSource, untrackSource } = useUserFocus();
  const [isTransitioning, setIsTransitioning] = useState(false);

  const {
    quiz,
    userAnswers,
    isLoading: isQuizLoading,
    error: quizError,
    result: quizResult,
    isSubmitting: isQuizSubmitting,
    generate: generateQuiz,
    selectAnswer: selectQuizAnswer,
    submit: submitQuiz,
    reset: resetQuiz,
    getDuration: getQuizDuration,
    allQuestionsAnswered
  } = useQuiz();

  const activityId = searchParams.get('activityId');
  const paramMode = searchParams.get('mode');

  // Use refs to avoid stale closures in cleanup
  const currentActivityIdRef = useRef<string | null>(null);
  const startTimeRef = useRef<number | null>(null);
  const stopExploreRef = useRef(stopExplore);
  const updateActivityRef = useRef(updateActivity);
  const isProcessingStart = useRef(false); // Guard for backlog navigation

  useEffect(() => {
    stopExploreRef.current = stopExplore;
    updateActivityRef.current = updateActivity;
  }, [stopExplore, updateActivity]);

  const mode = useMemo(() => {
    if (!activityId) return 'EXPLORE';
    if (paramMode === 'read') return 'READ';
    if (paramMode === 'quiz') return 'QUIZ';
    return 'READ'; // default if activityId is present
  }, [activityId, paramMode]);

  const currentItem = useMemo(() => {
    if (!activityId) return null;
    return activities.find(a => a.id === activityId) || null;
  }, [activities, activityId]);

  const saveCurrentDuration = async (overriddenId?: string, overriddenMode?: string) => {
    const modeToSave = overriddenMode || mode;

    // Get duration: for QUIZ use hook cumulative duration, otherwise use startTimeRef
    let duration = 0;
    if (modeToSave === 'QUIZ') {
      duration = getQuizDuration();
    } else if (startTimeRef.current) {
      duration = Math.floor((Date.now() - startTimeRef.current) / 1000);
    }

    if (duration <= 0) return;

    const idToSave = overriddenId || (modeToSave === 'EXPLORE' ? exploreId : activityId);

    if (idToSave) {
      try {
        if (modeToSave === 'EXPLORE') {
          await stopExploreRef.current(duration);
        } else {
          await updateActivityRef.current(idToSave, {
            durationSeconds: duration,
            sessionDateEnd: new Date().toISOString(),
            isCompleted: true
          });
        }
      } catch (err) {
        console.error('Failed to save activity duration', err);
      }
    }

    // Only reset if we're not providing overrides
    if (!overriddenId && !overriddenMode) {
      startTimeRef.current = null;
    }
  };

  // Track previous mode/id to handle transitions
  const prevModeRef = useRef(mode);
  const prevIdRef = useRef(activityId);

  useEffect(() => {
    // If we transition FROM something, save it using the PREVIOUS mode/id
    if (prevModeRef.current !== mode || prevIdRef.current !== activityId) {
      const idToSave = prevModeRef.current === 'EXPLORE' ? exploreId : prevIdRef.current;
      if (idToSave) {
        saveCurrentDuration(idToSave || undefined, prevModeRef.current);
      }
    }

    // Reset timer for the NEW activity/mode
    startTimeRef.current = Date.now();
    currentActivityIdRef.current = activityId;

    // Update refs for next change
    prevModeRef.current = mode;
    prevIdRef.current = activityId;
  }, [activityId, mode, exploreId]); // exploreId added here to trigger save when it's first created or cleared


  // Handle 'start' parameters (e.g. from Backlog)
  useEffect(() => {
    const start = searchParams.get('start');
    const id = searchParams.get('id');

    if (start === 'true' && id && !isProcessingStart.current) {
      isProcessingStart.current = true;

      const handleRead = async () => {
        try {
          const article: any = {
            title: searchParams.get('title') || "Article",
            sourceUrl: id, // THIS IS NOW SOURCE.ID (GUID) from Backlog
            lang: searchParams.get('lang') || undefined
          };
          const activity = await readArticle(article);
          const newActivityId = activity?.id || (activity as any)?.Id;

          if (newActivityId) {
            // Update params to the new activityId and clear start/id
            setSearchParams({ activityId: newActivityId }, { replace: true });
          }
        } catch (err) {
          console.error("Failed to process start parameter", err);
          toast({ variant: "destructive", title: "Erreur", description: "Impossible de lancer l'activité." });
        } finally {
          isProcessingStart.current = false;
        }
      };

      handleRead();
    }
  }, [searchParams, setSearchParams, readArticle, toast]);


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
        {mode === 'EXPLORE' && !exploreId && (
          <PageHeader
            title={t('derot.title', 'Derot Zone')}
            subtitle={t('derot.subtitle', 'Derot Zone')}
            description={t('derot.description', "Explorez, lisez et testez vos connaissances. C'est ici que se passe l'essentiel de votre apprentissage.")}
            icon={BrainCircuit}
            badgeIcon={BrainCircuit}
          />
        )}

        <section className="space-y-6 animate-in fade-in duration-1000">
          {(mode !== 'EXPLORE' || !!exploreId) && (
            <DerotZoneSubHeader
              mode={mode}
              onStopActivity={async () => {
                await saveCurrentDuration();
                resetQuiz(); // Important to clear quiz state if quitting
                navigate('/focus-area');
              }}
              onGoToQuiz={async () => {
                if (isTransitioning || !activityId || !user?.id) return;

                // Find current activity to get its source info
                let currentItem = activities.find(a => a.id === activityId);

                // If not found in local list, fetch it
                if (!currentItem) {
                  try {
                    currentItem = await getActivity(activityId);
                  } catch (err) {
                    console.error('Failed to get current activity details', err);
                    toast({
                      title: "Error",
                      description: "Impossible de récupérer les détails de l'activité en cours.",
                      variant: "destructive"
                    });
                    return;
                  }
                }

                if (!currentItem) return;

                setIsTransitioning(true);

                // 1. Stop current activity
                await saveCurrentDuration();

                // 2. Create NEW Quiz activity
                try {
                  const newQuizActivity = await activityApi.read(user.id, {
                    title: currentItem.title,
                    sourceId: currentItem.sourceId, // ALWAYS use technical GUID
                    sourceType: currentItem.sourceType!,
                    type: 'Quiz',
                    sessionId: currentItem.userSessionId
                  });

                  const newId = newQuizActivity?.id || (newQuizActivity as any)?.Id;
                  if (newId) {
                    setSearchParams({ activityId: newId, mode: 'quiz' });
                  }
                } catch (err) {
                  console.error('Failed to create quiz activity', err);
                  toast({
                    title: "Error",
                    description: "Échec de la création du nouveau quiz.",
                    variant: "destructive"
                  });
                } finally {
                  setIsTransitioning(false);
                }
              }}
              onSubmitQuiz={async () => {
                if (activityId) {
                  const duration = startTimeRef.current ? Math.floor((Date.now() - startTimeRef.current) / 1000) : 0;
                  await submitQuiz(activityId, duration);
                  refreshActivities();
                }
              }}
              isQuizSubmittable={allQuestionsAnswered}
              isSubmitting={isQuizSubmitting}
            />
          )}

          {mode === 'EXPLORE' && (
            <ExploreView
              articles={articles}
              onRefresh={refresh}
              onStartExplore={initExplore}
              onRead={async (article) => {
                const duration = startTimeRef.current ? Math.floor((Date.now() - startTimeRef.current) / 1000) : undefined;
                const activity = await readArticle(article, duration);
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
            <QuizView
              activityId={activityId!}
              onGetDuration={getQuizDuration}
              // Pass lifted quiz state
              quiz={quiz}
              userAnswers={userAnswers}
              isLoading={isQuizLoading}
              error={quizError}
              result={quizResult}
              isSubmitting={isQuizSubmitting}
              onGenerate={(id) => generateQuiz(id, currentItem?.durationSeconds)}
              onAnswerChange={selectQuizAnswer}
              onSubmit={submitQuiz}
              onFinish={() => {
                resetQuiz();
                navigate('/focus-area');
              }}
            />
          )}


          {error && (
            <div className="p-4 bg-destructive/10 text-destructive rounded-lg border border-destructive/20 text-center">
              {error}
            </div>
          )}
        </section>

        {/* Learning History Section */}
        {activities.length > 0 && mode === 'EXPLORE' && (
          <section className="space-y-8 animate-in fade-in duration-1000 pt-8 border-t border-border/40">
            <div className="flex items-center justify-between">
              <h2 className="text-2xl font-bold tracking-tight">{t('history.recentActivities', 'Recent Activities')}</h2>
            </div>
            <HistoryTimeline
              activities={activities.slice(0, 10)} // Show only top 10 recent
              userFocuses={userFocuses}
              onTrack={trackSource}
              onUntrack={untrackSource}
            />
          </section>
        )}

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
