# Frontend State Management

**Role**: Managing data flow and application state using **Zustand**.

## 1. Zustand Patterns

### **Store Structure**
*   **Rule**: Create small, domain-specific stores.
    *   `useAuthStore`
    *   `usePreferencesStore`
*   **Constraint**: DO NOT create one giant `RootStore`.

### **Separation of Actions**
*   **Pattern**: Separate state properties from actions.
    ```tsx
    interface UserState {
      user: User | null;
      actions: {
        login: (user: User) => void;
        logout: () => void;
      }
    }
    ```

### **Events vs Setters**
*   **Naming**: Name actions based on **Events** (`onLoginSuccess`), not just setters (`setUser`).
    *   *Why?* It describes *what happened*, making the logic clearer.

---

## 2. Encapsulation (Mandatory)

**CRITICAL RULE**: **UI Components MUST NOT import Stores directly.**

1.  **Define Store**: Internal to `/stores/`.
2.  **Export Hook**: Export a Custom Hook in `/hooks/` that wraps the store.

**Reasoning**:
*   Decouples UI from Zustand (we could switch to Redux/Context later).
*   Allows composing multiple stores into one logical hook.

**Example**:
```tsx
// ❌ UI calls Store
import { useAuthStore } from '@/stores/useAuthStore';
const { user } = useAuthStore();

// ✅ UI calls Hook
import { useAuth } from '@/hooks/useAuth';
const { user, login } = useAuth(); // Hook wraps store + API calls logic
```

---

## 3. Required Custom Hooks

Implement these "Facade Hooks" for key features:
*   `useAuth()`: Session, Login, Logout.
*   `useUser()`: Current user details.
*   `usePreferences()`: Theme, Language settings.
*   `useHistory()`: User activity log.
