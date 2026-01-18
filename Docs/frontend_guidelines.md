# Frontend Development Guidelines

## Theming and Styling

Derot My Brain uses a dynamic theming system. **All new UI components and pages MUST adhere to the following rules:**

### 1. Semantic Colors
**NEVER use hardcoded colors** like `text-white`, `bg-black`, `bg-slate-900`, etc. for primary UI elements.
Instead, use the semantic classes provided by `tailwindcss` which map to our CSS variables:

-   **background**: `bg-background` (Page background)
-   **foreground**: `text-foreground` (Primary text color)
-   **muted**: `text-muted-foreground` (Secondary/description text)
-   **card**: `bg-card` (Card background)
-   **card-foreground**: `text-card-foreground` (Card text)
-   **border**: `border-border` (Borders)
-   **primary**: `bg-primary`, `text-primary-foreground` (Primary actions/highlights)
-   **secondary**: `bg-secondary`, `text-secondary-foreground` (Secondary actions)

### 2. Components
Use the shared UI components from `@/components/ui` whenever possible (e.g., `Button`, `Card`, `Input`). These components are already configured to respond correctly to theme changes.

### 3. Theme Compatibility & Light Mode
**CRITICAL**: Always ensure your component looks good in **BOTH** Light and Dark modes.
- Avoid hardcoded offsets like `text-white/60` if the background might be white in Light mode.
- Use `bg-muted` or `bg-card` for containers instead of `bg-white/10` or `bg-black/10`.
- If a gradient text (e.g., `bg-clip-text`) is used, ensure the fallback or the gradient itself remains visible on all backgrounds, or switch to `text-foreground`.

### 4. Verification
After implementing a new feature, ALWAYS test it by switching themes (Light vs Dark vs Custom) to ensure all elements remain legible and aesthetically pleasing.

### 5. Hover Effects
Consistency in hover effects is crucial for the theme system.
- **Rule**: Interactive elements (list items, custom buttons) MUST use the `accent` color for hover states (`hover:bg-accent`, `hover:text-accent-foreground`), NOT `primary`.
- **Reasoning**: Some themes (e.g., Mind Lab) use distinct colors for Primary (Action) and Accent (Interaction/Feedback).
- **Implementation**: Prefer using standard `variant="ghost"` or `variant="outline"` which handle this automatically. If manually styling, use `hover:bg-accent` instead of `hover:bg-primary/x`.

### 6. Icon & Nested Element Rules
- **Rule**: Icons or containers inside interactive elements that adapt on hover MUST also use the `accent` color family to match their parent.
- **Example**: If a button turns `bg-accent` on hover, an icon inside it should stay visible or adapt using `group-hover:text-accent-foreground`. Avoid using `group-hover:text-primary` as this breaks the theme consistency.

---

## Frontend Architecture Principles

Just as the backend follows **SOLID principles**, the frontend must adhere to these core architectural principles to ensure maintainability, testability, and scalability.

### 1. Separation of Concerns (SoC)
**Each component, hook, or module should have a single, well-defined responsibility.**

- **UI Components** should focus solely on presentation and user interaction
- **Business Logic** should be extracted into custom hooks or services
- **Data Fetching** should be handled by dedicated services or hooks
- **State Management** should be centralized and separated from UI logic

**Example:**
```tsx
// ❌ BAD: Mixed concerns
function UserProfile() {
  const [user, setUser] = useState(null);
  
  useEffect(() => {
    fetch('/api/users/123').then(res => res.json()).then(setUser);
  }, []);
  
  return <div>{user?.name}</div>;
}

// ✅ GOOD: Separated concerns
function UserProfile() {
  const { user } = useUser(); // Business logic in hook
  return <UserProfileView user={user} />; // Pure presentation
}
```

### 2. Component-Driven Architecture
**Build the UI as a composition of small, reusable, and independent components.**

- **One Component = One Responsibility**
- Components should be composable and reusable
- Prefer small, focused components over large monolithic ones
- Use component composition to build complex UIs

**Guidelines:**
- Maximum 150-200 lines per component (excluding types)
- If a component grows too large, split it into smaller sub-components
- Extract repeated UI patterns into shared components

### 3. Composition Over Inheritance
**Favor component composition and props over class inheritance or complex component hierarchies.**

- Use **props** and **children** to compose behavior
- Leverage **render props** or **compound components** for flexible APIs
- Avoid deep component inheritance chains
- Use **Higher-Order Components (HOCs)** sparingly; prefer hooks

