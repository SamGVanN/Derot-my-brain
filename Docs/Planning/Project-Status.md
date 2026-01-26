# Project Status - Derot My Brain

**Last Updated**: 2026-01-25
**Current Phase**: Phase 6 & Phase 1 Refinements
**Overall Progress**: ~65% to V1 MVP

## Current Focus
Finalizing the **Derot Zone** Wikipedia integration. We are transitioning from UI/UX mocks to a full backend-integrated flow including:
- Real-time exploration tracking.
- Transactional linkage between Exploration and Reading.
- Real Wikipedia content fetching and storage in `UserActivity`.

## High-Level Status

| Feature | Status | Notes |
| :--- | :--- | :--- |
| Core Architecture | ✅ Done | Hexagonal-ish frontend, Service-based backend. |
| Authentication | ✅ Done | Local profile selection with persistence. |
| i18n (FR/EN) | ✅ Done | Fully implemented across all pages. |
| Derot Zone (UI) | ✅ Done | Explore, Read, and Quiz views implemented. |
| Wikipedia Service | 🟡 TODO | Integration logic for content fetching. |
| Derot Zone (Full user story) | 🟡 TODO | Explore, Read, and Quiz views implemented. |
| LLM Integration | 🟡 TODO | Ollama integration for quiz generation. |
| Focus Area | 🔴 Backlog | Visual dashboard for subject mastery. |

## Immediate Roadmap
1. **Stabilize E2E**: Resolve port 5005/5077 conflicts and unreliable cleanup.
2. **Real Content Flow**: Ensure `ReadView` displays actual Wikipedia text.
3. **Active Quiz**: Switch `QuizView` from demo questions to LLM-generated questions.

## Known Blockers
- E2E testing environment instability on Windows (process locking).
