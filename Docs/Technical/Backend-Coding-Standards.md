# Backend Coding Standards

**Role**: Rules for writing maintainable, standard-compliant C# code.

## 1. SOLID & Patterns Application

### **Single Responsibility (SRP)**
*   **Rule**: A class should do one thing.
*   **Constraint**: Do not mix "Data Access" (SQL) validity with "Business Rule" (Quiz logic) in the same class.

### **Dependency Injection (DIP)**
*   **Rule**: **Controllers** inject **Services** (Interfaces). **Services** inject **Repositories** (Interfaces).
*   **Constraint**: NEVER inject `DbContext` directly into a Controller.

### **Interface Segregation (ISP)**
*   **Rule**: Keep interfaces small.
*   **Constraint**: Don't create a massive `IGlobalService`. Break it down: `IUserService`, `IActivityService`.

---

## 2. Code Style & Naming

*   **Classes/Methods**: `PascalCase` (e.g., `UserRepository`, `GetByIdAsync`).
*   **Private Fields**: `_camelCase` with underscore (e.g., `_logger`, `_context`).
*   **Async/Await**:
    *   **ALWAYS** use `async/await` for I/O.
    *   **SUFFIX** async methods with `Async` (e.g., `SaveAsync`).
    *   **NEVER** use `.Result` or `.Wait()` (Deadlock risk).

---

## 3. Logging Strategy

*   **Library**: **Serilog**.
*   **Requirement**: Structured Logging.
*   **Pattern**:
    ```csharp
    // ✅ GOOD
    _logger.LogInformation("Processing activity {ActivityId} for user {UserId}", activity.Id, userId);
    
    // ❌ BAD (Unstructured)
    _logger.LogInformation($"Processing activity {activity.Id} for user {userId}");
    ```
*   **Constraint**: Log **Exceptions** with stack traces. Log **Business Events** (Quiz Created, User Registered) at Information level.

---

## 4. Error Handling

*   **Custom Exceptions**: Define in **Core**.
    *   `EntityNotFoundException` (Maps to 404)
    *   `DomainValidationException` (Maps to 400)
*   **Global Handler**: Use Middleware to catch these exceptions and return consistent JSON error responses.
    ```json
    {
      "error": "User with ID 123 not found",
      "code": "ENTITY_NOT_FOUND" 
    }
    ```
*   **Constraint**: Do not use `try/catch` in Controllers for control flow. Let Middleware handle "Expected" exceptions.
