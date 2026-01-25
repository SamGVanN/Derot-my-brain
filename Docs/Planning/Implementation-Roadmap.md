# Implementation Roadmap (Updated 2026-01-25)

This document reflects the current state of **Derot My Brain** and outlines the remaining tasks to achieve a fully functional, non-mocked MVP.

## 🟢 Completed Phases
- **Phase -1: Architecture Migration**: Centralized API client, Zustand stores, custom hooks, and component refactoring.
- **Phase 0: Foundation**: SQLite setup, seed data (categories/themes), and global configuration.
- **Phase 1: Core UX**: Session persistence, Welcome page, and local logging.
- **Phase 2: Preferences & i18n**: User preferences (question count, theme, language) and full French/English support.

## 🟡 In Progress: Phase 6 (Wikipedia Integration)
The goal is to move from a mocked POC to a real, integrated learning flow.

### Task 6.1: Stable Explore-to-Read Transition (CURRENT)
- **Objective**: Ensure exploration time is tracked and "Read" transition produces real content.
- **Remaining**:
    - [ ] Verify `WikipediaService.ReadAsync` correctly fetches content from `WikipediaContentSource`.
    - [ ] Ensure `ReadView` displays the `ArticleContent` retrieved from the backend.
    - [ ] Stabilize E2E tests (`DerotZone.e2e.spec.ts`).

### Task 6.2: Real Quiz Generation (Next)
- **Objective**: Connect the "Quiz" mode to the `OllamaLlmService`.
- **Tasks**:
    - [ ] Update `QuizView` to fetch questions from `GET /api/activities/{id}/quiz`.
    - [ ] Ensure `ActivityService.GenerateQuizAsync` uses the article content stored during the Read phase.
    - [ ] Implement quiz submission and score calculation.

## 🔴 Upcoming Phases

### Phase 3: My Focus Area & Knowledge Depth
- **Objective**: Implement the aggregation logic for subjects.
- **Tasks**:
    - [ ] Finalize `UserFocusService` aggregation logic.
    - [ ] Implement the Focus Area dashboard with evolution charts (My Focus Area Page).

### Phase 4: Backlog & Navigation
- **Objective**: Manage contents to be processed later.
- **Tasks**:
    - [ ] Implement the Backlog page.
    - [ ] Enable starting an activity from the Backlog.

### Phase 5: Library & Document Upload
- **Objective**: Support local PDF/Markdown/TXT files.
- **Tasks**:
    - [ ] Implement File Upload in `FileContentSource`.
    - [ ] Support text extraction from various formats.

---

## 🛠️ Technical Debt & Stabilization
- [ ] Fix port conflict in E2E tests (stabilize `scripts/kill-derot-api.ps1`).
- [ ] Implement `admin` config updates for LLM URL in the UI.
- [ ] Refine `PageHeader` usage across all dynamically created pages.
