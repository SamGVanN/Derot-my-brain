# Backend Architecture

**Role**: Defines the "Shape" of the system, layer responsibilities, and dependency rules.

## 1. Clean Architecture Layers

The system follows a strict **Onion / Clean Architecture** model. Dependencies flow **inwards**.

### **1. Core (DerotMyBrain.Core)**
*   **Role**: The "Inner Circle". Contains Enterprise Logic and Application Business Rules.
*   **Dependencies**: **NONE**. Pure C# only. No EF Core, no HTTP, no JSON libraries.
*   **Contents**:
    *   **Entities**: Pure domain objects (e.g., `User`, `UserActivity`).
    *   **Interfaces**: Contracts for *everything* external (e.g., `IRepository`, `ILlmService`, `IContentSource`).
    *   **Domain Services**: Pure logic services (e.g., `QuizGeneratorService`).
    *   **Exceptions**: Domain-specific exceptions (e.g., `DomainException`).

### **2. Infrastructure (DerotMyBrain.Infrastructure)**
*   **Role**: The "Outer Circle". Implements interfaces defined in Core.
*   **Dependencies**: Projects: `Core`; Libraries: `EntityFrameworkCore`, `Serilog`, `HttpClient`.
*   **Contents**:
    *   **Data Persistence**: `SqliteUserRepository`, `DerotDbContext`.
    *   **External Services**: `OllamaLlmService`, `WikipediaContentSource`.
    *   **File I/O**: `FileIngestionService`.

### **3. API (DerotMyBrain.API)**
*   **Role**: Presentation Layer / Entry Point.
*   **Dependencies**: `Core`, `Infrastructure`.
*   **Contents**:
    *   **Controllers**: REST endpoints.
    *   **DTOs**: Request/Response models.
    *   **Program.cs**: Dependency Injection wiring.
    *   **Middleware**: Global exception handling, Logging.

---

## 2. Dependency Rules (Strict)

1.  **Core** MUST NOT reference **Infrastructure** or **API**.
2.  **Infrastructure** MUST NOT reference **API**.
3.  **Core** MUST NOT reference **Entity Framework Core** (No `DbSet`, no `DataAnnotations` that are EF/SQL specific).
4.  **Controllers** MUST NOT contain business logic. They only orchestrate **Services**.

---

## 3. External Interactions

External systems are abstracted via Interfaces in **Core** and implemented in **Infrastructure**.

### **LLM Interaction**
*   **Core**: `activeService.GenerateQuiz()` calls `ILlmService.GenerateQuestionsAsync()`.
*   **Infrastructure**: `OllamaLlmService` implements `ILlmService`, calls local Ollama instance via HTTP.

### **Content Sources**
*   **Core**: `IContentSource.FetchAsync(url)`.
*   **Infrastructure**: 
    *   `WikipediaContentSource`: Scrapes/Cleans Wikipedia HTML.
    *   *(Future)* `FileContentSource`: Reads uploaded PDF/Text files.

### **Future Capability: File Input**
*   **Pattern**: To support file uploads from frontend:
    1.  **API**: Accepts `IFormFile`.
    2.  **Core**: Defines `IDocumentProcessor` interface.
    3.  **Infrastructure**: Implements `IDocumentProcessor` (using minimal libraries like `PdfSharp` or distinct micro-services if complex).
