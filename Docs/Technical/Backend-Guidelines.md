# Backend Guidelines & Golden Rules

**Role**: Entry point for all Backend development. Start here.

> [!IMPORTANT]
> **AI AGENT INSTRUCTIONS**: You MUST read and follow these rules. Violating "Critical Constraints" constitutes a task failure.

## 1. Critical Constraints

1.  **Strict Clean Architecture**:
    *   **Core** (Domain) depends on **Nothing**.
    *   **Infrastructure** depends on **Core**.
    *   **API** depends on **Core** and **Infrastructure**.
    *   *See [Backend-Architecture.md](Backend-Architecture.md)*

2.  **Storage Policy (SQLite Only)**:
    *   V1 uses **SQLite** + **EF Core** (Embedded).
    *   **FORBIDDEN**: SQL Server, Docker containers for DB, Cloud DBs.
    *   *See [Storage-Policy.md](Storage-Policy.md)*

3.  **Test-Driven Development (TDD)**:
    *   You **MUST** write failing tests (Red) before implementing features (Green).
    *   Use `xUnit` + `Moq`.
    *   *See [Testing-Strategy.md](Testing-Strategy.md)*

4.  **Mock Data Requirement**:
    *   All new features **MUST** include mock data for `TestUser` (`test-user-id-001`).

## 2. Detailed Documentation

*   **[Backend-Architecture.md](Backend-Architecture.md)**: Layers, Dependency Rules, External Integrations (LLM, Wiki).
*   **[Backend-Coding-Standards.md](Backend-Coding-Standards.md)**: SOLID details, Naming Conventions, Logging, Error Handling.
*   **[Backend-API-Standards.md](Backend-API-Standards.md)**: REST patterns, DTOs, Validation.

## 3. Quick Reference Checklist

- [ ] Does my Service return a DTO (not Entity)?
- [ ] Did I create an Interface in Core for this new external dependency?
- [ ] Is my Controller thin (only calling Services)?
- [ ] Did I write a Unit Test for this logic?
- [ ] Did I adhere to the `TestUser` mock data rule?
