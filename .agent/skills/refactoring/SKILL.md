---
name: refactoring-skill
description: Improving code structure and readability while maintaining behavioral parity and project standards.
---

## Instructions
1.  **Safety Net**:
    - Ensure a solid test suite exists. Check `d:\Repos\Derot-my-brain\src\backend\DerotMyBrain.Tests` for backend coverage.
    - Run existing tests: `dotnet test` or `npm test`.
2.  **Standards Alignment**:
    - **Dumb Components**: Move side effects and business logic to Custom Hooks in `src/frontend/src/hooks`.
    - **Thin Controllers**: Move business logic to Services in `DerotMyBrain.Core`.
    - **Dependency Inversion**: Ensure interfaces are defined in Core and implementation in Infrastructure.
3.  **Design System Compliance**:
    - Replace hardcoded hex colors or Tailwind absolute colors (e.g., `text-blue-500`) with semantic tokens (e.g., `text-primary`).
    - Verify dark mode using `dark:` variants or CSS variables defined in `index.css`.
4.  **Clean Code**:
    - Apply SOLID principles.
    - Improve naming based on `d:\Repos\Derot-my-brain\Docs\Reference\Glossary.md`.
    - Update `d:\Repos\Derot-my-brain\Docs\Reference\DataModel.md` if new entities or relationships were introduced.
5.  **Verification**:
    - Run tests after every change to ensure zero regression.

## Checklist
- [ ] Logic moved from Component to Hook?
- [ ] Logic moved from Controller to Service?
- [ ] Semantic colors used throughout?
- [ ] Tests still passing?
- [ ] Clean Architecture respected?
