# Frontend Architecture

**Role**: Defines the structural foundation of the frontend application.

## 1. Clean Architecture Layers

The frontend follows a layered architecture adapted for React. Dependencies flow **inwards** or "downwards".

### **1. UI Layer (Presentation)**
*   **Role**: Pure rendering and user interaction.
*   **Location**: `/components`, `/pages`.
*   **Responsibilities**:
    *   Receive data via Props.
    *   Emit events via Callbacks.
    *   **NO** Side effects (API calls).
    *   **NO** Complex State logic.

### **2. Application Layer (Orchestration)**
*   **Role**: Glue between UI and Domain.
*   **Location**: `/hooks`, `/stores`.
*   **Responsibilities**:
    *   **Custom Hooks**: `useAuth`, `useUser`, `usePreferences`.
    *   **State Management**: Zustand stores.
    *   **Orchestration**: Calling API services and updating state.

### **3. Domain Layer (Business Logic)**
*   **Role**: Business rules and Data definitions.
*   **Location**: `/services`, `/models`.
*   **Responsibilities**:
    *   **Models**: TypeScript Interfaces (`User`, `Activity`).
    *   **Services**: Pure logic (e.g., `ScoreCalculator`).

### **4. Infrastructure Layer (External)**
*   **Role**: Interaction with the outside world.
*   **Location**: `/api`, `/utils`.
*   **Responsibilities**:
    *   **API Client**: Axios instance (`apiClient.ts`).
    *   **Endpoints**: Raw HTTP calls (`userApi.ts`).
    *   **DTOs**: Request/Response shapes.

---

## 2. Directory Structure

```text
src/
├── components/        # UI Layer
│   ├── ui/           # Shared (shadcn) components (Button, Card)
│   └── features/     # Feature-specific components (LoginForm)
├── pages/            # UI Layer (Page definitions for Router)
├── hooks/            # Application Layer (Logic orchestration)
├── stores/           # Application Layer (Global State)
├── services/         # Domain Layer (Business Logic)
├── models/           # Domain Layer (Types)
├── api/              # Infrastructure Layer (HTTP)
└── utils/            # Infrastructure Layer (Helpers)
```

---

## 3. Separation of Concerns (SoC) Rules

### **The "Dumb Component" Rule**
*   **Constraint**: A UI component (`.tsx`) should never import from `/api` or `/services`.
*   **Pattern**: It should import a **Custom Hook** (`/hooks`) that handles the logic.

**❌ BAD:**
```tsx
import { userApi } from '@/api/userApi'; // Direct Infra dependency!

function Profile() {
  const [user, setUser] = useState();
  useEffect(() => { userApi.get().then(setUser) }, []);
  // ...
}
```

**✅ GOOD:**
```tsx
import { useUser } from '@/hooks/useUser'; // Application Layer dependency

function Profile() {
  const { user, isLoading } = useUser();
  if (isLoading) return <Spinner />;
  return <ProfileView user={user} />;
}
```

### **Unidirectional Data Flow**
*   **Rule**: Props go Down. Events go Up.
*   **Constraint**: Avoid "Two-Way Binding" complexity. Lift state up to Hooks or Stores.
