---
name: task-planning-skill
description: Breaking down complex user requests into actionable, well-structured plans.
---

## Instructions
When planning a task:
1.  **Consult the Source of Truth**: Read `d:\Repos\Derot-my-brain\Docs\ANTIGRAVITY_INSTRUCTIONS.md` and `d:\Repos\Derot-my-brain\Docs\Planning\Project-Status.md`.
2.  **Analyze Technical Compliance**: Check relevant docs in `d:\Repos\Derot-my-brain\Docs\Technical\` to ensure the plan respects Clean Architecture.
3.  **Break Down by Layer**:
    - **Backend**: Core (Entities/Interfaces/Services), Infrastructure (Persistence/External), API (Controllers/DTOs).
    - **Frontend**: API Client, Stores, Hooks, Components, Pages.
    - **Testing**: Backend Unit/Integration, Frontend Vitest/RTL.
4.  **Adhere to Standards**:
    - Thin controllers, thick services (SOLID).
    - Dumb components (Presentation only), logic in Custom Hooks.
    - **Rich Aesthetics**: Ensure the plan includes polishing the UI for premium look and feel.
    - **Local-First**: Ensure AI and storage remain local (Ollama / SQLite).
5.  **Mock Data**: Always include a step to seed mock data for `TestUser` (`test-user-id-001`) in `d:\Repos\Derot-my-brain\src\backend\DerotMyBrain.Infrastructure\Data\DbInitializer.cs`.
6.  **Structure**: Present the plan with clear checkboxes and logical dependencies.

## Example Plan Structure
1. **Analysis**: Impact on existing entities and services.
2. **Backend**:
   - [ ] Core: Entity/Interface changes.
   - [ ] Infrastructure: Repository/External Service implementation.
   - [ ] API: DTOs, Controllers, DI registration.
3. **Frontend**:
   - [ ] Infrastructure: API Client/DTOs.
   - [ ] Application: Zustand store updates, Custom Hooks.
   - [ ] Presentation: UI components, Pages.
4. **Testing**:
   - [ ] Backend: Unit tests (Service) and Integration tests (Controller).
   - [ ] Frontend: Component/Hook tests.
5. **Validation**: Mock data seeding and manual verification.
