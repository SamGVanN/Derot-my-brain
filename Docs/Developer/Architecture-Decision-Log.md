# Architecture Decision Log

**Date:** 2026-01-23
**Topic:** Clean Architecture vs Practical Constraints (V1)

## Context
We are implementing a strict **Clean Architecture** (N-Tier) backend for the "Derot My Brain" project. The project involves AI Agents as core developers, necessitating strict, explicit rules. The project also relies on **SQLite (Embedded)** and must interact with external systems (LLM, Wikipedia).

## Decision Analysis

We reviewed the compatibility of the proposed architectural rules against practical development constraints.

### 1. Pure Domain vs. Pragmatism (Entity Framework)
*   **The Conflict**: "Core MUST NOT reference Entity Framework Core". This prevents using simple `[MaxLength]`, `[Required]` attributes on Entities if they serve as EF configurations.
*   **The Decision**: **Maintain Strict Purity**.
    *   **Reasoning**: Keeping the Core independent ensures the domain logic is testable without any infrastructure dependencies. It makes the system robust against future DB changes (e.g., swapping SQLite for LiteDB or DocumentDB).
    *   **Implication**: Database configuration must be handled via the Fluent API in the `DbContext` (`OnModelCreating`), located in the Infrastructure layer.
    *   **Verdict**: **Acceptable Trade-off**. The verbosity in `DbContext` buys us long-term stability and clarity for Agents.

### 2. KISS vs. N-Tier Layers
*   **The Conflict**: Reading a simple entity requires traversing Controller -> Service -> Interface -> Repository -> Interface -> Implementation. This violates KISS (Keep It Simple Stupid) for simple CRUD operations.
*   **The Decision**: **Enforce N-Tier Structure**.
    *   **Reasoning**: While heavy for simple CRUD, this structure provides the necessary "slots" for future complexity (LLM orchestration, Caching, Event publishing) without refactoring the entire flow. Agents thrive on consistent patterns; exceptions to the rule ("just use the DB directly here") cause confusion and "spaghetti code".
    *   **Verdict**: **Necessary Complexity**. The consistency is more valuable than the initial speed of writing direct-access code.

### 3. TDD vs. Embedded Database (SQLite)
*   **The Conflict**: TDD requires fast feedback. Spinning up a real SQLite file for every unit test is slow and messy (file locking, cleanup).
*   **The Decision**: **Use EF Core In-Memory for Unit Tests, SQLite for Integration**.
    *   **Reasoning**: `Testing-Strategy.md` explicitly allows EF Core InMemory for business logic tests. This preserves the speed of TDD.
    *   **Verdict**: **Resolved**. The Testing Strategy mitigates the performance risk.


## 2. Frontend Architecture Decisions

### 2.1 Decision: Strict "Dumb Component" Rule
*   **Context**: React components often become bloated with `useEffect`, API calls, and complex transformation logic.
*   **Decision**: **UI Components MUST NOT import API clients or Services.** All logic must be extracted to **Custom Hooks**.
*   **Trade-off**: Requires creating a `useFeature.ts` for almost every smart component. This increases file count (Verbosity).
*   **Rationale**:
    *   **Testability**: Hooks can be tested in isolation (using `renderHook`) without rendering the UI.
    *   **Reusability**: Logic can be reused content-agnostically.
    *   **Maintainability**: "View" is separated from "Behavior".

### 2.2 Decision: Store Encapsulation (No Direct Store Access)
*   **Context**: Using state management libraries (like Zustand, Redux) directly in components couples the UI to that specific library.
*   **Decision**: **UI Components MUST NOT import Zustand stores directly.** They must use a Custom Hook wrapper (e.g., `useAuth` wraps `useAuthStore`).
*   **Trade-off**: High boilerplate. Every store needs a facade hook.
*   **Rationale**:
    *   **Decoupling**: We can swap Zustand for Context or Signals without touching the UI.
    *   **Composition**: A Facade Hook can combine data from multiple stores (e.g., `useDashboard` combines `useUser` and `useActivity`), simplifying consumption.

### 2.3 Distinction: UI State vs. Business Logic
*   **Clarification**: "No Logic in Components" does **NOT** ban `useState`.
    *   **Allowed**: UI State (e.g., `isDropdownOpen`, `formInputValue`).
    *   **Banned**: Business State (e.g., `userProfile`, `calculationResult`).
*   **Why**: Passing every single boolean through a custom hook is overkill (KISS). UI ephemeral state belongs in the View.

## Conclusion
The architectural guidelines are strictly compatible with the project goals. The identified friction points are deliberate design choices to favor **Maintainability** and **Agent-Readability** over initial development speed.
