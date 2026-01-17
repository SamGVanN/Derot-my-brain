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
│   Stockage fichiers      │
│        JSON              │
│                          │
│ - users.json             │
│ - history_<user>.json    │
│ - backlog_<user>.json    │
└──────────────────────────┘
