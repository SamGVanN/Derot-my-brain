# Backend API Standards

**Role**: Guidelines for exposing the Core logic via HTTP REST API.

## 1. REST Conventions

*   **Resource Naming**: Plural nouns (e.g., `/api/users`, `/api/activities`).
*   **HTTP Verbs**:
    *   `GET`: Read (Safe, Idempotent).
    *   `POST`: Create (Not Idempotent).
    *   `PUT`: Update/Replace (Idempotent).
    *   `DELETE`: Remove (Idempotent).
*   **Hierarchy**: Use nested routes for proprietary resources.
    *   `GET /api/users/{userId}/activities` (Activities belonging to a user)

## 2. Request/Response DTOs

*   **Rule**: NEVER return Entity classes (e.g., `User`) directly. Always map to a DTO.
*   **Naming**: `{Entity}{Action}Dto`.
    *   Request: `CreateUserDto`, `UpdateActivityDto`
    *   Response: `UserDto`, `ActivitySummaryDto`
*   **Mapping**: Use manual mapping or `Mapster` (avoid heavy AutoMapper if possible to keep it KISS, but AutoMapper is acceptable if configured strictly).

## 3. Validation

*   **Input Validation**: Validate DTOs at the Controller entry level.
*   **Libraries**: `FluentValidation` (Preferred) or DataAnnotations.
*   **Constraint**: Fail fast. Return `400 Bad Request` with specific field errors if validation fails.

## 4. Response Structure

*   **Success**:
    *   `200 OK`: Payload in body.
    *   `201 Created`: `Location` header set to new resource URL.
    *   `204 No Content`: For void actions (Delete).
*   **Error**:
    *   Standardized Problem Details (RFC 7807) or simple JSON Error wrapper.