**Example:**
```tsx
// ✅ GOOD: Composition
function Card({ children, header, footer }) {
  return (
    <div className="card">
      {header && <div className="card-header">{header}</div>}
      <div className="card-body">{children}</div>
      {footer && <div className="card-footer">{footer}</div>}
    </div>
  );
}

// Usage
<Card 
  header={<h2>Title</h2>}
  footer={<Button>Action</Button>}
>
  Content here
</Card>
```

### 4. Unidirectional Data Flow
**Data should flow in a single direction: from parent to child via props, and from child to parent via callbacks.**

- **Props down, events up** pattern
- Avoid two-way binding when possible
- State should be lifted to the nearest common ancestor
- Use context or state management for deeply nested prop drilling

**Example:**
```tsx
// ✅ GOOD: Unidirectional flow
function ParentComponent() {
  const [count, setCount] = useState(0);
  
  return (
    <ChildComponent 
      count={count} 
      onIncrement={() => setCount(c => c + 1)} 
    />
  );
}

function ChildComponent({ count, onIncrement }) {
  return <button onClick={onIncrement}>Count: {count}</button>;
}
```

### 5. Custom Hooks for Business Logic
**Extract all business logic, side effects, and stateful operations into custom hooks.**

This keeps components "dumb" (focused on UI) and makes logic reusable and testable.

**Required Custom Hooks:**
- `useAuth()` - Authentication state and operations
- `useCart()` - Shopping cart logic (if applicable)
- `usePermissions()` - User permissions and authorization
- `useUser()` - Current user data and operations
- `useCategories()` - Category management
- `useQuiz()` - Quiz state and operations
- `useWikipedia()` - Wikipedia article fetching
- etc.

**Example:**
```tsx
// ✅ GOOD: Business logic in hook
function useAuth() {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  
  const login = async (credentials) => {
    const user = await authService.login(credentials);
    setUser(user);
  };
  
  const logout = () => {
    authService.logout();
    setUser(null);
  };
  
  return { user, loading, login, logout };
}

// Component is "dumb" - just UI
function LoginPage() {
  const { login, loading } = useAuth();
  return <LoginForm onSubmit={login} loading={loading} />;
}
```

### 6. Clean Architecture / Hexagonal Architecture (Adapted for Frontend)
**Organize code in layers with clear boundaries and dependencies flowing inward.**

**Layers (from outer to inner):**
1. **UI Layer** (`/components`, `/pages`) - React components, pure presentation
2. **Application Layer** (`/hooks`, `/contexts`) - Application-specific logic, custom hooks
3. **Domain Layer** (`/services`, `/models`) - Business logic, data models, API services
4. **Infrastructure Layer** (`/api`, `/utils`) - External dependencies, HTTP clients, utilities

**Dependency Rule:** 
- Outer layers can depend on inner layers
- Inner layers should NOT depend on outer layers
- Use **dependency injection** via props or context

**Directory Structure Example:**
```
src/
├── components/        # UI Layer (presentation)
│   ├── ui/           # Shared UI components
│   └── features/     # Feature-specific components
├── pages/            # UI Layer (page components)
├── hooks/            # Application Layer (custom hooks)
├── contexts/         # Application Layer (React contexts)
├── services/         # Domain Layer (business logic, API calls)
├── models/           # Domain Layer (TypeScript types/interfaces)
├── api/              # Infrastructure Layer (HTTP client, API config)
└── utils/            # Infrastructure Layer (helpers, formatters)
```

### 7. Dependency Injection
**Inject dependencies via props or context rather than importing directly.**

- Components should receive dependencies as props
- Use React Context for app-wide dependencies
- Makes components more testable and flexible
- Reduces tight coupling

**Example:**
```tsx
// ✅ GOOD: Dependencies injected
function UserList({ userService }) {
  const [users, setUsers] = useState([]);
  
  useEffect(() => {
    userService.getAll().then(setUsers);
  }, [userService]);
  
  return <ul>{users.map(u => <li key={u.id}>{u.name}</li>)}</ul>;
}

// Or via context
const UserServiceContext = createContext(null);

function UserList() {
  const userService = useContext(UserServiceContext);
  // ... rest of component
}
```

### 8. Keep UI Components "Dumb"
**UI components should be as simple and declarative as possible.**

