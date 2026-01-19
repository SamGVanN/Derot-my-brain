# React Router v7 - Routing and Navigation Guidelines

This document outlines the routing and navigation patterns used in Derot My Brain with React Router v7.

---

## Table of Contents

1. [Declarative Routing](#declarative-routing)
2. [Protected Routes Pattern](#protected-routes-pattern)
3. [Navigation Links](#navigation-links)
4. [Programmatic Navigation](#programmatic-navigation)
5. [Route Organization](#route-organization)
6. [URL Structure Best Practices](#url-structure-best-practices)
7. [Navigation Component Patterns](#navigation-component-patterns)

---

## Declarative Routing

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

---

## Protected Routes Pattern

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

---

## Navigation Links

### NavLink for Menu Items

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

### Link for Simple Navigation

**Use `Link` for simple navigation without active state:**

```tsx
<Link to="/history">Back to History</Link>
```

---

## Programmatic Navigation

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

---

## Route Organization

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

---

## URL Structure Best Practices

### Use Clear, Semantic URLs

- ✅ `/history` - User's activity history
- ✅ `/profile` - User profile page
- ✅ `/preferences` - User settings
- ✅ `/tracked-topics` - User's tracked topics
- ✅ `/derot` - Main Derot page
- ✅ `/guide` - Help guide
- ❌ `/page1`, `/view2` - Non-descriptive URLs

### Use Kebab-Case for Multi-Word Routes

- ✅ `/tracked-topics`
- ✅ `/user-profile`
- ❌ `/trackedTopics`, `/tracked_topics`

### Home Route Behavior

**The home route (`/`) should redirect based on authentication:**

```tsx
<Route 
  path="/" 
  element={user ? <Navigate to="/history" replace /> : <UserSelectionPage />} 
/>
```

- **Unauthenticated**: Show login/user selection
- **Authenticated**: Redirect to `/history` (user's home)

---

## Navigation Component Patterns

### Header Component

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

### Navigation Menu (Responsive)

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

### Active Page Highlighting

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

---

## Nested Routes (Future)

**When implementing nested routes, use `<Outlet />` in parent:**

```tsx
// Parent route component
function DashboardLayout() {
  return (
    <div>
      <Sidebar />
      <main>
        <Outlet /> {/* Child routes render here */}
      </main>
    </div>
  );
}

// Route configuration
<Route path="/dashboard" element={<DashboardLayout />}>
  <Route index element={<DashboardHome />} />
  <Route path="stats" element={<StatsPage />} />
  <Route path="settings" element={<SettingsPage />} />
</Route>
```

---

## Testing Navigation

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

- ✅ Use React Router v7 for all routing
- ✅ Protected routes via wrapper components
- ✅ `NavLink` for menu items with active state
- ✅ `useNavigate()` for programmatic navigation
- ✅ Semantic URLs with kebab-case
- ✅ Responsive navigation (sidebar + hamburger)
- ✅ Authentication-aware UI in header
- ✅ Always test navigation flows
- ✅ Home route redirects based on auth state
- ✅ Fallback route for 404s
