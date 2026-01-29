# Antigravity Instructions - Derot My Brain

## Project Overview
"Derot My Brain" is a local web application designed to stimulate curiosity and active learning using Wikipedia content or any other text source. It alternates between free reading and dynamically generated quizzes.

**Key Goals:**
- **Local hosting**: Run entirely on the user's machine (Windows/Linux/Homelab).
- **SQLite**: Local embedded database for data persistence (No external SQL Server).
- **Local AI**: Use Ollama (llama3, mistral, etc.) for generating questions and evaluating answers.
- **Active Learning**: Explore -> Read -> Quiz -> History/"Tracking Sources" loop.
- **Flexibility**: Can launch Read/Quiz directly from an **Activity** or any **Source** (Document, OnlineResource, Backlog).

## Current Project Status (Jan 2026)
- **Core Architecture**: Done (Clean Arch Backend, Hexagonal-ish Frontend).
- **i18n**: FR/EN fully implemented.
- **Wikipedia**: Exploration works; Reading/Quiz requires stabilization.
- **LLM**: Backend exists but liaison with Ollama is being de-mocked.

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
- **Critical Strategy**: De-mock features! Replace `sampleArticles` with real API calls and valid LLM prompts.

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
    SelectedCategories TEXT, -- JSON Array of category IDs
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- Table Activities (UserActivity)
CREATE TABLE Activities (
    Id TEXT PRIMARY KEY,
    UserId TEXT NOT NULL,
    SourceId TEXT NOT NULL,
    SourceType INTEGER NOT NULL,
    SourceHash TEXT NOT NULL,
    Title TEXT NOT NULL,
    Description TEXT NOT NULL,
    Type INTEGER NOT NULL,
    SessionDateStart TEXT NOT NULL,
    SessionDateEnd TEXT,
    ReadDurationSeconds INTEGER,
    QuizDurationSeconds INTEGER,
    Score INTEGER NOT NULL,
    QuestionCount INTEGER NOT NULL,
    ScorePercentage REAL,
    IsNewBestScore INTEGER DEFAULT 0,
    IsCompleted INTEGER DEFAULT 0,
    ArticleContent TEXT,
    LlmModelName TEXT,
    LlmVersion TEXT,
    Payload TEXT,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- Table UserFocuses
CREATE TABLE UserFocuses (
    Id TEXT PRIMARY KEY,
    UserId TEXT NOT NULL,
    SourceHash TEXT NOT NULL,
    SourceId TEXT NOT NULL,
    SourceType INTEGER NOT NULL,
    DisplayTitle TEXT NOT NULL,
    IsPinned INTEGER DEFAULT 0,
    IsArchived INTEGER DEFAULT 0,
    BestScore REAL DEFAULT 0,
    LastScore REAL DEFAULT 0,
    LastAttemptDate TEXT NOT NULL,
    TotalReadTimeSeconds INTEGER DEFAULT 0,
    TotalQuizTimeSeconds INTEGER DEFAULT 0,
    TotalStudyTimeSeconds INTEGER DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    UNIQUE(UserId, SourceHash)
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

### User Focus
- `GET /api/users/{userId}/user-focus` - Get all user focus topics.
- `POST /api/users/{userId}/user-focus` - Track/Untrack a topic.
- `DELETE /api/users/{userId}/user-focus/{sourceHash}` - Untrack a topic.
- `POST /api/users/{userId}/user-focus/{sourceHash}/rebuild` - Rebuild stats from history.

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