- Minimal logic in JSX
- No direct API calls in components
- No complex calculations in render
- Use hooks for side effects and state
- Props should be simple and well-typed

**Summary:**
- ✅ **One component = One responsibility**
- ✅ **Composition > Inheritance**
- ✅ **Business logic in custom hooks**
- ✅ **UI components as "dumb" as possible**
- ✅ **Dependencies injected (props/context)**
- ✅ **Unidirectional data flow**

---

## State Management: Zustand

For this project, **Zustand** is the recommended state management solution over Redux.

### Why Zustand?

1. **App Complexity**: Small to medium-sized application
2. **Minimal Boilerplate**: ~1KB vs Redux's ~15KB
3. **Simpler API**: Easier to learn and use
4. **Equivalent Performance**: No performance trade-offs
5. **Easy Migration**: Can migrate to Redux later if needed

### When to Use Zustand

Use Zustand for:
- **Global application state** (user session, preferences, theme)
- **Shared state** across multiple components
- **Complex state logic** that doesn't fit in a single component
- **State that needs to persist** across route changes

**Don't use Zustand for:**
- Local component state (use `useState`)
- Server state (use React Query or SWR)
- Form state (use React Hook Form or similar)

### Zustand Best Practices

1. **Create Focused Stores**: One store per domain/feature
   ```tsx
   // stores/useAuthStore.ts
   export const useAuthStore = create((set) => ({
     user: null,
     login: (user) => set({ user }),
     logout: () => set({ user: null }),
   }));
   
   // stores/usePreferencesStore.ts
   export const usePreferencesStore = create((set) => ({
     theme: 'derot-brain',
     language: 'en',
     setTheme: (theme) => set({ theme }),
     setLanguage: (language) => set({ language }),
   }));
   ```

2. **Use Selectors**: Only subscribe to needed state
   ```tsx
   // ✅ GOOD: Selective subscription
   const user = useAuthStore((state) => state.user);
   
   // ❌ BAD: Subscribe to entire store
   const { user, login, logout } = useAuthStore();
   ```

3. **Separate Actions from State**: Keep stores organized
   ```tsx
   export const useUserStore = create((set, get) => ({
     // State
     users: [],
     loading: false,
     
     // Actions
     fetchUsers: async () => {
       set({ loading: true });
       const users = await userService.getAll();
       set({ users, loading: false });
     },
   }));
   ```

4. **Persist Important State**: Use Zustand middleware
   ```tsx
   import { persist } from 'zustand/middleware';
   
   export const useAuthStore = create(
     persist(
       (set) => ({
         user: null,
         login: (user) => set({ user }),
         logout: () => set({ user: null }),
       }),
       { name: 'auth-storage' }
     )
   );
   ```

### Integration with Architecture Principles

Zustand stores should:
- Be defined in the **Application Layer** (`/stores` or `/hooks`)
- Contain **business logic** and **state management**
- Be **injected** into components via hooks
- Follow **unidirectional data flow** (actions modify state, components react)
- Keep **UI components dumb** (they just consume store state)

**Example Integration:**
```tsx
// stores/useQuizStore.ts (Application Layer)
export const useQuizStore = create((set) => ({
  currentQuiz: null,
  score: 0,
  loadQuiz: async (articleId) => {
    const quiz = await quizService.generate(articleId);
    set({ currentQuiz: quiz, score: 0 });
  },
  submitAnswer: (answer) => {
    // Business logic here
    set((state) => ({ score: state.score + (answer.correct ? 1 : 0) }));
  },
}));

// components/QuizView.tsx (UI Layer)
function QuizView() {
  const { currentQuiz, score, submitAnswer } = useQuizStore();
  
  if (!currentQuiz) return <div>Loading...</div>;
  
  return (
    <div>
      <h2>Score: {score}</h2>
      <QuizQuestion 
        question={currentQuiz.questions[0]} 
        onSubmit={submitAnswer} 
      />
    </div>
  );
}
```

---

## Summary

**Frontend Architecture Checklist:**
- [ ] Components follow Single Responsibility Principle
- [ ] Business logic extracted into custom hooks
- [ ] UI components are "dumb" and declarative
- [ ] Composition used over inheritance
- [ ] Unidirectional data flow maintained
- [ ] Dependencies injected via props/context
- [ ] Clean Architecture layers respected
- [ ] Zustand used for global state management
- [ ] All theming rules followed (see above)

