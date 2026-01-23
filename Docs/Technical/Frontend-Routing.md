# Frontend Routing & Navigation

**Role**: Managing URL-based navigation and access control using **React Router v7**.

## 1. Declarative Routing

**Rule**: Define all routes centrally in `App.tsx` (or a dedicated `AppRoutes.tsx`).

```tsx
<Routes>
  <Route path="/" element={<Home />} />
  <Route path="/profile" element={<ProtectedRoute><Profile /></ProtectedRoute>} />
</Routes>
```

**Constraint**: Avoid imperative routing (logic inside components) to define structure. Use `<Navigate>` or wrapper components for logic.

## 2. Protected Routes

**Pattern**: High-Order Component wrapping.

```tsx
function ProtectedRoute({ children }) {
  const { user, isLoading } = useAuth();
  
  if (isLoading) return <Spinner />;
  if (!user) return <Navigate to="/" replace />;
  
  return children;
}
```

*   **Requirement**: Always handle the `isLoading` state (e.g., checking session on refresh) before redirecting, otherwise users get kicked out on F5.

## 3. Navigation Components

### **Menu Items**
*   **Component**: Use `NavLink`.
*   **Feature**: Use the `isActive` render prop to highlight the current menu item.
*   **Style**: Active items should use `bg-accent` (not primary).

### **Programmatic Navigation**
*   **Hook**: `useNavigate()`.
*   **Usage**: Only for event handlers (e.g., after Form Submit). For links, always use `<Link>` or `<NavLink>`.

## 4. URL Guidelines

*   **Format**: `kebab-case` (e.g., `/user-profile`, not `/userProfile`).
*   **Semantics**: URLs should be descriptive (`/history`, `/settings`).
