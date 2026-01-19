# Frontend Architecture & Guidelines

This document consolidates all frontend development guidelines, architectural principles, and technical patterns for the Derot My Brain project.

## Table of Contents

1. [Theming & Styling](#theming--styling)
2. [Architecture Principles](#architecture-principles)
3. [React Best Practices](#react-best-practices)
4. [State Management (Zustand)](#state-management-zustand)
5. [Routing & Navigation (React Router v7)](#routing--navigation-react-router-v7)

---

## Theming & Styling

Derot My Brain uses a dynamic theming system. **All new UI components and pages MUST adhere to the following rules:**

### 1. Semantic Colors

**NEVER use hardcoded colors** like `text-white`, `bg-black`, `bg-slate-900`, etc. for primary UI elements.
Instead, use the semantic classes provided by `tailwindcss` which map to our CSS variables:

- **background**: `bg-background` (Page background)
- **foreground**: `text-foreground` (Primary text color)
- **muted**: `text-muted-foreground` (Secondary/description text)
- **card**: `bg-card` (Card background)
- **card-foreground**: `text-card-foreground` (Card text)
- **border**: `border-border` (Borders)
- **primary**: `bg-primary`, `text-primary-foreground` (Primary actions/highlights)
- **secondary**: `bg-secondary`, `text-secondary-foreground` (Secondary actions)

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

- **Library**: Use **Lucide React** for all icons.
- **Rule**: Icons or containers inside interactive elements that adapt on hover MUST also use the `accent` color family to match their parent.
- **Example**: If a button turns `bg-accent` on hover, an icon inside it should stay visible or adapt using `group-hover:text-accent-foreground`. Avoid using `group-hover:text-primary` as this breaks the theme consistency.

---

## Architecture Principles

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
- `useUser()` - Current user data and operations
- `usePreferences()` - User preferences management
- `useCategories()` - Category management
- `useHistory()` - User history operations
- `useQuiz()` - Quiz state and operations (future)
- `useWikipedia()` - Wikipedia article fetching (future)

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
2. **Application Layer** (`/hooks`, `/stores`) - Application-specific logic, custom hooks, Zustand stores
3. **Domain Layer** (`/services`, `/models`) - Business logic, data models, API services
4. **Infrastructure Layer** (`/api`, `/utils`) - External dependencies, HTTP clients, utilities

**Dependency Rule:**
- Outer layers can depend on inner layers
- Inner layers should NOT depend on outer layers
- Use **dependency injection** via props or context

**Directory Structure:**
```
src/
├── components/        # UI Layer (presentation)
│   ├── ui/           # Shared UI components
│   └── features/     # Feature-specific components
├── pages/            # UI Layer (page components)
├── hooks/            # Application Layer (custom hooks)
├── stores/           # Application Layer (Zustand stores)
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

## React Best Practices

### 1. Keys

- **Rule**: NEVER use the array index as a key for dynamic lists (`key={index}`).
- **Why**: React uses keys to track element identity. Index keys cause state bugs and performance issues when lists are reordered or filtered.
- **Use**: Unique IDs from your data (e.g., `key={user.id}`).

### 2. Hooks Rules

- **Dependencies**: Always include ALL used variables in the dependency array of `useEffect`, `useMemo`, and `useCallback`. Do not "lie" to React to avoid re-renders—fix your logic instead.
- **Cleanup**: Always return a cleanup function in `useEffect` when creating listeners, subscriptions, or timers.

### 3. Performance

- **Referential Equality**:
  - Use `useMemo` for expensive calculations.
  - Use `useCallback` for functions passed to child components to ensure referential stability.
- **React.memo**: Wrap purely presentational components in `React.memo` *only* if they re-render frequently with the same props. Premature optimization adds overhead.

### 4. Component Patterns

- **Consistency**: Use `function ComponentName() {}` or const arrow functions, but **be consistent** across the project.
- **Destructuring**: Destructure props in the function signature for clarity: `const Card = ({ title, children }) => ...`.

---

## State Management (Zustand)

For this project, **Zustand** is the recommended state management solution.

### 1. Core Principles

- **Multiple Focused Stores**: Create small, domain-specific stores (e.g., `useAuthStore`, `usePreferencesStore`) rather than one giant root store.
- **Encapsulation**: **Do NOT export the store directly.** Export custom hooks that allow components to consume *specific* slices of state.

### 2. Advanced Best Practices

#### Encapsulation via Custom Hooks

**Rule**: Components should never know about the store implementation. They should just use a hook.
**Why**: This allows you to refactor state structure without breaking components and prevents components from accidentally accessing the raw store.

```tsx
// ❌ BAD: Exporting store directly
export const useStore = create((set) => ({ ... }));

// ✅ GOOD: Exporting domain-specific hooks
// internal definition (not exported)
const useBearStore = create((set) => ({
  bears: 0,
  actions: {
    increase: () => set((state) => ({ bears: state.bears + 1 })),
  },
}))

// public API
export const useBears = () => useBearStore((state) => state.bears)
export const useBearActions = () => useBearStore((state) => state.actions)
```

#### Atomic & Stable Selectors

**Rule**: Always select the smallest possible slice of state.
**Why**: Zustand strictly checks for equality (`oldState === newState`). If you return a new object every time, you will force re-renders.

```tsx
// ❌ BAD: Returns a new object every render -> Infinite re-renders
const { bears, increase } = useBearStore((state) => ({ 
  bears: state.bears, 
  increase: state.actions.increase 
}))

// ✅ GOOD: Atomic selectors (Primitive values)
const bears = useBearStore((state) => state.bears)

// ✅ GOOD: Stable object selector with useShallow
import { useShallow } from 'zustand/react/shallow'
const { bears, increase } = useBearStore(
  useShallow((state) => ({ bears: state.bears, increase: state.actions.increase }))
)
```

#### Model Actions as Events

**Rule**: Name actions based on the **event** that occurred, not just the setter.
**Why**: This separates the "what happened" from the "how state changes", making the business logic clearer.

```tsx
// ❌ BAD: Setter style
setAuthenticatedUser: (user) => set({ user, status: 'authed' })

// ✅ GOOD: Event style
onLoginSuccess: (user) => set({ user, status: 'authed' })
```

#### Separate Actions from State

**Rule**: Group actions in a separate object or clearly separated section within the store.

**Pattern**:
```tsx
const useStore = create((set) => ({
  // State
  count: 0,
  
  // Actions
  actions: {
    increment: () => set((state) => ({ count: state.count + 1 })),
    reset: () => set({ count: 0 }),
  },
}))

export const useCount = () => useStore((state) => state.count)
export const useCountActions = () => useStore((state) => state.actions)
```

#### Reset Pattern

**Pattern**: Use a defined initial state object to easily reset stores (e.g., on logout).

```tsx
const initialState = { count: 0, user: null }

const useStore = create((set) => ({
  ...initialState,
  actions: {
    reset: () => set(initialState),
  }
}))
```

#### DevTools

**Rule**: Always use the `devtools` middleware for debugging.

```tsx
import { devtools } from 'zustand/middleware'

const useStore = create(devtools((set) => ({ ... }), { name: 'MyStore' }))
```

### Integration with Architecture Principles

Zustand stores belong in the **Application Layer** (`/stores` or `/hooks`). They contain business logic and state, keeping UI components "dumb".

---

## Routing & Navigation (React Router v7)

This section outlines the routing and navigation patterns used in Derot My Brain with React Router v7.

### 1. Declarative Routing

**Use declarative `<Routes>` and `<Route>` components in `App.tsx`.**

```tsx
// ✅ GOOD: Declarative routing
<Routes>
  <Route path="/" element={<HomePage />} />
  <Route path="/profile" element={<ProfilePage />} />
  <Route path="/preferences" element={<PreferencesPage />} />
</Routes>
```

**Avoid**: Manual route management with state variables.

### 2. Protected Routes Pattern

**Use wrapper components for route protection.**

```tsx
// ✅ GOOD: Protected route wrapper
function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { user, isInitializing } = useAuth();
  const location = useLocation();

  if (isInitializing) {
    return <LoadingSpinner />;
  }

  if (!user) {
    return <Navigate to="/" state={{ from: location }} replace />;
  }

  return <>{children}</>;
}

// Usage
<Route 
  path="/profile" 
  element={
    <ProtectedRoute>
      <ProfilePage />
    </ProtectedRoute>
  } 
/>
```

**Route Guards Checklist:**

Every protected route must:
- ✅ Check authentication state
- ✅ Show loading state during initialization
- ✅ Redirect unauthenticated users to login
- ✅ Preserve intended destination in location state
- ✅ Handle edge cases (expired sessions, etc.)

### 3. Navigation Links

#### NavLink for Menu Items

**Use `NavLink` for navigation menu items to get automatic active state.**

```tsx
// ✅ GOOD: NavLink with active state
<NavLink
  to="/profile"
  className={({ isActive }) =>
    cn(
      "flex items-center gap-3 px-3 py-2 rounded-lg",
      "hover:bg-accent hover:text-accent-foreground",
      isActive && "bg-accent text-accent-foreground font-medium"
    )
  }
>
  <User className="h-5 w-5" />
  <span>{t('nav.profile')}</span>
</NavLink>
```

#### Link for Simple Navigation

**Use `Link` for simple navigation without active state:**

```tsx
<Link to="/history">Back to History</Link>
```

### 4. Programmatic Navigation

**Use `useNavigate()` hook for programmatic navigation.**

```tsx
// ✅ GOOD: Programmatic navigation
const navigate = useNavigate();

const handleLogout = () => {
  logout();
  navigate('/');
};

const handleCancel = () => {
  navigate(-1); // Go back
};

const handleSave = () => {
  // Navigate with state
  navigate('/profile', { state: { saved: true } });
};
```

**Access navigation state in target component:**

```tsx
const location = useLocation();
const saved = location.state?.saved;
```

### 5. Route Organization

**Organize routes by access level and feature:**

```tsx
<Routes>
  {/* Public Routes */}
  <Route path="/" element={user ? <Navigate to="/history" /> : <LoginPage />} />
  <Route path="/guide" element={<GuidePage />} />

  {/* Protected Routes */}
  <Route path="/history" element={<ProtectedRoute><HistoryPage /></ProtectedRoute>} />
  <Route path="/profile" element={<ProtectedRoute><ProfilePage /></ProtectedRoute>} />
  <Route path="/preferences" element={<ProtectedRoute><PreferencesPage /></ProtectedRoute>} />
  <Route path="/derot" element={<ProtectedRoute><DerotPage /></ProtectedRoute>} />
  <Route path="/tracked-topics" element={<ProtectedRoute><TrackedTopicsPage /></ProtectedRoute>} />

  {/* Fallback */}
  <Route path="*" element={<Navigate to="/" replace />} />
</Routes>
```

### 6. URL Structure Best Practices

#### Use Clear, Semantic URLs

- ✅ `/history` - User's activity history
- ✅ `/profile` - User profile page
- ✅ `/preferences` - User settings
- ✅ `/tracked-topics` - User's tracked topics
- ✅ `/derot` - Main Derot page
- ✅ `/guide` - Help guide
- ❌ `/page1`, `/view2` - Non-descriptive URLs

#### Use Kebab-Case for Multi-Word Routes

- ✅ `/tracked-topics`
- ✅ `/user-profile`
- ❌ `/trackedTopics`, `/tracked_topics`

#### Home Route Behavior

**The home route (`/`) should redirect based on authentication:**

```tsx
<Route 
  path="/" 
  element={user ? <Navigate to="/history" replace /> : <UserSelectionPage />} 
/>
```

- **Unauthenticated**: Show login/user selection
- **Authenticated**: Redirect to `/history` (user's home)

### 7. Navigation Component Patterns

#### Header Component

**The header must adapt to authentication state:**

```tsx
export function Header() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  return (
    <header>
      {/* Logo - clickable, navigates to home */}
      <Link to={user ? "/history" : "/"}>
        <Logo />
      </Link>

      {/* Right side - Authentication state */}
      {!user ? (
        // Unauthenticated: Show language + theme selectors
        <>
          <LanguageSelector />
          <ThemeSelector />
        </>
      ) : (
        // Authenticated: Show settings + user menu + logout
        <>
          <Button onClick={() => navigate('/preferences')}>
            <Settings />
          </Button>
          <UserMenuDropdown />
          <Button onClick={logout}>
            <LogOut />
          </Button>
        </>
      )}
    </header>
  );
}
```

#### Navigation Menu (Responsive)

**Desktop: Fixed sidebar, Mobile: Hamburger menu**

```tsx
export function NavigationMenu() {
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  return (
    <>
      {/* Mobile Hamburger Button */}
      <Button 
        className="md:hidden fixed top-20 left-4 z-50" 
        onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
      >
        {isMobileMenuOpen ? <X /> : <Menu />}
      </Button>

      {/* Mobile Overlay */}
      {isMobileMenuOpen && (
        <div 
          className="md:hidden fixed inset-0 bg-background/80 backdrop-blur-sm z-40"
          onClick={() => setIsMobileMenuOpen(false)}
        />
      )}

      {/* Navigation Sidebar */}
      <nav className={cn(
        "fixed left-0 top-16 h-[calc(100vh-4rem)] w-64 bg-card border-r z-40",
        "transition-transform duration-300 ease-in-out",
        "md:translate-x-0",
        isMobileMenuOpen ? "translate-x-0" : "-translate-x-full"
      )}>
        {/* Navigation links */}
        {navigationItems.map((item) => (
          <NavLink
            key={item.path}
            to={item.path}
            onClick={() => setIsMobileMenuOpen(false)}
            className={({ isActive }) =>
              cn(
                "flex items-center gap-3 px-3 py-2 rounded-lg",
                "hover:bg-accent hover:text-accent-foreground",
                isActive && "bg-accent text-accent-foreground font-medium"
              )
            }
          >
            <item.icon className="h-5 w-5" />
            <span>{t(item.labelKey)}</span>
          </NavLink>
        ))}
      </nav>

      {/* Desktop spacer */}
      <div className="hidden md:block w-64 flex-shrink-0" />
    </>
  );
}
```

#### Active Page Highlighting

**Always highlight the current page in navigation:**

```tsx
<NavLink
  to="/profile"
  className={({ isActive }) =>
    cn(
      "base-styles",
      isActive && "bg-accent text-accent-foreground font-medium"
    )
  }
>
  Profile
</NavLink>
```

**Key points:**
- Use `NavLink` instead of `Link` for menu items
- Use `isActive` prop from className function
- Apply `bg-accent` for active state (not `bg-primary`)
- Add `font-medium` for visual emphasis

### 8. Testing Navigation

**Always test navigation flows in both authenticated and unauthenticated states:**

1. **Unauthenticated User:**
   - ✅ Can access public routes (/, /guide)
   - ✅ Redirected to / when accessing protected routes
   - ✅ Header shows language/theme selectors

2. **Authenticated User:**
   - ✅ Redirected to /history when accessing /
   - ✅ Can access all protected routes
   - ✅ Header shows user menu, settings, logout
   - ✅ Active page highlighted in navigation
   - ✅ Logout redirects to /

3. **Mobile Responsiveness:**
   - ✅ Hamburger menu appears on mobile
   - ✅ Overlay closes when clicking outside
   - ✅ Navigation links close menu on click

---

## Summary Checklist

**Frontend Architecture Checklist:**
- [ ] Components follow Single Responsibility Principle
- [ ] Business logic extracted into custom hooks
- [ ] UI components are "dumb" and declarative
- [ ] Composition used over inheritance
- [ ] Unidirectional data flow maintained
- [ ] Dependencies injected via props/context
- [ ] Clean Architecture layers respected
- [ ] Zustand used for global state management
- [ ] All theming rules followed
- [ ] React Router v7 patterns followed
- [ ] Protected routes implemented correctly
- [ ] Navigation tested in all states

---

## Related Documentation

- [Storage-Policy.md](file:///d:/Repos/Derot-my-brain/Docs/Technical/Storage-Policy.md) - Backend storage constraints
- [Backend-Guidelines.md](file:///d:/Repos/Derot-my-brain/Docs/Technical/Backend-Guidelines.md) - Backend architecture patterns
- [Testing-Strategy.md](file:///d:/Repos/Derot-my-brain/Docs/Technical/Testing-Strategy.md) - Testing approach and standards
- [Color-Palettes.md](file:///d:/Repos/Derot-my-brain/Docs/Reference/Color-Palettes.md) - Available theme color palettes
