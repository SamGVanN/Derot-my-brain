

# OllamaLlmService

Il ne faut pas hardcoder num_ctx = 16384
Mais plutôt :
- exposer ça comme une option de stratégie
- ou au moins une constante configurable

Par exemple :
Quiz court → 4k
Article Wikipédia → 8k
Document utilisateur long → 16k
MAIS ça doit être en accord avec le model utilisé

Ça colle très bien avec :
- ILlmService
- une future LlmRequestOptions
- ou une policy côté Infrastructure


# DTO propre : LlmRequestOptions

```C#
namespace DerotMyBrain.Core.Llm;

public class LlmRequestOptions
{
    /// <summary>
    /// Taille maximale du contexte (tokens).
    /// Si null, une stratégie automatique sera appliquée.
    /// </summary>
    public int? MaxContextTokens { get; init; }

    /// <summary>
    /// Réserve de tokens pour la réponse du modèle.
    /// </summary>
    public int ResponseTokens { get; init; } = 512;

    /// <summary>
    /// Niveau de complexité attendu de la tâche.
    /// </summary>
    public LlmTaskComplexity Complexity { get; init; } = LlmTaskComplexity.Standard;
}
```

```C#
public enum LlmTaskComplexity
{
    Simple,     // QCM simple, résumé court
    Standard,   // Quiz classique
    Deep        // Quiz complexe, analyse poussée
}

```

Important :
aucune notion d’Ollama
pas de num_ctx ici → vocabulaire métier


# Stratégie automatique de calcul du num_ctx

Principe

On part de :
- la taille réelle du contenu
- une marge pour la réponse
- un plafond dépendant du modèle

Règle simple mais efficace :
tokens_contenu + tokens_réponse + marge

## Infrastructure – Estimation des tokens

Approximation suffisante (et réaliste) :
```C#
public static class TokenEstimator
{
    // 1 token ≈ 4 caractères (anglais / français mix)
    public static int EstimateTokens(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        return text.Length / 4;
    }
}
```

## Infrastructure – Stratégie ContextWindowCalculator

```C#
public class ContextWindowCalculator
{
    private readonly int _modelMaxContext;

    public ContextWindowCalculator(int modelMaxContext)
    {
        _modelMaxContext = modelMaxContext;
    }

    public int Calculate(
        string content,
        LlmRequestOptions options)
    {
        // Si le domaine impose une valeur → on respecte
        if (options.MaxContextTokens.HasValue)
        {
            return Math.Min(options.MaxContextTokens.Value, _modelMaxContext);
        }

        var contentTokens = TokenEstimator.EstimateTokens(content);

        var complexityMultiplier = options.Complexity switch
        {
            LlmTaskComplexity.Simple => 1.1,
            LlmTaskComplexity.Standard => 1.25,
            LlmTaskComplexity.Deep => 1.4,
            _ => 1.25
        };

        var estimatedContext =
            (int)(contentTokens * complexityMultiplier)
            + options.ResponseTokens;

        // Clamp final
        return Math.Min(estimatedContext, _modelMaxContext);
    }
}
```

Résultat :
- petit article → petit num_ctx
- gros doc → ça monte automatiquement
- jamais au-delà de ce que le modèle supporte

# Ollama – Mapping final

```C#
var numCtx = contextCalculator.Calculate(content, options);

var requestBody = new
{
    model = modelName,
    prompt = prompt,
    stream = false,
    format = "json",
    options = new
    {
        num_ctx = numCtx
    }
};

```
# Recommandation concrète pour le projet

## Valeurs par défaut suggérées ( à suggérer dans la page "Guide")
Cas	Modèle	Max
Quiz standard	mistral	~6k
Gros article	mistral	8k (max)
Documents longs	mistral-nemo / qwen2.5	16k
Docs XXL	modèles 32k+	chunking

