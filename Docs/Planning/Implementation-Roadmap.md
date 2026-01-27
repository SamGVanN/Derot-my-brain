# Implementation Roadmap (Updated 2026-01-27)

This document reflects the current technical debt and the path forward to move from a mocked POC to a functional MVP.

## 🟢 Completed (Technical Foundation)
- **Phase -1: Architecture Migration**: Centralized API client, Zustand stores, and component refactoring.
- **Phase 0: Foundation**: SQLite setup, seed data, and global configuration.
- **Phase 1: Core UX**: Session identity, Welcome page, and Navigation shell.
- **Phase 2: Preferences & i18n**: Multi-language support and basic user settings.

## 🟡 In Progress: Phase 6 (Wikipedia & Real Content)
**Goal**: Transition from mocked "sample articles" to real API data.

### Task 6.1: Real Wikipedia Integration (CRITICAL)
- **Objective**: Ensure the backend fetches real content and the frontend displays it.
- **Status**: Backend infrastructure exists but is not functional in the end-to-end flow.
- **Tasks**:
    - [ ] Debug and Validate `WikipediaContentSource.cs` with the real API.
    - [ ] Remove hardcoded `sampleArticles` in `DerotPage.tsx`.
    - [ ] Update `ExploreView` to fetch trending/random topics via the backend.
    - [ ] Ensure `ReadView` displays the exact content retrieved during the Explore transition.

### Task 6.2: Derot Zone Bug Bash
- **Objective**: Fix navigation and state management issues in the Derot Zone.
- **Tasks**:
    - [ ] Fix broken transitions between Explore, Read, and Quiz modes.
    - [ ] Ensure `useWikipediaExplore` correctly manages the backend `UserActivity` lifecycle.

## 🔴 Upcoming: Phase 7 (LLM / Quiz Validation)
**Goal**: Connect the Quiz UI and validate the Ollama liaison.

### Task 7.1: Ollama Liaison Validation
- **Objective**: Ensure the backend can talk to a local Ollama instance reliably.
- **Tasks**:
    - [ ] Configure `OllamaLlmService` with dynamic settings (Url, Model).
    - [ ] Test and validate the JSON response format from Ollama.

### Task 7.2: Quiz UI Connection
- **Objective**: Connect `QuizView.tsx` to the backend.
- **Tasks**:
    - [ ] Fetch real questions from `GET /api/activities/{id}/quiz`.
    - [ ] Implement answer submission and score tracking.

---

## 🛠️ Technical Debt & Stabilization
- [ ] **E2E Stability**: Fix the process locking issue in `scripts/kill-derot-api.ps1`.
- [ ] **Data Model Alignment**: Ensure `UserActivity` payload correctly stores diverse content types (Wikipedia vs Documents).
