# SKILLS — What an AI Agent Can Do in this Repo

This file lists concrete, repository-specific capabilities an AI coding agent should have and direct examples to perform them.

- **Build & run backend**: use the ASP.NET Core project in `src/backend`.

  Example:
  ```bash
  cd src/backend
  dotnet restore
  dotnet watch run --project DerotMyBrain.API
  ```

- **Run backend tests**: run all unit/integration tests from `src/backend`:

  ```bash
  cd src/backend
  dotnet test
  ```

- **Build & run frontend**: use Vite in `src/frontend`.

  Example:
  ```bash
  cd src/frontend
  npm install
  npm run dev
  ```

- **Run frontend tests**: Vitest commands
  ```bash
  cd src/frontend
  npm run test
  npm run test:ui  # interactive
  ```

- **Add or modify a backend endpoint** (typical flow):
  1. Update the interface in `src/backend/DerotMyBrain.Core/Interfaces`.
  2. Implement in `src/backend/DerotMyBrain.Infrastructure`.
  3. Register implementation in DI (`src/backend/DerotMyBrain.API/Program.cs`).
  4. Add a controller under `src/backend/DerotMyBrain.API/Controllers`.
  5. Add/adjust tests in `src/backend/DerotMyBrain.Tests`.

- **Database & seeding**:
  - Project uses SQLite via EF Core. Default DB filename referenced in docs: `derot-my-brain.db`.
  - When creating mock data for tests or local runs, always use the designated TestUser: Name `TestUser`, ID `test-user-id-001`.
  - Prefer EF migrations for schema changes; do not modify production DB flows without approval.

- **LLM / AI tasks**:
  - The infra contains an LLM client expected to target a local runner (Ollama) — check `DerotMyBrain.Infrastructure` for the client.
  - When generating prompts or seeding LLM-related tests, reference `Docs/Reference/LLM-Prompts.md`.

- **Debugging tips**:
  - Backend compile/runtime errors: `dotnet build` then check `src/backend/DerotMyBrain.API/Logs` for runtime logs.
  - Frontend issues: open browser devtools and Vite terminal output.

- **Documentation & context sources** (read these first):
  - `Docs/Planning/functional_specifications_derot_my_brain.md` — product goals and acceptance criteria.
  - `Docs/Reference/Glossary.md` — project-specific terms used across specs.
  - `Docs/Developer/Getting-Started.md` — exact build/run commands.
  - `Docs/Technical/Architecture.md` — Clean Architecture mapping and the TestUser rule.

- **PR & test expectations**:
  - Small, focused PRs only. Include unit tests for backend changes and update frontend tests when UI behavior changes.
  - Update `Docs/Planning/Project-Status.md` when marking tasks done.

- **Files you will edit most often**:
  - `src/backend/DerotMyBrain.API/Program.cs` (DI + endpoints wiring)
  - `src/backend/DerotMyBrain.API/Controllers/*`
  - `src/backend/DerotMyBrain.Core/*` (interfaces, DTOs, entities)
  - `src/backend/DerotMyBrain.Infrastructure/*` (implementations, store, LLM client)
  - `src/frontend/src/*` (routes, pages, components)

If you want, I can produce quick task templates (branch name, test checklist, PR template) next.

## Branch naming (agent prepares, you commit)

Agents should prepare a suggested branch name and short description, but NOT commit or push branches — the human developer will create and push the branch.

Preferred branch-name patterns (choose one):

- `feature/<ticket?>-short-description` — new features or user-facing work
- `fix/<ticket?>-short-description` — bug fixes
- `chore/short-description` — maintenance, dependency updates
- `docs/short-description` — documentation updates
- `hotfix/short-description` — urgent production fixes
- `experiment/short-description` — spikes or prototypes

Guidelines:
- Keep names lower-case, hyphen-separated, max ~50 chars after the optional ticket.
- If you have a ticket or issue id use it: `feature/IMPL-123-add-quiz-gen`.
- Include a 1-line PR description prepared by the agent; the developer may copy it into the PR body.

Example prepared output an agent should provide (but not push):

- Branch name: `feature/IMPL-123-add-quiz-generation`
- PR title: `Add quiz generation service and API endpoint`
- PR description (1–2 lines): `Implements LLM-driven quiz generation: adds IQuizService, infrastructure implementation, controller endpoint, and unit tests.`
