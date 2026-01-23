# Frontend Coding Standards

**Role**: Guidelines for writing clean, consistent, and maintainable React code.

## 1. Theming & Semantic Colors

**Constraint**: **NEVER** use hardcoded colors. This breaks Dark Mode.

### **Color Mapping**
| Concept | Tailwind Class | Usage |
| :--- | :--- | :--- |
| **Page Background** | `bg-background` | Main containers |
| **Primary Text** | `text-foreground` | Standard text |
| **Secondary Text** | `text-muted-foreground` | Descriptions, subtitles |
| **Card Background** | `bg-card` | Cards, Panels |
| **Borders** | `border-border` | Dividers, Inputs |
| **Primary Action** | `bg-primary`, `text-primary-foreground` | Main CTA Buttons |
| **Hover/Interactive**| `hover:bg-accent`, `hover:text-accent-foreground` | Dropdowns, List items |

**Rule**: Always test components in both **Light** and **Dark** modes.

---

## 2. Component Patterns

### **Composition > Inheritance**
*   **Rule**: Use `children` prop and slots (`header`, `footer`) instead of massive config objects.
*   **Example**: `<Card header={<Title />}>{content}</Card>`

### **One Component = One File**
*   **Rule**: Keep files small (max ~150 lines). Break huge pages into `components/features/`.

### **Props Destructuring**
*   **Style**:
    ```tsx
    // ✅ GOOD
    const MyComponent = ({ title, isActive }: Props) => { ... }
    
    // ❌ BAD
    const MyComponent = (props: Props) => { ... props.title ... }
    ```

---

## 3. React Best Practices

### **Hooks Rules**
*   **Dependency Arrays**: NEVER lie to React. If you use a variable in `useEffect`, put it in the dependency array. If that causes infinite loops, **fix your logic**, don't remove the dependency.
*   **Memoization**: Use `useMemo` for heavy computations. Use `useCallback` for functions passed as props to optimized children.

### **Keys in Lists**
*   **Rule**: NEVER use array index as `key`.
*   **Constraint**: Always use unique IDs from the data (e.g., `user.id`).
