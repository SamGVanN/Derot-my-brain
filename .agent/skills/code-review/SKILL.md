---
name: code-review-skill
description: Rigorous checklist for reviewing code changes against architecture, testing, and aesthetic standards.
---

## Instructions
Review code against these categories:

### 1. Architecture & Patterns
- **Backend Cleanliness**: Is `DerotMyBrain.Core` free of external dependencies (No EF Core, No JSON)?
- **Thin Controllers**: Do controllers only orchestrate services?
- **Dumb Components**: Are components in `src/frontend/src/components` free of `useEffect` data fetching? (Logic should be in hooks).
- **Zustand**: Is state accessed via custom hooks, not directly from stores?

### 2. Testing & Quality
- **TDD Verification**: Check if tests were added to `DerotMyBrain.Tests` or `src/frontend/src/**/*.test.tsx`.
- **AAA Pattern**: Are tests structured with Arrange, Act, Assert?
- **Mock Data**: Is there updated seed data for `TestUser` (`test-user-id-001`)?

### 3. Aesthetics & Theming
- **Semantic Colors**: Search for hardcoded colors like `#ffffff`, `bg-white`, `text-black`. These should be `bg-background`, `text-foreground`.
- **Premium UI**: Does the change use shadcn components and maintain a high-end look?
- **Dark Mode**: Is the UI legible and beautiful in both modes?

### 4. Logging & Errors
- **Structured Logging**: Is `Serilog` used with message templates (e.g., `_logger.LogInfo("User {Id}", id)`)?
- **Error Handling**: Are custom exceptions used from `Core`?

## Feedback Format
Provide feedback in a structured way:
- **Major**: Architectural violations or missing tests.
- **Minor**: Naming inconsistencies or 스타일 enhancements.
- **Positive**: Good use of patterns or clean code.
