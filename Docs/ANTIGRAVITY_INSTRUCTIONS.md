# Antigravity Instructions - Derot My Brain

## Project Overview
"Derot My Brain" is a local web application designed to stimulate curiosity and active learning using Wikipedia content or any other text source. It alternates between free reading and dynamically generated quizzes.

**Key Goals:**
- **Local hosting**: Run entirely on the user's machine (Windows/Linux/Homelab).
- **SQLite**: Local embedded database for data persistence (No external SQL Server).
- **Local AI**: Use Ollama (llama3, mistral, etc.) for generating questions and evaluating answers.
- **Active Learning**: Read -> Quiz -> History/TrackedTopics loop.

## Technology Stack
- **Frontend**: React + TypeScript (using Vite).
  - Use well-known, tested, and approved libraries.
  - *Styling*: **shadcn/ui** + Tailwind CSS (Selected).
  - **State Management**: **Zustand** (recommended over Redux).
  - **Constraint**: **FOSS (Free and Open Source) or Free resources ONLY**.
  - **MUST respect Frontend Architecture Principles** (see `Docs/Technical/Frontend-Guidelines.md`):
    - Separation of Concerns
    - Component-driven architecture (one component = one responsibility)
    - Composition over inheritance
    - Unidirectional data flow (props down, events up)
    - Custom Hooks for business logic (useAuth, useUser, useQuiz, etc.)
    - Clean Architecture / Hexagonal (adapted for frontend)
    - Keep UI components "dumb" (presentation only)
    - Dependency injection via props/context
- **Backend**: .NET Core
  - **MUST respect Clean Architecture Regulations** (see `Docs/Technical/Backend-Guidelines.md`).
  - **Data**: SQLite (.db).
- **AI**: Ollama exposing a local HTTP API.

## Data Structures (SQLite Schema)

### Database Schema
```sql
-- Table Users
CREATE TABLE Users (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    LastConnectionAt TEXT NOT NULL
);

-- Table UserPreferences
CREATE TABLE UserPreferences (
    UserId TEXT PRIMARY KEY,
    QuestionCount INTEGER DEFAULT 10,
    PreferredTheme TEXT DEFAULT 'derot-brain',
    Language TEXT DEFAULT 'auto',
    FavoriteCategories TEXT, -- JSON Array of categories
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- Table Activities (UserActivity)
CREATE TABLE Activities (
    Id TEXT PRIMARY KEY,
    UserId TEXT NOT NULL,
    Type TEXT NOT NULL, -- 'Read', 'Quiz'
    Title TEXT NOT NULL, -- Topic/Article Title
    Description TEXT,
    SourceUrl TEXT,
    ContentSourceType TEXT, -- 'Wikipedia', 'File', 'Url'
    ArticleContent TEXT,
    LlmModelName TEXT,
    LlmVersion TEXT,
    
    -- Quiz Stats
    Score INTEGER NOT NULL DEFAULT 0,
    MaxScore INTEGER NOT NULL DEFAULT 0,
    
    LastAttemptDate TEXT NOT NULL,
    IsCompleted INTEGER DEFAULT 0,
    IsTracked INTEGER DEFAULT 0,
    Payload TEXT, -- JSON blob for questions/answers
    
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- Table TrackedTopics
CREATE TABLE TrackedTopics (
    Id TEXT PRIMARY KEY,
    UserId TEXT NOT NULL,
    Topic TEXT NOT NULL,
    BestScore INTEGER DEFAULT 0,
    BestScoreDate TEXT,
    TotalQuizAttempts INTEGER DEFAULT 0,
    TotalReadSessions INTEGER DEFAULT 0,
    LastInteraction TEXT NOT NULL,
    
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    UNIQUE(UserId, Topic)
);
```

## API Endpoints

### Auth / User
- `POST /api/users` - Create or login user.
- `GET /api/users/{id}` - Get user details.

### Activity Management
- `GET /api/users/{userId}/activities` - List activities (supports ?topic= filter).
- `GET /api/users/{userId}/activities/{activityId}` - Get specific activity.
- `POST /api/users/{userId}/activities` - Create activity manually.
- `PUT /api/users/{userId}/activities/{activityId}` - Update activity.
- `DELETE /api/users/{userId}/activities/{activityId}` - Delete activity.

### Active Learning Flow
- `POST /api/users/{userId}/activities/start` - Start a new reading session (fetches content).
  - Payload: `{ "url": "...", "sourceType": "Wikipedia" }`
- `POST /api/users/{userId}/activities/{activityId}/quiz` - Generate quiz for an activity.
- `POST /api/quiz/evaluate` - Evaluate answer (if not handled by frontend/local logic).

### Statistics & Tracking
- `GET /api/users/{userId}/statistics` - Get dashboard stats.
- `GET /api/users/{userId}/statistics/activity-calendar` - Get heatmap data.
- `GET /api/users/{userId}/statistics/top-scores` - Get leaderboard/top scores.

### Tracked Topics
- `GET /api/users/{userId}/compendium` - Get all tracked topics.
- `POST /api/users/{userId}/compendium/import` - Import tracked topics from history.

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
