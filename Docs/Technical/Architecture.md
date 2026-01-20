# Derot My Brain – Architecture

┌─────────────────────┐
│     Utilisateur     │
│  (Navigateur Web)   │
└─────────┬───────────┘
          │ HTTP
          ▼
┌──────────────────────────┐
│        Frontend          │
│   React + TypeScript     │
│                          │
│ - Identification         │
│ - Lecture Wikipédia      │
│ - Quiz UI                │
│ - Historique / Backlog   │
└─────────┬────────────────┘
          │ REST API
          ▼
┌──────────────────────────┐
│        Backend API       │
│    ASP.NET Core Web API  │
│                          │
│ - Gestion utilisateurs   │
│ - Historique & backlog   │
│ - Récupération Wikipédia │
│ - Génération quiz        │
│ - Évaluation réponses    │
└──────┬─────────┬─────────┘
       │         │
       │         │ HTTP
       │         ▼
       │   ┌───────────────────┐
       │   │    Ollama API     │
       │   │  (localhost:11434)│
       │   │                   │
       │   │ - mistral:7b      │
       │   │ - génération Q/R  │
       │   │ - scoring         │
       │   └───────────────────┘
       │
       ▼
┌──────────────────────────┐
│     SQLite Database      │
│     derot-my-brain.db    │
│                          │
│ - Users Table            │
│ - Activities Table       │
│ - Preferences Table      │
└──────────────────────────┘

## Agent Interaction Guidelines

To ensure consistency and prevent data pollution during development and testing, all AI Agents (like Antigravity) should follow these rules:

1.  **Test User**: Always use the designated test account for mock data creation and automated testing.
    - **Name**: `TestUser`
    - **ID**: `test-user-id-001`
2.  **Data Storage**: Mock history records should be written to `history_test-user-id-001.json`.
3.  **Logical Consistency**: Ensure that any mock data created is logically consistent with the `TestUser` profile.

