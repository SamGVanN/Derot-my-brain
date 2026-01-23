# Frontend Guidelines & Golden Rules

**Role**: Entry point for all Frontend development. Start here.

> [!IMPORTANT]
> **AI AGENT INSTRUCTIONS**: You MUST read and follow these rules. Violating "Critical Constraints" constitutes a task failure.

## 1. Critical Constraints

1.  **Strict Separation of Concerns**:
    *   **UI Components** MUST BE "Dumb" (Presentation only).
    *   **NO** direct API calls in Components (Use Custom Hooks).
    *   **NO** business logic in Components (Use Custom Hooks).
    *   *See [Frontend-Architecture.md](Frontend-Architecture.md)*

2.  **Theming & Styling**:
    *   **NEVER** use hardcoded colors (e.g., `bg-white`, `text-black`).
    *   **ALWAYS** use semantic Tailwind classes (e.g., `bg-background`, `text-foreground`).
    *   **ALWAYS** support both Light and Dark modes.
    *   *See [Frontend-Coding-Standards.md](Frontend-Coding-Standards.md)*

3.  **State Management**:
    *   Use **Zustand** for global state.
    *   **NEVER** export stores directly; export Custom Hooks.
    *   *See [Frontend-State-Management.md](Frontend-State-Management.md)*

4.  **Routing**:
    *   Use **React Router v7** declarative routing.
    *   PROTECT routes using wrapper components (`ProtectedRoute`).
    *   *See [Frontend-Routing.md](Frontend-Routing.md)*

## 2. Detailed Documentation

*   **[Frontend-Architecture.md](Frontend-Architecture.md)**: Layers, Directory Structure, Clean Architecture.
*   **[Frontend-Coding-Standards.md](Frontend-Coding-Standards.md)**: React Patterns, Component Rules, Theming.
*   **[Frontend-State-Management.md](Frontend-State-Management.md)**: Zustand Stores, Custom Hooks, Actions.
*   **[Frontend-Routing.md](Frontend-Routing.md)**: Navigation patterns, URL structure.

## 3. Quick Reference Checklist

- [ ] Is my component "dumb" (no logic)?
- [ ] Did I move logic to `hooks/useFeature.ts`?
- [ ] Did I use `bg-card` instead of `bg-white`?
- [ ] Did I use `NavLink` for menu items?
- [ ] Did I avoid `useEffect` for data fetching (used a hook instead)?
