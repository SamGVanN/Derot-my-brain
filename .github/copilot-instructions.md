# Copilot / Agent Instructions — Derot My Brain

Purpose: Quickly onboard AI coding agents to be productive in this repository. Focus on concrete, discoverable patterns, commands, and references.

1) Big picture (how code is organized)
- Clean Architecture: `DerotMyBrain.Core` (domain) → `DerotMyBrain.Infrastructure` (implementations) → `DerotMyBrain.API` (ASP.NET Core entry) → `src/frontend` (React + Vite).
- Key folders: `src/backend/DerotMyBrain.Core`, `src/backend/DerotMyBrain.Infrastructure`, `src/backend/DerotMyBrain.API/Controllers`, `src/frontend`.

2) How to build & run (exact commands)
- Backend (development):
  - cd to repo root then: `cd src/backend`
  - `dotnet restore`
  - `dotnet watch run --project DerotMyBrain.API` (Hot Reload + opens Swagger)
  - Swagger UI: `http://localhost:<port>/swagger/index.html` for testing APIs
- Frontend (development):
  - In a new terminal: `cd src/frontend`
  - `npm install` (only once or after package.json changes)
  - `npm run dev` (Vite dev server, default: `http://localhost:5173`)
- Tests:
  - Backend unit/integration: run `dotnet test` from `src/backend`
  - Frontend: `npm run test` or `npm run test:ui` from `src/frontend`

3) Critical project-specific conventions
- Architecture: Domain (`Core`) defines interfaces — implementations live in `Infrastructure`; the API project wires DI and exposes DTOs/controllers.
- Database: SQLite via EF Core. DB file name used in docs: `derot-my-brain.db` (seed/mock data live here for local testing).
- Agent test-data rule: use the designated TestUser when seeding mock data: Name `TestUser`, ID `test-user-id-001` (see Docs/Technical/Architecture.md).
- LLM integration: the project expects a local LLM (Ollama or equivalent). Look in `DerotMyBrain.Infrastructure` for the LLM client integration.

4) Where to look for specific patterns / examples
- API endpoints and DI setup: [src/backend/DerotMyBrain.API/Program.cs](src/backend/DerotMyBrain.API/Program.cs)
- Controllers: [src/backend/DerotMyBrain.API/Controllers](src/backend/DerotMyBrain.API/Controllers)
- Domain services & entities: [src/backend/DerotMyBrain.Core/Entities](src/backend/DerotMyBrain.Core/Entities)
- Infrastructure implementations: [src/backend/DerotMyBrain.Infrastructure](src/backend/DerotMyBrain.Infrastructure)
- Frontend entry & routes: [src/frontend/src](src/frontend/src)

5) Common tasks and explicit examples
- Add a backend endpoint: update `DerotMyBrain.Core` interfaces → implement in `Infrastructure` → add controller in `DerotMyBrain.API` and register the implementation in DI (`Program.cs`).
- Seed test data locally: modify seeder in `Infrastructure` or add migration; ensure seeded records use `TestUser` id to avoid polluting other data.
- Troubleshooting startup issues: check `src/backend/DerotMyBrain.API/Logs` and run `dotnet build` to view compile errors; frontend console and Vite logs show runtime errors.

6) Important docs to consult (order matters)
- `Docs/README.md` — doc organization and agent workflow
- `Docs/Developer/Getting-Started.md` — exact build/run/test commands
- `Docs/Technical/Architecture.md` — architecture and agent rules (TestUser, data rules)
- `Docs/Technical/*` — Backend/Frontend coding standards and testing strategy
- `Docs/Planning/functional_specifications_derot_my_brain.md` — product goals and functional requirements
- `Docs/Reference/Glossary.md` — project-specific terms and definitions used in specs

7) What not to change without approval
- Database schema or production data flows; prefer migrations and tests.
- Public API contracts (DTO shapes) without updating corresponding frontend code and tests.

8) Pull-request & testing expectations for agents
- Create small, focused changes with unit tests (backend `dotnet test`, frontend `npm run test`).
- Update `Planning/Project-Status.md` and the relevant roadmap task when implementation is finished.

9) If uncertain, ask the user to provide:
- Which area to modify (Frontend vs Backend vs Docs)
- Whether local LLM integration is available (Ollama running) or a remote LLM should be used

References: `Docs/README.md`, `Docs/Developer/Getting-Started.md`, `Docs/Technical/Architecture.md`, `src/backend/DerotMyBrain.API/Program.cs`, `src/frontend`.

---
If anything here is unclear or you want a different focus (e.g., deeper front-end patterns or CI instructions), tell me and I'll iterate.
