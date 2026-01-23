# Derot My Brain – Architecture (Clean Architecture Edition)

## Table of Contents
1. [High-Level Architecture](#high-level-architecture)
2. [Project Breakdown & Responsibilities](#project-breakdown--responsibilities)
3. [Layer interactions](#layer-interactions)
4. [Agent Interaction Guidelines](#agent-interaction-guidelines)

---

## 1. High-Level Architecture

The system follows **Clean Architecture** principles involving N-Tier separation. The core domain logic is independent of external frameworks, databases, or UI.

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

## 2. Project Breakdown & Responsibilities

### **1. DerotMyBrain.Core** (Class Library)
**Role**: The "Inner Circle". It resembles the Domain and Application layers.
**Dependencies**: NONE (Pure C#).
**Responsibilities**:
- **Entities**: Represent the domain state (e.g., `User`, `UserActivity`, `TrackedTopic`, `AppConfiguration`).
- **Interfaces**: Define contracts for external services (e.g., `IRepository`, `ILlmService`, `IContentSource`).
- **Domain Services**: Business logic that orchestrates entities (e.g., `ActivityService`, `TrackedTopicService`, `SeedDataService`).

### **2. DerotMyBrain.Infrastructure** (Class Library)
**Role**: The "Outer Circle". Implements interfaces defined in Core.
**Dependencies**: `DerotMyBrain.Core`, `EntityFrameworkCore`, HTTP Clients.
**Responsibilities**:
- **Data Persistence**: `DerotDbContext` (SQLite), Repositories implementation.
- **External API Clients**:
    - **WikipediaContentSource**: Fetches and parses Wikipedia articles.
    - **OllamaLlmService**: Communicates with the local LLM for quiz generation.
    - **FileContentSource**: Reads uploaded documents.
    - **ConfigurationService**: Manages app settings.

### **3. DerotMyBrain.API** (ASP.NET Core Web API)
**Role**: The "Presentation Layer". Entry point for the frontend.
**Dependencies**: `DerotMyBrain.Core`, `DerotMyBrain.Infrastructure`.
**Responsibilities**:
- **Controllers**: Handle HTTP Requests/Responses (`ActivitiesController`).
- **Dependency Injection**: Wires up Core interfaces to Infrastructure implementations.
- **DTOs**: Defines the structure of data sent to/from the Frontend.

---

## 3. Layer Interactions

### **Example: Generic Quiz Generation Flow**

1.  **API**: Receives `POST /api/activities/start` with a Wiki URL.
2.  **API**: Calls `ActivityService.StartReadingAsync(url)` (in Core).
3.  **Core**: Calls `IContentSource.GetContentAsync(url)`.
4.  **infra**: `WikipediaContentSource` fetches the HTML, strips tags, returns text.
5.  **Core**: Creates `UserActivity` entity with the content and status `Reading`.
6.  **API**: Returns the clean text to the frontend for display.
7.  **(Later)** User clicks "Generate Quiz".
8.  **API**: Calls `ActivityService.GenerateQuizAsync(activityId)` (in Core).
9.  **Core**: Calls `ILlmService.GenerateQuestions(content)`.
10. **Infra**: `OpenAiLlmService` prompts the LLM and parses the JSON response.
11. **Core**: Saves questions to `UserActivity` and returns them.

---

## 4. Agent Interaction Guidelines

To ensure consistency and prevent data pollution during development and testing, all AI Agents (like Antigravity) should follow these rules:

1.  **Test User**: Always use the designated test account for mock data creation and automated testing.
    - **Name**: `TestUser`
    - **ID**: `test-user-id-001`
2.  **Data Storage**: Mock data should be seeded in the SQLite database (`derot-my-brain.db`).
3.  **Logical Consistency**: Ensure that any mock data created is logically consistent with the `TestUser` profile.
