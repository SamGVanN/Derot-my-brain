import { useTranslation } from 'react-i18next';
import { Layout } from '@/components/Layout';
import DerotZone, { type ArticleCard } from '@/components/DerotZone/DerotZone';
import { Brain, Sparkles, TrendingUp } from 'lucide-react';

export function DerotPage() {
  const { t } = useTranslation();

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
        <section className="flex flex-col items-center text-center space-y-4 pt-4">
          <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-primary/10 text-primary text-sm font-medium animate-in fade-in slide-in-from-top-4 duration-500">
            <Sparkles className="h-4 w-4" />
            <span>Discover Knowledge</span>
          </div>
          <h1 className="text-4xl md:text-6xl font-extrabold tracking-tight bg-clip-text text-transparent bg-gradient-to-r from-foreground via-foreground/80 to-muted-foreground animate-in fade-in slide-in-from-bottom-4 duration-500">
            {t('derot.title', 'Derot Zone')}
          </h1>
          <p className="text-muted-foreground text-lg max-w-2xl animate-in fade-in slide-in-from-bottom-4 duration-700">
            {t('derot.description', 'Explore curated Wikipedia articles tailored to your interests. Expand your brain, one article at a time.')}
          </p>
        </section>

        <section className="space-y-8 animate-in fade-in duration-1000">
          <div className="flex items-center justify-between border-b pb-4 border-border/40">
            <div className="flex items-center gap-2">
              <TrendingUp className="h-5 w-5 text-primary" />
              <h2 className="text-2xl font-bold tracking-tight">Trending Topics</h2>
            </div>
          </div>

          <DerotZone articles={sampleArticles} />
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
