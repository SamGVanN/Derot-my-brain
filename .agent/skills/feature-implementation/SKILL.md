---
name: feature-implementation
description: Systematic approach to implementing new features with full technical compliance and test coverage.
---

## Instructions
1.  **Preparation**:
- [ ] Review `Docs/ANTIGRAVITY_INSTRUCTIONS.md` for project-specific rules.
    - [ ] Check `Docs/Technical/` for relevant guidelines.
    - [ ] Check `Docs/Reference/Glossary.md` for consistent naming.
    - [ ] Check `Docs/Reference/DataModel.md` for the database schema.
2.  **TDD First**:
    - Start by writing failing tests in `DerotMyBrain.Tests` (Backend: xUnit/Moq) or `src/frontend/src/**/*.test.tsx` (Frontend: Vitest/RTL).
    - Ensure tests cover nominal, edge, and error cases.
3.  **Active Learning Workflow**:
    - Implement the "Explore -> Read -> Quiz" cycle.
    - **Flexibility**: Allow launching "Read" or "Quiz" directly from an existing **Activity** or any **Source** (Document, OnlineResource, Backlog).
4.  **AI Integration (Ollama)**:
    - Use the local Ollama API for text analysis and quiz generation.
    - **De-mocking**: Do not use `sampleArticles` or mocked results if the goal is functional E2E.
5.  **Backend Implementation**:
    - Follow Clean Architecture: Core -> Infrastructure -> API.
    - Use `PascalCase` for methods, `_camelCase` for fields.
    - Ensure thin controllers and thick services.
4.  **Frontend Implementation**:
    - Follow layered architecture: API -> (Store) -> Hook -> Component.
    - Use semantic Tailwind classes (e.g., `bg-background`).
    - Move all logic to Custom Hooks.
5.  **Data Persistence**:
    - Use SQLite + EF Core. No external DBs.
    - Seed realistic data for `TestUser`.
6.  **Review**:
    - Run all tests: `dotnet test` and `npm test`.
    - Check for lint errors or hardcoded colors.

## Critical Constraints
- **NO** direct API calls in Components.
- **NO** business logic in Controllers.
- **TDD** is mandatory.
- **Mock data** for `TestUser` is mandatory.
