---
name: feature-implementation
description: Systematic approach to implementing new features with full technical compliance and test coverage.
---

## Instructions
1.  **Preparation**:
    - Review `Docs/Technical/` for relevant guidelines.
    - Check `Docs/Reference/Glossary.md` for consistent naming.
2.  **TDD First**:
    - Start by writing failing tests in `DerotMyBrain.Tests` (Backend) or `src/frontend/src/**/*.test.tsx` (Frontend).
    - Ensure tests cover nominal, edge, and error cases.
3.  **Backend Implementation**:
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
