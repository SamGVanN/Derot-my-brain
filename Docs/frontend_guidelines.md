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

