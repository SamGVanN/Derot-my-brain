# Derot My Brain – System Architecture

## 1. High-Level Overview

The system follows **Clean Architecture** principles involving N-Tier separation. The core domain logic is independent of external frameworks, databases, or UI.

For detailed architecture rules, see:
*   **Backend**: [Backend-Architecture.md](Backend-Architecture.md)
*   **Frontend**: [Frontend-Architecture.md](Frontend-Architecture.md)

```ascii
┌────────────────────────────────────────────────────────┐
│                      Presentation                      │
│  ┌──────────────┐    ┌──────────────────────────────┐  │
│  │   Frontend   │───►│       DerotMyBrain.API       │  │
│  │ (React/TSX)  │    │      (ASP.NET Core 9)        │  │
│  └──────────────┘    └──────────────┬───────────────┘  │
└─────────────────────────────────────│──────────────────┘
                                      │
           ┌──────────────────────────▼──────────────────────────┐
           │                     Infrastructure                  │
           │               DerotMyBrain.Infrastructure           │
           │                                                     │
           │  ┌────────────┐   ┌─────────────┐   ┌────────────┐  │
           │  │ EF Core DB │   │ LLM Client  │   │ Wiki Client│  │
           │  └─────┬──────┘   └──────┬──────┘   └─────┬──────┘  │
           └────────│─────────────────│────────────────│─────────┘
                    │                 │                │
┌───────────────────▼─────────────────▼────────────────▼───────────────────┐
│                                   Core                                   │
│                           DerotMyBrain.Core                              │
│                                                                          │
│      ┌────────────┐         ┌──────────────┐         ┌────────────┐      │
│      │  Entities  │         │  Interfaces  │         │  Services  │      │
│      └────────────┘         └──────────────┘         └────────────┘      │
└──────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Layer Responsibilities

### **1. Core (DerotMyBrain.Core)**
*   **Role**: The Domain. Pure C#.
*   **Responsibilities**: Entities, Business Logic (Services), Abstract Interfaces.

### **2. Infrastructure (DerotMyBrain.Infrastructure)**
*   **Role**: Implementation details.
*   **Responsibilities**: Database (SQLite + EF Core), External APIs (Wikipedia, LLM).

### **3. API (DerotMyBrain.API)**
*   **Role**: Entry point.
*   **Responsibilities**: Controllers, Dependency Injection, DTOs.

### **4. Frontend**
*   **Role**: User Interface.
*   **Responsibilities**: React + TypeScript, consumes the REST API.
*   See **[Frontend-Architecture.md](Frontend-Architecture.md)** for details.

---

## 3. Agent Interaction Guidelines

To ensure consistency and prevent data pollution during development and testing, all AI Agents (like Antigravity) should follow these rules:

1.  **Test User**: Always use the designated test account for mock data creation and automated testing.
    *   **Name**: `TestUser`
    *   **ID**: `test-user-id-001`
2.  **Data Storage**: Mock data should be seeded in the SQLite database (`derot-my-brain.db`).
3.  **Logical Consistency**: Ensure that any mock data created is logically consistent with the `TestUser` profile.

---

## 4. Related Documentation

### Backend
- [Backend-Architecture.md](Backend-Architecture.md)
- [Backend-Coding-Standards.md](Backend-Coding-Standards.md)
- [Backend-API-Standards.md](Backend-API-Standards.md)
- [Storage-Policy.md](Storage-Policy.md)

### Frontend
- [Frontend-Guidelines.md](Frontend-Guidelines.md) (Entry Point)
- [Frontend-Architecture.md](Frontend-Architecture.md)
- [Frontend-Coding-Standards.md](Frontend-Coding-Standards.md)
- [Frontend-State-Management.md](Frontend-State-Management.md)
- [Frontend-Routing.md](Frontend-Routing.md)
