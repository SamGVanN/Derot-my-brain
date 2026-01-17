# Antigravity Instructions - Derot My Brain

## Project Overview
"Derot My Brain" is a local web application designed to stimulate curiosity and active learning using Wikipedia content. It alternates between free reading and dynamically generated quizzes.

**Key Goals:**
- **Local hosting**: Run entirely on the user's machine (Windows/Homelab).
- **No SQL**: Data persistence handled via local JSON files.
- **Local AI**: Use Ollama (llama3, mistral, etc.) for generating questions and evaluating answers.
- **Active Learning**: Read -> Quiz -> History/Backlog loop.

## Technology Stack
- **Frontend**: React + TypeScript (using Vite).
  - Use well-known, tested, and approved libraries.
  - *Styling*: **shadcn/ui** + Tailwind CSS (Selected).
  - **Constraint**: **FOSS (Free and Open Source) or Free resources ONLY**.
- **Backend**: ASP.NET Core Web API.
  - **MUST respect SOLID principles**.
- **AI**: Ollama exposing a local HTTP API.
- **Data**: JSON text files.

## Data Structures (JSON)

### Users (`users.json`)
```json
{
  "users": [
    {
      "name": "Alex",
      "createdAt": "2026-01-10T14:22:00Z"
    }
  ]
}
```

### History (`history_{username}.json`)
```json
{
  "user": "Alex",
  "history": [
    {
      "topic": "Révolution française",
      "wikiPageId": "Révolution_française",
      "firstSeenAt": "2026-01-10T15:10:00Z",
      "lastScore": 4,
      "lastAttemptAt": "2026-01-10T15:25:00Z"
    }
  ]
}
```

### Backlog (`backlog_{username}.json`)
```json
{
  "user": "Alex",
  "topics": [
    {
      "topic": "Physique quantique",
      "wikiPageId": "Physique_quantique",
      "addedAt": "2026-01-10T16:00:00Z"
    }
  ]
}
```

## API Endpoints

### Auth / User
- `POST /api/users` - Create or select a user.
- `GET /api/users` - Retrieve list of users.

### Wikipedia
- `GET /api/wiki/random?categories=history,science` - Fetch random page metadata.
- `GET /api/wiki/page/{wikiPageId}` - Fetch specific page content.

### Quiz
- `POST /api/quiz/generate` - Generate questions from content.
  - Payload: `{ "wikiPageId": "...", "articleText": "..." }`
- `POST /api/quiz/evaluate` - Evaluate a single answer.
  - Payload: `{ "expectedAnswer": "...", "userAnswer": "..." }`

### History
- `GET /api/history/{username}`
- `POST /api/history/{username}` - Add entry after quiz.

### Backlog
- `GET /api/backlog/{username}`
- `POST /api/backlog/{username}` - Add topic to backlog.
- `DELETE /api/backlog/{username}/{wikiPageId}` - Remove from backlog.

## AI Prompts

### 1. Question Generation
**Goal**: Generate 5 factual questions from the text.
**Format**: Strict JSON.

```text
Tu es un assistant pédagogique.

À partir de l’article Wikipédia suivant, génère EXACTEMENT 5 questions de quiz
pour tester la compréhension du sujet.

Contraintes :
- Les questions doivent porter uniquement sur les informations présentes dans le texte
- Les réponses doivent être factuelles et précises
- Les questions doivent être variées (dates, concepts, faits, définitions)
- Les réponses doivent être courtes (1 à 3 phrases max)
- Ne fais aucune supposition externe au texte

Format de sortie STRICTEMENT en JSON, sans texte avant ou après :

{
  "topic": "<titre de l'article>",
  "questions": [
    {
      "question": "...",
      "answer": "..."
    }
  ]
}

Article Wikipédia :
<<<
{ARTICLE_TEXT}
>>>
```

### 2. Answer Evaluation
**Goal**: Compare user answer to expected answer with semantic tolerance.
**Format**: Strict JSON.

```text
Tu es un correcteur de quiz.

Compare la réponse utilisateur à la réponse attendue.

Donne un score de similarité sémantique entre 0 et 1 :
- 1 = réponse totalement correcte
- 0 = réponse incorrecte

Sois tolérant aux reformulations et synonymes.
Ignore les fautes mineures.

Retourne STRICTEMENT ce JSON :

{
  "score": 0.0,
  "explanation": "Brève explication"
}

Réponse attendue :
<<<
{EXPECTED_ANSWER}
>>>

Réponse utilisateur :
<<<
{USER_ANSWER}
>>>
```
