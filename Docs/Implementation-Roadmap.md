# Implementation Roadmap

This document breaks down the feature bucket list into delegatable implementation tasks. Each task is designed to be handled independently in a new context/prompt.

---

## ⚠️ Technical Constraints & Storage Policy

### Storage Requirements (CRITICAL)

**For POC/V1: JSON Files ONLY - No SQL Database**

- ✅ **Allowed:** JSON files stored locally
- ❌ **Not Allowed:** SQL Server, PostgreSQL, MySQL, or any external database requiring installation/connection string
- ✅ **Reason:** Application must be self-contained, deployable locally, and work offline without external dependencies

### Data Storage Structure

All data must be stored as JSON files in the `/data/` directory:

```
/data/
├── seed/                    # Immutable reference data
│   ├── categories.json      # Wikipedia categories
│   └── themes.json          # Color themes
├── config/                  # Global configuration
│   └── app-config.json      # LLM URL, global settings
└── users/                   # User-specific data
    ├── users.json           # User profiles and preferences
    ├── user-{id}-history.json    # Activity history per user
    └── user-{id}-tracked.json    # Tracked topics (favorites) per user
```

### Acceptable Alternatives (If Complexity Requires)

If an AI agent detects that JSON files become insufficient, the following alternatives are acceptable:

1. **SQLite** ✅
   - Embedded database (single file)
   - No installation required
   - Works offline
   - Managed by application autonomously
   - File: `/data/derot-my-brain.db`

2. **LiteDB** ✅ (Recommended for .NET)
   - NoSQL embedded database
   - Single DLL, no installation
   - Works offline
   - .NET native
   - File: `/data/derot-my-brain.litedb`

3. **RavenDB Embedded** ✅
   - NoSQL embedded database
   - No installation required
   - Works offline
   - File-based storage

### Not Acceptable Alternatives

❌ **SQL Server** - Requires installation or connection string  
❌ **PostgreSQL** - Requires external server  
❌ **MySQL/MariaDB** - Requires external server  
❌ **MongoDB** - Requires external server (unless embedded mode)  
❌ **Any cloud database** - Requires internet connection

### Migration Path

If migration from JSON to embedded DB is needed:
1. Create migration script to convert JSON → SQLite/LiteDB
2. Maintain backward compatibility (read old JSON files)
3. Provide migration tool for users
4. Document migration process

### For All Tasks

**Default assumption:** Use JSON files unless explicitly stated otherwise.

**When proposing alternatives:** Only suggest SQLite, LiteDB, or RavenDB Embedded.

---

## 🧪 Development Methodology Requirements (CRITICAL)

### Test-Driven Development (TDD)

**All new features MUST be implemented using TDD:**

1. **Write Tests First:**
   - Write unit tests BEFORE implementing the feature
   - Define expected behavior through tests
   - Tests should fail initially (Red phase)

2. **Implement Feature:**
   - Write minimal code to make tests pass (Green phase)
   - Focus on functionality, not optimization

3. **Refactor:**
   - Clean up code while keeping tests green
   - Improve design and maintainability

**Testing Requirements:**
- **Backend:** Unit tests for all services, integration tests for API endpoints
- **Frontend:** Component tests for complex UI components
- **Coverage:** Minimum 80% code coverage for new features
- **Test Framework:** 
  - Backend: xUnit or NUnit
  - Frontend: Vitest + React Testing Library

### Mock Data for TestUser (CRITICAL)

**Every new feature MUST include mock data for the user "TestUser":**

- **User ID:** Use existing TestUser ID from `/data/users/users.json`
- **Purpose:** Automated testing, demonstrations, and development
- **Requirements:**
  - Create realistic, representative mock data
  - Cover edge cases (empty states, maximum values, etc.)
  - Include in seed data or test fixtures
  - Document mock data structure

**Mock Data Examples:**
```json
// For new activity features
{
  "userId": "testuser-id",
  "topic": "Quantum Mechanics",
  "score": 18,
  "totalQuestions": 20,
  "date": "2026-01-18T14:30:00Z"
}

// For new preference features
{
  "userId": "testuser-id",
  "newPreference": "default-value",
  "updatedAt": "2026-01-18T14:30:00Z"
}
```

**TestUser Data Location:**
- User profile: `/data/users/users.json`
- Activity history: `/data/users/user-{testuser-id}-history.json`
- Tracked Topics: `/data/users/user-{testuser-id}-tracked.json`

### For All Tasks

**Mandatory for each task:**
1. ✅ Write tests first (TDD)
2. ✅ Create TestUser mock data
3. ✅ Verify tests pass with mock data
4. ✅ Document test scenarios in acceptance criteria

**Acceptance Criteria Template:**
- [ ] Unit tests written and passing
- [ ] Integration tests written and passing (if applicable)
- [ ] TestUser mock data created and documented
- [ ] Code coverage ≥ 80%
- [ ] All tests pass in CI/CD pipeline

---

## Phase -1: Frontend Architecture Migration (CRITICAL)

> [!CAUTION]
> **CRITICAL PRIORITY**: This phase MUST be completed BEFORE Phase 0 and all other features. The current frontend codebase violates the architecture principles defined in `Docs/frontend_guidelines.md`. Implementing new features on top of this foundation will lead to technical debt and maintainability issues.

**See detailed implementation plan:** `C:\Users\samue\.gemini\antigravity\brain\c4c2af8b-5dba-4a61-ad75-7b261354d2cc\implementation_plan.md`

### Task -1.1: Infrastructure Layer Setup
**Priority:** CRITICAL  
**Estimated Complexity:** Medium  
**Dependencies:** None

#### Objective
Establish proper Infrastructure Layer with centralized HTTP client and API organization.

#### Current Issues
- UserService mixes `axios` and `fetch`
- Hardcoded API URLs throughout codebase
- No centralized error handling
- Services in wrong directory (`/services` instead of `/api`)

#### Specifications
1. Create `/api` directory structure (client.ts, endpoints.ts, userApi.ts, categoryApi.ts)
2. Implement centralized axios instance with base URL from environment
3. Move and refactor `UserService.ts` → `userApi.ts`
4. Remove axios/fetch mixing

#### Acceptance Criteria
- [x] `/api` directory created with proper structure
- [x] Centralized HTTP client implemented
- [x] All API calls use centralized client
- [x] No hardcoded API URLs in components

---

### Task -1.2: Zustand State Management Setup
**Priority:** CRITICAL  
**Estimated Complexity:** Medium  
**Dependencies:** Task -1.1

#### Objective
Implement Zustand stores for global state management (authentication and preferences).

#### Current Issues
- No global state management
- State scattered across components
- Direct localStorage access in components

#### Specifications
1. Install Zustand: `npm install zustand`
2. Create `stores/useAuthStore.ts` with authentication state and persistence
3. Create `stores/usePreferencesStore.ts` with user preferences state
4. Implement Zustand middleware for localStorage persistence

#### Acceptance Criteria
- [x] Zustand installed and configured
- [x] `useAuthStore` implemented with persistence
- [x] `usePreferencesStore` implemented
- [x] No direct localStorage access in components

---

### Task -1.3: Custom Hooks Implementation
**Priority:** CRITICAL  
**Estimated Complexity:** High  
**Dependencies:** Task -1.2

#### Objective
Extract all business logic from components into custom hooks.

#### Current Issues
- Business logic embedded in components
- Direct API calls in components (App.tsx, UserPreferencesPage, history-view)
- Components are too large (App.tsx: 166 lines, UserPreferencesPage: 487 lines)

#### Specifications
Create the following custom hooks:
1. `hooks/useAuth.ts` - Authentication logic (wraps useAuthStore)
2. `hooks/useUser.ts` - User operations
3. `hooks/usePreferences.ts` - Preferences management (wraps usePreferencesStore)
4. `hooks/useHistory.ts` - History operations
5. Enhance `hooks/useCategories.ts` - Use centralized API client

#### Acceptance Criteria
- [x] All 5 custom hooks implemented
- [x] Hooks follow Single Responsibility Principle
- [x] Business logic extracted from components
- [x] No direct API calls in components

---

### Task -1.4: Component Refactoring & Architecture Alignment
**Priority:** CRITICAL  
**Estimated Complexity:** High  
**Dependencies:** Task -1.3

#### Objective
Refactor key frontend components to strictly adhere to `frontend_guidelines.md`, specifically ensuring "Dumb Components", "Separation of Concerns", and "Custom Hooks" usage.

#### Components to Refactor

1. **`App.tsx`** (Currently ~193 lines)
   - **Goal:** Simplify to pure routing/provider setup (< 100 lines).
   - **Action Items:**
     - Move session validation logic (`validateSession`) into `hooks/useAuth.ts` (e.g., `useAuth().initializeSession()`).
     - Remove direct imports of `useAuthStore` and `usePreferencesStore`. Use `useAuth()` and `usePreferences()` hooks instead.
     - Remove `UserService` import.
     - Remove manual `welcomeHiddenSession` state if possible or move to a UI-only wrapper.
     - Fix pure type casting (`as any`).

2. **`pages/UserPreferencesPage.tsx`** (Currently ~486 lines)
   - **Goal:** Split into focused sub-components.
   - **Action Items:**
     - Create `components/preferences/GeneralPreferencesForm.tsx` handling Language, Theme, Question Count.
     - Create `components/preferences/CategoryPreferencesForm.tsx` handling Category selection.
     - Replace manual dropdowns with `@/components/ui` components (`Select`, etc.).
     - Remove direct `UserService` and `categoryApi` calls. Use `usePreferences()` and `useCategories()` hooks.
     - Ensure the main page component only orchestrates these forms and layout.

3. **`pages/UserSelectionPage.tsx`**
   - **Goal:** Remove business logic.
   - **Action Items:**
     - Replace `UserService.getAllUsers()` with `useUser().users` (or `useAuth().availableUsers`).
     - Remove `UserService.login()` calls; use `useAuth().login()`.

4. **`components/history-view.tsx`**
   - **Goal:** make it a pure presentational component.
   - **Action Items:**
     - Remove `useEffect` and `UserService.getHistory`.
     - Accept data via props OR use `useHistory()` hook inside.
     - Handle `loading` and `error` states from the hook.

#### Technical Constraints
- **Strict Separation:** UI Components MUST NOT import from `services/*` or `api/*`. They MUST import `hooks/*`.
- **UI Components:** Use `shadcn/ui` components where available.
- **Styling:** Use Tailwind CSS utility classes (no hardcoded colors).

#### Acceptance Criteria
- [x] `App.tsx` has no direct references to stores or services.
- [x] `UserPreferencesPage.tsx` is under 150 lines (orchestration only).
- [x] `GeneralPreferencesForm.tsx` and `CategoryPreferencesForm.tsx` created.
- [x] `history-view.tsx` uses `useHistory` hook.
- [x] No component imports `UserService` directly.
- [x] Application compiles and runs with identical functionality.

---

### Task -1.5: Context Cleanup & Verification
**Priority:** HIGH  
**Estimated Complexity:** Low  
**Dependencies:** Task -1.4

#### Objective
Clean up redundant contexts and verify all architecture principles are followed.

#### Specifications
1. Evaluate `contexts/UserContext.tsx` - may be redundant with Zustand stores
2. Run full verification checklist (see implementation plan)
3. Manual testing of all features
4. Update documentation

#### Acceptance Criteria
- [x] UserContext simplified or removed
- [x] All verification checklist items pass
- [x] All manual tests pass
- [x] No regression in existing functionality

---

## Phase 0: Application Foundation & Initialization

**⚠️ MUST BE DONE AFTER PHASE -1 (Architecture Migration)**

### Task 0.1: Application Initialization & Configuration
**Priority:** CRITICAL  
**Estimated Complexity:** Medium  
**Dependencies:** None (must be done first)

#### Objective
Set up application initialization with seed data (categories, themes) and global configuration management (LLM URL).

#### Specifications
- **Backend:**
  - **Seed Data (Immutable - JSON Files):**
    - Create `SeedDataService` to initialize:
      - **Wikipedia Categories** (13 official categories)
      - **Themes** (5 color palettes)
    - Store as JSON files in `/data/seed/`
    - Run initialization on first application startup
    - Idempotent: can run multiple times without duplicating data
  
  - **Wikipedia Categories Seed:**
    ```csharp
    public class WikipediaCategory
    {
        public string Id { get; set; } // e.g., "culture-arts"
        public string Name { get; set; } // e.g., "Culture and the arts"
        public string NameFr { get; set; } // French translation
        public int Order { get; set; } // Display order
        public bool IsActive { get; set; } = true;
    }
    ```
    
    Categories to seed:
    1. general-reference → "General reference" / "Référence générale"
    2. culture-arts → "Culture and the arts" / "Culture et arts"
    3. geography-places → "Geography and places" / "Géographie et lieux"
    4. health-fitness → "Health and fitness" / "Santé et forme"
    5. history-events → "History and events" / "Histoire et événements"
    6. human-activities → "Human activities" / "Activités humaines"
    7. mathematics-logic → "Mathematics and logic" / "Mathématiques et logique"
    8. natural-sciences → "Natural and physical sciences" / "Sciences naturelles"
    9. people-self → "People and self" / "Personnes et soi"
    10. philosophy-thinking → "Philosophy and thinking" / "Philosophie et pensée"
    11. religion-belief → "Religion and belief systems" / "Religion et croyances"
    12. society-sciences → "Society and social sciences" / "Société et sciences sociales"
    13. technology-sciences → "Technology and applied sciences" / "Technologie et sciences appliquées"
  
  - **Themes Seed:**
    ```csharp
    public class Theme
    {
        public string Id { get; set; } // e.g., "derot-brain"
        public string Name { get; set; } // e.g., "Derot Brain"
        public string Description { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; } = true;
    }
    ```
    
    Themes to seed:
    1. curiosity-loop → "Curiosity Loop" (Dark/Blue)
    2. derot-brain → "Derot Brain" (Dark/Violet) - **Default**
    3. knowledge-core → "Knowledge Core" (Dark/Cyan)
    4. mind-lab → "Mind Lab" (Dark/Teal)
    5. neo-wikipedia → "Neo-Wikipedia" (Light/Blue)
  
  - **Global Configuration:**
    - Create `AppConfiguration` model:
      ```csharp
      public class AppConfiguration
      {
          public string Id { get; set; } = "global";
          public LLMConfiguration LLM { get; set; }
          public DateTime LastUpdated { get; set; }
      }
      
      public class LLMConfiguration
      {
          public string Url { get; set; } // e.g., "http://localhost:11434"
          public int Port { get; set; } = 11434;
          public string Provider { get; set; } = "ollama"; // "ollama", "anythingllm", "openai"
          public string DefaultModel { get; set; } = "llama3:8b";
          public int TimeoutSeconds { get; set; } = 30;
      }
      ```
    
    - Store as JSON file in `/data/config/app-config.json`
    - Default configuration created on first startup
    - Add endpoints:
      - `GET /api/config` - Get global configuration
      - `PUT /api/config` - Update global configuration (admin only)
      - `GET /api/config/llm` - Get LLM configuration
      - `PUT /api/config/llm` - Update LLM configuration
  
  - **Initialization Flow:**
    1. Check if seed data exists
    2. If not, run seed data initialization
    3. Check if global config exists
    4. If not, create default configuration
    5. Log initialization status

- **Frontend:**
  - Create `useAppConfig` hook to fetch global configuration
  - Create admin page (optional for V1) to update LLM URL
  - Display LLM connection status in settings/about page
  - Handle LLM connection errors gracefully

- **Data Storage:**
  - Seed data: `/data/seed/categories.json` and `/data/seed/themes.json`
  - Global config: `/data/config/app-config.json`
  - User data: `/data/users/` (see Technical Constraints section for structure)

#### Acceptance Criteria
- [x] Seed data (categories + themes) initialized on first startup
- [x] 13 Wikipedia categories available via API
- [x] 5 themes available via API
- [x] Global configuration created with default LLM URL
- [ ] LLM configuration can be retrieved via API
- [ ] LLM configuration can be updated via API
- [x] Initialization is idempotent (can run multiple times safely)
- [x] Seed data is immutable (cannot be deleted/modified by users)
- [ ] Frontend can fetch and display LLM configuration
- [x] Error handling for missing/corrupted seed data

---

## Phase 1: Core Infrastructure & User Experience Enhancements

### Task 1.1: Session Persistence & Authentication State
**Priority:** HIGH  
**Estimated Complexity:** Medium  
**Dependencies:** None

#### Objective
Implement session persistence so that refreshing a page keeps the user authenticated instead of redirecting to the root/login page.

#### Specifications
- **Frontend:**
  - Store authenticated user information in `localStorage` or `sessionStorage`
  - Create a session management service/hook that:
    - Persists user ID and name on login
    - Restores user session on app initialization
    - Clears session on explicit logout
  - Update routing logic to check for existing session before redirecting to login
  - Add session validation (check if user still exists in backend)

- **Backend:**
  - Add endpoint: `GET /api/users/{id}` to validate user existence
  - Return 404 if user not found (session invalidation)

#### Acceptance Criteria
- [x] User remains logged in after page refresh
- [x] Invalid/deleted users are redirected to login
- [x] Session persists across browser tabs
- [x] Explicit logout clears session properly

---

### Task 1.2: Welcome Page for New Users
**Priority:** HIGH  
**Estimated Complexity:** Low  
**Dependencies:** Task 1.1 (session persistence)

#### Objective
Implement a welcome page that appears for first-time users, explaining the app's purpose and main features.

#### Specifications
- **Frontend:**
  - Create `WelcomePage.tsx` component with three options:
    1. "Read the Guide" - Shows detailed guide
    2. "Proceed to App" - Skip to main app
    3. "Don't Show Again" - Proceed and never show welcome page
  - Store user preference in `localStorage` with key `hasSeenWelcome`
  - Create `GuideModal.tsx` or `GuidePage.tsx` with:
    - App purpose explanation
    - Main features overview (Wikipedia reading, quiz generation, history, backlog)
    - User-friendly "for dummies" language
    - Visual aids (icons, screenshots if available)
  - Add navigation logic to show welcome page on first visit

- **Design:**
  - Follow existing theme system
  - Use engaging visuals and clear typography
  - Make guide scannable with headers and bullet points

#### Acceptance Criteria
- [x] New users see welcome page on first visit
- [x] "Don't Show Again" option persists preference
- [x] Guide is clear, concise, and beginner-friendly
- [x] Welcome page respects current theme
- [ ] Users can access guide later from settings/help menu

---

### Task 1.3: Backend Logging System
**Priority:** HIGH  
**Estimated Complexity:** Low  
**Dependencies:** None

#### Objective
Implement a robust logging system to capture backend errors and operation logs in files, replacing the ephemeral terminal output.

#### Specifications
- **Backend:**
  - Install and configure a logging library (recommended: **Serilog** or **NLog**).
  - Configure logging to write to files in a `/Logs` directory at the root of the API project (e.g., `DerotMyBrain.API/Logs/`).
  - **Constraints:**
    - Logs must be persisted to files.
    - Directory must be `/Logs` at the API root.
    - Log rotation (e.g., daily) should be enabled.
    - Capture all unhandled exceptions and critical application events.

#### Acceptance Criteria
- [x] Logging library installed (Serilog/NLog)
- [x] Logs are written to `/Logs` directory
- [x] Log files rotate daily
- [x] Error logs capture stack traces and context
- [x] INFO level logs capture startup and key events

---

## Phase 2: User Preferences & i18n

### Task 2.1: Extend User Model with Preferences
**Priority:** HIGH  
**Estimated Complexity:** Medium  
**Dependencies:** None

#### Objective
Extend the User model to include user preferences and metadata (quiz question count, last connection date).

#### Specifications
- **Backend:**
  - Update `User.cs` model:
    ```csharp
    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastConnectionAt { get; set; }
        public UserPreferences Preferences { get; set; }
    }
    
    public class UserPreferences
    {
        public int QuestionCount { get; set; } = 10; // Default: 10
        public string PreferredTheme { get; set; } = "derot-brain";
        // Future: language, difficulty, etc.
    }
    ```
  - Update `UserService.cs` to:
    - Update `LastConnectionAt` on each login
    - Provide methods to get/update preferences
  - Add endpoints:
    - `GET /api/users/{id}/preferences`
    - `PUT /api/users/{id}/preferences`
  - Migrate existing user data to include new fields

- **Frontend:**
  - Create `UserPreferences` TypeScript interface matching backend
  - Update user context/state to include preferences
  - Update login flow to fetch user preferences

#### Acceptance Criteria
- [x] User model includes preferences and last connection date
- [x] Last connection updates on each login
- [x] Preferences can be retrieved and updated via API
- [x] Existing users migrated with default preferences
- [x] Frontend state includes user preferences

---

### Task 2.2: User Preferences Page
**Priority:** MEDIUM  
**Estimated Complexity:** Low  
**Dependencies:** Task 2.1

#### Objective
Create a dedicated user preferences page where users can configure their settings.

#### Specifications
- **Frontend:**
  - Create `UserPreferencesPage.tsx` with:
    - **Quiz Settings:**
      - Question count selector: Radio buttons or dropdown for 5/10/15/20 (initially 10 on user creation)
      - Visual indication of current selection
    - **Theme Settings:**
      - Theme selector (reuse existing `ThemeSelector` component)
    - **Future Settings Placeholder:**
      - Section for difficulty, language, etc.
  - Add "Save" and "Cancel" buttons
  - Show success/error notifications on save
  - Add "My Brain" (Historique + Tracked Topics) navigation link in main menu

- **API Integration:**
  - Call `PUT /api/users/{id}/preferences` on save
  - Optimistic UI updates with rollback on error

#### Acceptance Criteria
- [x] Users can select question count (5/10/15/20)
- [x] Preferences save successfully to backend
- [x] UI updates immediately on save
- [x] Error handling for failed saves
- [x] Page accessible from navigation menu

---

### Task 2.4: Contextual Preference Initialization & Loading
**Priority:** HIGH
**Estimated Complexity:** Medium
**Dependencies:** Task 2.1, Task 2.2

#### Objective
Ensure that user preferences (Language, Theme) are immediately applied upon login and correctly captured from the current environment upon user creation.

#### Specifications
- **Frontend (Login Flow):**
  - When `login` occurs:
    - Retrieve full user object including `Preferences`.
    - **Immediately** update the global application state (Language, Theme) to match the user's stored preferences.
    - Ensure no "flash" of default theme/language occurs if possible (optimistic or loading state).

- **Frontend (User Creation Flow):**
  - When creating a new user:
    - Capture the **current** active Language and Theme (which the user might have set on the Welcome/Landing page).
    - Pass these values to the `CreateUser` payload (or update immediately after creation).
    - Ensure the new user's initial stored preferences match what they were seeing when they clicked "Create".

#### Acceptance Criteria
- [x] Login immediately switches theme/language to user's saved preference
- [x] Creating a new user saves the currently active theme/language as their default
- [x] Verified manual test: Change theme on landing page -> Create User -> Check Profile -> Theme matches
- [x] Verified manual test: Login as User A (Theme X) -> Logout -> Login as User B (Theme Y) -> Theme switches to Y

---

### Task 2.5: Internationalization (i18n) Implementation (Formerly 8.1)
**Priority:** HIGH  
**Estimated Complexity:** Medium  
**Dependencies:** None (but should be done early to avoid refactoring)

#### Objective
Implement a complete internationalization system supporting English and French, with all text content stored in translation resource files.

#### Specifications
- **Frontend:**
  - Install and configure i18n library (e.g., `react-i18next`)
  - Create translation resource files:
    - `/src/locales/en.json` - English translations
    - `/src/locales/fr.json` - French translations
  - Create translation structure covering:
    - Navigation menu items
    - Page titles and headers
    - Button labels
    - Form labels and placeholders
    - Error messages
    - Tooltips and help text
    - Welcome page content
    - Guide content
  - Create `useTranslation` hook wrapper for easy access
  - Add language selector in user preferences
  - Detect browser language on first visit (default)
  - Store language preference in user preferences (backend)

- **Backend:**
  - Add `Language` field to `UserPreferences` model:
    ```csharp
    public class UserPreferences
    {
        public int QuestionCount { get; set; } = 10;
        public string PreferredTheme { get; set; } = "derot-brain";
        public string Language { get; set; } = "auto"; // "en", "fr", or "auto"
        // ... other fields
    }
    ```
  - Update preferences endpoints to handle language

- **Translation File Structure:**
  ```json
  {
    "nav": {
      "derot": "Derot",
      "history": "History",
      "backlog": "Backlog",
      "profile": "Profile",
      "preferences": "Preferences",
      "guide": "Guide",
      "logout": "Logout"
    },
    "common": {
      "save": "Save",
      "cancel": "Cancel",
      "delete": "Delete",
      "edit": "Edit",
      "confirm": "Confirm"
    },
    "derot": {
      "recycle": "Recycle",
      "addToBacklog": "Add to Backlog",
      "startQuiz": "Start Quiz"
    }
    // ... etc
  }
  ```

#### Acceptance Criteria
- [x] All UI text comes from translation files (no hardcoded strings)
- [x] Users can switch between English and French
- [x] Language preference persists across sessions
- [x] Browser language auto-detected on first visit
- [x] All pages and components fully translated
- [ ] Date/time formatting respects selected language

---

### Task 2.6: Category Preferences Management (Formerly 8.2)
**Priority:** HIGH  
**Estimated Complexity:** Medium (simplified from High)  
**Dependencies:** Task 0.1 (Seed Data), Task 2.1 (User Preferences), Task 8.1 (i18n for labels)

#### Objective
Allow users to select their preferred Wikipedia categories for article filtering. Categories are loaded from seed data initialized in Task 0.1.

#### Specifications
- **Backend:**
  - **Use categories from seed data** (initialized in Task 0.1)
  - Categories are already available via `GET /api/categories`
  - No need to hardcode or create categories here
  
  - Update `UserPreferences` model:
    ```csharp
    public class UserPreferences
    {
        public int QuestionCount { get; set; } = 10;
        public string PreferredTheme { get; set; } = "derot-brain";
        public string Language { get; set; } = "auto";
        public List<string> SelectedCategories { get; set; } = new(); // Category IDs from seed data
    }
    ```
  
  - **Default for new users:** ALL 13 categories selected (all category IDs from seed data)
  - Initialization logic:
    ```csharp
    // On user creation
    var allCategories = await _categoryService.GetAllCategories();
    newUser.Preferences.SelectedCategories = allCategories.Select(c => c.Id).ToList();
    ```
  
  - Add endpoints:
    - `GET /api/users/{id}/preferences/categories` - Get user's selected categories
    - `PUT /api/users/{id}/preferences/categories` - Update selected categories
  
  - Validation:
    - At least one category must be selected
    - Categories must exist in seed data
    - Validate against `GET /api/categories`

- **Frontend:**
  - Fetch available categories from `GET /api/categories`
  - Update `UserPreferencesPage.tsx` to include category section:
    - **Section: "Wikipedia Categories"**
    - Display all categories from API as checkboxes
    - Use translated names based on current language (Name or NameFr)
    - Visual grouping (optional: by theme)
    - "Select All" / "Deselect All" buttons
    - Indication of how many categories are selected (e.g., "8/13 selected")
  
  - Category selection UI:
    - Checkboxes for each category
    - Labels from API (translated via category.Name or category.NameFr)
    - Responsive layout (grid or list)
    - Visual feedback on selection
  
  - Save behavior:
    - "Save" button updates preferences
    - Validation: at least 1 category required
    - Success/error notification
  
  - Default state for new users: All categories checked (loaded from API)

#### Acceptance Criteria
- [x] Categories loaded from seed data via API
- [x] User preferences include selected categories list (IDs)
- [x] New users have ALL categories selected by default
- [x] Users can check/uncheck categories in Preferences page
- [x] At least one category must remain selected
- [x] "Select All" and "Deselect All" buttons work correctly
- [x] Changes save successfully to backend
- [x] Category selection persists across sessions
- [x] Category names displayed in correct language (EN/FR)
- [x] Validation prevents selecting non-existent categories

---

## Phase 3: Application Structure (Sprint A)

### Task 3.1: Main Navigation Menu (Formerly 4.1)
**Priority:** HIGH  
**Estimated Complexity:** Medium  
**Dependencies:** None

#### Objective
Implement a navigation menu to allow users to move between different pages of the application.

#### Specifications
- **Frontend:**
  - Create `NavigationMenu.tsx` component with links to:
    - **Derot** (main Wikipedia/quiz page)
    - **History** (activity history)
    - **Backlog** (user's saved articles)
    - **Profile** (user information)
    - **Preferences** (user settings)
    - **Guide** (help/welcome guide)
    - **Logout**
  - Design options:
    - Sidebar navigation (collapsible)
    - Top navigation bar
    - Hamburger menu for mobile
  - Highlight active page
  - Integrate with existing `Layout.tsx`

- **Routing:**
  - Set up React Router routes for all pages
  - Ensure navigation preserves user state
  - Add route guards for authenticated pages
  - **Home page routing:**
    - Not authenticated: User selection/login page
    - Authenticated: History page (user's home)
  - Logo/app title clickable → navigates to home

- **Header Authentication State:**
  - **When NOT authenticated:**
    - Display language selector
    - Display theme selector
    - No user menu or settings button
  
  - **When authenticated:**
    - **User Menu Button** (user icon + username):
      - Dropdown menu with:
        - Profile (navigate to profile page)
        - History (navigate to history page)
        - Backlog (navigate to backlog page)
        - Logout (clear session, return to login)
    - **Settings Button** (gear/cog icon):
      - Navigate to preferences page
    - **Logout Button** (logout icon):
      - Alternative quick logout option
    - Button order: Settings → User Menu → Logout
  
  - Header should adapt dynamically based on authentication state
  - Smooth transitions when auth state changes

#### Acceptance Criteria
- [ ] Navigation menu accessible from all pages
- [ ] All pages reachable via navigation
- [ ] Active page highlighted in menu
- [ ] Navigation responsive on mobile
- [ ] Logout functionality works correctly
- [ ] Header shows language/theme selectors when not authenticated
- [ ] Header shows user menu + settings + logout when authenticated
- [ ] User menu dropdown displays all required links
- [ ] Settings button navigates to preferences
- [ ] Logo/title clickable and navigates to correct home page
- [ ] Authenticated home page is history page
- [ ] Header transitions smoothly on auth state change

---

### Task 3.2: User Profile Page (Formerly 4.2)
**Priority:** MEDIUM  
**Estimated Complexity:** Low  
**Dependencies:** Task 2.1

#### Objective
Create a user profile page displaying user information with edit capabilities.

#### Specifications
- **Frontend:**
  - Create `UserProfilePage.tsx` with:
    - Display fields:
      - Name (editable)
      - User ID (read-only)
      - Account created date (read-only)
      - Last connection date (read-only)
      - Total activities count
      - Total backlog items count
    - Edit mode toggle
    - Save/Cancel buttons in edit mode
  - Add validation for name field (non-empty, max length)
  - **Account Deletion:**
    - Add "Delete Account" button (danger zone section)
    - Click opens confirmation modal:
      - Title: "Delete Account?"
      - Warning message: "⚠️ This action is permanent and cannot be undone."
      - Details: "All your data will be deleted, including:"
        - Profile information
        - Activity history
        - Backlog items
        - Preferences
      - Confirmation input: "Type your username to confirm"
      - Actions: "Delete Account" (danger button), "Cancel"
    - On confirmation: Delete user and redirect to login

- **Backend:**
  - Add endpoint: `PUT /api/users/{id}` to update user name
  - Add endpoint: `DELETE /api/users/{id}` to delete user account
    - Delete user profile JSON file
    - Delete user history JSON file
    - Delete user backlog JSON file
    - Return 204 No Content on success
  - Add validation for user updates
  - Add validation for deletion (user must exist)

#### Acceptance Criteria
- [ ] Profile displays all user information
- [ ] User can edit their name
- [ ] Changes save successfully
- [ ] Validation prevents invalid names
- [ ] Statistics (activity count, backlog count) accurate
- [ ] Delete Account button visible in danger zone
- [ ] Confirmation modal displays all warnings
- [ ] Username confirmation required before deletion
- [ ] Account deletion removes all user data
- [ ] User redirected to login after deletion
- [ ] Deleted user cannot log back in

---

## Phase 4: Data Infrastructure & LLM (Sprint B)

### Task 4.1: LLM Configuration UI (Formerly 2.3)
**Priority:** MEDIUM  
**Estimated Complexity:** Medium  
**Dependencies:** Task 0.1 (Global LLM Configuration)

#### Objective
Add frontend UI for LLM configuration in the user preferences page, allowing users to configure and test LLM connectivity.

#### Specifications
- **Frontend:**
  - Add LLM Configuration section to `UserPreferencesPage.tsx` in "General Settings"
  - Place BEFORE question count selector
  - Two implementation options:
    
    **Option A: Separate URL and Port Fields**
    - URL input field (e.g., "http://localhost")
    - Port input field (e.g., "11434")
    - Read-only combined display showing "URL:Port" (e.g., "http://localhost:11434")
    - Auto-update combined display when either field changes
    
    **Option B: Single URL Field with Validation (Recommended)**
    - Single input field for complete URL (e.g., "http://localhost:11434")
    - Validation button (💾 icon or "Test Connection")
    - Click triggers backend validation
    - Display modal with results:
      - **Success:** "✅ LLM accessible at [URL]"
      - **Error:** "❌ Unable to reach LLM. Please verify your URL and port."
  
  - Save button updates global configuration
  - Load current LLM configuration on page load

- **Backend:**
  - Add endpoint: `POST /api/config/llm/test` - Test LLM connectivity
    - Accepts URL as parameter
    - Attempts connection to LLM
    - Returns success/failure with error details
  - Update existing `PUT /api/config/llm` endpoint if needed
  - Validation:
    - URL format validation
    - Port range validation (1-65535)
    - Timeout handling (max 5 seconds)

- **Error Handling:**
  - Network errors (LLM not running)
  - Invalid URL format
  - Port not accessible
  - Timeout errors
  - Display user-friendly error messages

#### Acceptance Criteria
- [ ] LLM configuration section visible in preferences page
- [ ] Section appears before question count selector
- [ ] Users can input LLM URL (and port if Option A)
- [ ] Test connection button validates LLM accessibility
- [ ] Success modal shows when LLM is reachable
- [ ] Error modal shows clear message when LLM is unreachable
- [ ] Configuration saves to backend successfully
- [ ] Current configuration loads on page initialization
- [ ] URL validation prevents invalid formats
- [ ] Timeout prevents indefinite waiting

---

### Task 4.2: Enhanced Activity Model (Formerly 3.1)
**Priority:** HIGH  
**Estimated Complexity:** Medium  
**Dependencies:** None

#### Objective
Enhance the activity history to track LLM information, best scores, and "Tracked Topic" status (formerly Backlog).

#### Specifications
- **Backend:**
  - Update `UserActivity.cs` model:
    ```csharp
    public class UserActivity
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Topic { get; set; } // Wikipedia article title
        public string WikipediaUrl { get; set; }
        public DateTime FirstAttemptDate { get; set; }
        public DateTime LastAttemptDate { get; set; }
        public int LastScore { get; set; }
        public int BestScore { get; set; }
        public int TotalQuestions { get; set; } // e.g., 5, 10, 15, 20
        public LLMInfo LlmUsed { get; set; }
        public bool IsTracked { get; set; } // True if topic is in Tracked Topics
        public string Type { get; set; } // "Read" or "Quiz"
    }
    
    public class LLMInfo
    {
        public string ModelName { get; set; } // e.g., "llama3:8b"
        public string Version { get; set; } // e.g., "v1.0"
    }
    ```
  - Update `UserService.cs` to:
    - Track best score across all attempts
    - Update last attempt date on each session
    - **Logic for "Read" vs "Quiz"**:
      - **Read**: Created if user scrolls to bottom OR clicks "Start Quiz".
      - **Quiz**: Created (or updates Read) only if answers are submitted.
    - Store LLM information with each activity
  - Add migration logic for existing activities

- **Frontend:**
  - Update `UserActivity` TypeScript interface
  - Update history display to show:
    - Last score: `X/Y` (e.g., 7/10)
    - Best score: `X/Y` (e.g., 9/10)
    - LLM used (shown on hover or in details)
    - Tracked indicator (⭐ icon)

#### Acceptance Criteria
- [ ] Activities track both last and best scores
- [ ] Activity Type ("Read" vs "Quiz") is correctly determined
- [ ] LLM information stored with each activity
- [ ] Tracked status (IsTracked) available per activity
- [ ] Existing activities migrated successfully

---

## Phase 5: Data Views - History & Backlog (Sprint C)

### Task 5.1: Backlog Page (Formerly 4.3)
**Priority:** HIGH  
**Estimated Complexity:** Medium  
**Dependencies:** Task 4.2

#### Objective
Create a dedicated backlog page where users can view and manage their saved articles.

#### Specifications
- **Frontend:**
  - Create `BacklogPage.tsx` with:
    - Grid/list view of backlog items
    - Each item shows:
      - Article title
      - Date added to backlog
      - Last attempt date (if any)
      - Best score (if attempted)
      - Actions: "Start Quiz", "Remove from Backlog"
    - Empty state message when backlog is empty
    - Search/filter functionality

- **Backend:**
  - Add endpoints:
    - `GET /api/users/{id}/backlog` - Get all backlog items
    - `POST /api/users/{id}/backlog` - Add item to backlog
    - `DELETE /api/users/{id}/backlog/{activityId}` - Remove from backlog
  - Update `UserActivity` service to manage backlog flag

#### Acceptance Criteria
- [ ] Backlog page displays all saved articles
- [ ] Users can start quiz from backlog
- [ ] Users can remove items from backlog
- [ ] Empty state shown when no items
- [ ] Search/filter works correctly

---

### Task 5.2: Enhanced History View (Formerly 3.2)
**Priority:** MEDIUM  
**Estimated Complexity:** Medium  
**Dependencies:** Task 4.2

#### Objective
Update the history view to display enhanced activity information with "Tracked Topic" indicators and Split Card design.

#### Specifications
- **Frontend:**
  - Update `history-view.tsx` component:
    - Display activities as cards (Grid layout)
    - **Card Design**:
      - **Standard**: Simple card for non-tracked topics.
      - **Split Card (Tracked Topics)**: 
        - Left: Current Session Stats
        - Right: Best Score (Personal Record)
        - *Mobile*: Vertical layout instead of split.
      - **Festive Card**: Special style if Session Score > Best Score (New Record!)
    - Data displayed:
      - Topic (clickable to restart)
      - Date
      - Badge "Read" or "Quiz" + Score
      - Tracked indicator (⭐ icon)
      - Actions: "Track Topic" (Add to favorites)
  - Add sorting options (by date, score, topic)
  - Add filtering options (all, tracked, non-tracked)

- **Interactions:**
  - Click topic to restart quiz on that article
  - Click "Track Topic" (⭐) to add to tracked topics
  - Hover over LLM info to see model details

#### Acceptance Criteria
- [ ] History displays Split Cards for tracked topics
- [ ] Mobile view uses vertical layout for cards
- [ ] Festive card appears for new records
- [ ] Tracked items clearly indicated with ⭐ icon
- [ ] Users can track/untrack topics from history
- [ ] Sorting and filtering work correctly

---

### Task 5.3: Activity Statistics (Formerly 3.3)
**Priority:** LOW  
**Estimated Complexity:** Medium  
**Dependencies:** Task 4.2, Task 5.2

#### Objective
Add an enhanced statistics section to the history page with a GitLab-style activity calendar and best personal score display.

#### Specifications
- **Frontend:**
  - Create `ActivityStatsPanel.tsx` component
  - Display as sidebar or top section on history page
  - Include the following statistics:
    
    **Key Statistics:**
    - **Last Activity:** Date + Wikipedia article name (clickable to rework)
    - **Total Activities:** Count of all completed activities
    - **Best Personal Score:** Display as "X/Y (Z%) - [Article Name]"
      - Example: "18/20 (90%) - Quantum Mechanics"
      - Clickable to view that activity's details
    
    **Activity Calendar (GitLab-style):**
    - Visual grid showing activity over time
    - Each cell represents a day
    - Color intensity based on activity count that day
    - Hover shows: "X activities on [date]"
    - Display last 12 months or configurable range
    - Use theme colors for intensity gradient
  
  - Responsive design (collapse to compact view on mobile)
  - Smooth animations for data updates

- **Backend:**
  - Add endpoint: `GET /api/users/{id}/statistics`
    - Returns:
      ```json
      {
        "lastActivity": {
          "date": "2026-01-18T14:30:00Z",
          "topic": "Quantum Mechanics",
          "activityId": "..."
        },
        "totalActivities": 42,
        "bestScore": {
          "score": 18,
          "totalQuestions": 20,
          "percentage": 90,
          "topic": "Quantum Mechanics",
          "activityId": "...",
          "date": "2026-01-15T10:00:00Z"
        },
        "activityCalendar": [
          { "date": "2026-01-18", "count": 3 },
          { "date": "2026-01-17", "count": 1 },
          // ... last 365 days
        ]
      }
      ```
  - Optimize query performance for calendar data
  - Cache statistics (update on activity completion)

- **Design:**
  - Follow existing theme system
  - Use consistent color palette for calendar
  - Ensure accessibility (screen readers, keyboard navigation)
  - Add loading states for statistics fetch

#### Acceptance Criteria
- [ ] Statistics panel displays on history page
- [ ] Last activity shows date and article name
- [ ] Total activities count is accurate
- [ ] Best personal score displays correctly with article name
- [ ] Activity calendar shows last 12 months
- [ ] Calendar cells show correct activity counts
- [ ] Hover tooltips display date and count
- [ ] Color intensity reflects activity frequency
- [ ] Clicking best score navigates to that activity
- [ ] Clicking last activity allows reworking
- [ ] Statistics update when new activity completed
- [ ] Responsive design works on mobile
- [ ] Component respects current theme

---

### Task 5.4: Enhanced History and Backlog Actions (Formerly 8.4)
**Priority:** MEDIUM  
**Estimated Complexity:** Low  
**Dependencies:** Task 5.2, Task 5.1

#### Objective
Update History and Backlog pages with improved action buttons and interactions.

#### Specifications
- **History Page Updates:**
  - Update `history-view.tsx` component:
    - **"Rework Topic" button** for each activity:
      - Icon: 🔄 or similar
      - Tooltip: "Rework this topic with a new quiz"
      - Action: Navigate to Derot page with article loaded
      - Disables profile filter (as per Task 6.3 rules)
    - **Book icon (📖) for backlog toggle:**
      - Clickable icon (not just indicator)
      - Filled/solid when in backlog
      - Outline/empty when not in backlog
      - Click toggles backlog status
      - No confirmation modal (instant toggle)
      - Visual feedback on toggle (animation/color change)
      - Tooltip: "Add to backlog" / "Remove from backlog"

- **Backlog Page Updates:**
  - Update `BacklogPage.tsx` component:
    - **"Rework Topic" button** for each item:
      - Same as history page
      - Navigates to Derot page with article
    - **Trash icon (🗑️) for removal:**
      - Click opens confirmation modal:
        - Title: "Remove from backlog?"
        - Message: "Are you sure you want to remove '{ArticleTitle}' from your backlog? This won't delete your activity history."
        - Actions: "Remove", "Cancel"
      - On confirm: Remove from backlog
      - Visual feedback (item fades out)

- **Backend:**
  - Update backlog endpoints:
    - `POST /api/users/{id}/backlog/{activityId}/toggle` - Toggle backlog status
    - Ensure DELETE endpoint has proper validation

#### Acceptance Criteria
- [ ] History page has "Rework Topic" button for each activity
- [ ] Book icon in history is clickable and toggles backlog status
- [ ] Book icon shows filled/outline state correctly
- [ ] Backlog page has "Rework Topic" button
- [ ] Trash icon in backlog opens confirmation modal
- [ ] Confirmation modal clearly explains the action
- [ ] Backlog removal works correctly
- [ ] Visual feedback on all actions
- [ ] Tooltips explain each action

---

## Phase 6: Core Functionality - Derot Page (Sprint D)

### Task 6.1: Derot Page - Wikipedia Integration (Formerly 5.1)
**Priority:** HIGH  
**Estimated Complexity:** High  
**Dependencies:** Task 2.1 (for question count preference)

#### Objective
Create the main "Derot" page where users read Wikipedia articles and take quizzes.

#### Specifications
- **Backend:**
  - Create `WikipediaService.cs`:
    - Integrate with Wikipedia API
    - Fetch random article
    - Fetch article by category/interest
    - **Use User Language**: Fetch content from appropriate Wikipedia locale (en/fr) based on user preference
    - Parse and clean article content
  - Add endpoints:
    - `GET /api/wikipedia/random` - Get random article
    - `GET /api/wikipedia/article/{title}` - Get specific article
    - `GET /api/wikipedia/categories` - Get available categories

- **Frontend:**
  - Create `DerotPage.tsx` with:
    - Article display area (markdown rendering)
    - "Recycle" button (get new article without saving)
    - "Start Quiz" button
    - "Add to Backlog" button
    - Sidebar/overlay for quick access to:
      - History (modal/drawer)
      - Backlog (modal/drawer)
    - Current article state preservation during navigation

- **State Management:**
  - Preserve article content when opening history/backlog
  - Don't save to history until at least one answer submitted

#### Acceptance Criteria
- [ ] Wikipedia articles load and display correctly
- [ ] "Recycle" button loads new article without saving
- [ ] "Add to Backlog" saves current article
- [ ] History/backlog accessible without losing article state
- [ ] Article only saved to history after quiz submission

---

### Task 6.2: Derot Page - Quiz Generation & Evaluation (Formerly 5.2)
**Priority:** HIGH  
**Estimated Complexity:** Very High  
**Dependencies:** Task 6.1, Task 4.1 (LLM Config)

#### Objective
Implement the core quiz loop: generate questions via LLM, collect answers, evaluate results, and save to history.

#### Specifications
- **Backend:**
  - Create `LlmService.cs`:
    - Connect to configured LLM (Ollama)
    - Construct prompts for Question Generation
    - Construct prompts for Answer Evaluation
  - **Question Generation:**
    - Input: Article content (chunked if needed)
    - Output: JSON array of questions (Multiple Choice or Open)
    - Count: Based on user preference (default 10)
  - **Evaluation:**
    - Input: User answers + Correct answers/Context
    - Output: Score (0-100%), Corrections/Explanations

- **Frontend:**
  - **Quiz Mode UI:**
    - Display questions one by one or list (User preference? start with 1 by 1)
    - Input fields for answers
    - "Submit" button
  - **Results Mode UI:**
    - Score display
    - Question-by-question review:
      - User answer vs Correct answer
      - Explanation
    - "Try Another Article" button
    - "Retry Quiz" button (same article)

- **Flow:**
  1. User reads article -> Clicks "Start Quiz"
  2. Backend generates questions (show loading spinner)
  3. User answers questions -> Clicks "Submit"
  4. Backend evaluates -> Returns results -> Saves Activity (Score)
  5. Frontend displays results

#### Acceptance Criteria
- [ ] Quiz generates correct number of questions
- [ ] LLM integration works for generation
- [ ] User can submit answers
- [ ] LLM evaluates answers accurately
- [ ] Results page shows score and corrections
- [ ] Activity saved to history upon completion
- [ ] Loading states handled gracefully

---

### Task 6.3: Category Filtering on Derot Page (Formerly 8.3)
**Priority:** HIGH  
**Estimated Complexity:** Medium  
**Dependencies:** Task 2.6 (Category Prefs), Task 6.1

#### Objective
Implement category filtering on the Derot page using user's selected categories from preferences, with ability to temporarily modify selection.

#### Specifications
- **Frontend (Derot Page):**
  - **Category Filter Section** at top of page:
    - Display user's selected categories from preferences
    - Show as checkboxes or chips/badges
    - User can temporarily check/uncheck categories
    - Visual indicator showing X/13 categories selected
  
  - **Filter Behavior:**
    - **When starting NEW activity:**
      - Categories loaded from user preferences
      - User can modify selection temporarily
      - Changes are NOT saved unless "Save" button clicked
      - Visual indicator: "⚠️ Temporary changes (not saved to preferences)"
    
    - **When working from Backlog/History:**
      - Category filter HIDDEN or DISABLED (greyed out)
      - Message: "Category filter not available when reworking an article"
      - Cannot modify categories
    
    - **After clicking "Recycle" button:**
      - Category filter becomes available again
      - **All categories UNCHECKED** (complete reset)
      - User must re-select categories or click "Load from Preferences"

  - **Temporary Modifications UI:**
    - Checkboxes for all 13 categories
    - Current selection highlighted
    - Counter: "X/13 categories selected"
    - **Warning indicator** when different from saved preferences
    - **"Save to Preferences" button** appears when modifications made:
      - Click opens confirmation modal:
        - "Save these X categories to your preferences?"
        - "This will update your default category selection"
        - Options: "Save", "Cancel"
    - **"Load from Preferences" button:**
      - Reloads categories from user preferences
      - Discards temporary changes
    - **"Reset" button:**
      - Unchecks ALL categories
      - Only available for new activity (not from backlog/history)

- **Backend:**
  - Update Wikipedia service to filter by categories:
    - `GET /api/wikipedia/random?categories=culture-arts,science`
  - Ensure at least one category is selected for filtering
  - Return random article from selected categories

#### Acceptance Criteria
- [ ] Category checkboxes display user's preferences on load
- [ ] Filter hidden/disabled when reworking article from backlog/history
- [ ] Filter available for new activities
- [ ] User can modify categories temporarily
- [ ] Visual indicator shows temporary changes
- [ ] "Save to Preferences" button appears when modified
- [ ] Confirmation modal explains what will be saved
- [ ] "Load from Preferences" button restores saved selection
- [ ] "Reset" button unchecks all categories
- [ ] Recycle button unchecks all categories (complete reset)
- [ ] Wikipedia API filters articles by selected categories
- [ ] At least one category must be selected to load article
- [ ] UX clearly communicates temporary vs. saved changes

---

### Task 6.4: LLM Resource Estimation & Monitoring (Formerly 5.3)
**Priority:** MEDIUM  
**Estimated Complexity:** Low  
**Dependencies:** Task 6.1

#### Objective
Estimate and display the computational cost (time/RAM) of generating a quiz for a given article length, preventing user frustration.

#### Specifications
- **Backend:**
  - Analyze article length (token count approx)
  - Estimate generation time based on LLM model (e.g., 50 tokens/sec)
  - Return metadata with article:
    ```json
    {
      "title": "Quantum Mechanics",
      "wordCount": 5000,
      "estimatedGenTime": 120, // seconds
      "complexity": "High"
    }
    ```

- **Frontend:**
  - Display "Estimated Quiz Gen Time: ~2 mins" on Derot page
  - Warning if article is very long (>5000 words):
    - "⚠️ Long article. Quiz generation may take 2-3 minutes."
  - Progress bar during generation (simulated or real if streaming)

#### Acceptance Criteria
- [ ] Estimated generation time displayed
- [ ] Warnings shown for long articles
- [ ] Progress feedback provided during wait

---

## Phase 7: Data Management (User Export)

### Task 7.1: User Data Export Feature (Formerly 6.1)
**Priority:** LOW  
**Estimated Complexity:** Medium  
**Dependencies:** All previous tasks

#### Objective
Allow users to export their data (profile, preferences, backlog, and optionally history).

#### Specifications
- **Backend:**
  - Create `ExportService.cs`:
    - Generate JSON export of user data
    - Include: user profile, preferences, backlog
    - Optional: include full history
  - Add endpoint:
    - `GET /api/users/{id}/export?includeHistory=true` - Export user data

- **Frontend:**
  - Add export button in user profile or preferences page
  - Create `ExportModal.tsx`:
    - Checkbox: "Include activity history"
    - Export format: JSON
    - Download button
  - Generate downloadable file with user data

#### Acceptance Criteria
- [ ] Users can export their data
- [ ] Export includes profile and preferences
- [ ] Export includes backlog
- [ ] History inclusion is optional
- [ ] Downloaded file is valid JSON
- [ ] Export button accessible from profile/preferences

---

## Phase 8: User Guidance & Onboarding

### Task 8.1: Contextual Help & Tooltips (Formerly 7.1)
**Priority:** LOW  
**Estimated Complexity:** Low  
**Dependencies:** All page implementations

#### Objective
Add contextual help and tooltips throughout the application to guide users.

#### Specifications
- **Frontend:**
  - Add tooltips to all interactive elements
  - Add help icons (?) next to complex features
  - Create `HelpTooltip.tsx` reusable component
- **Welcome Page Updates:**
  - Ensure welcome page mentions all key features

#### Acceptance Criteria
- [ ] All interactive elements have tooltips
- [ ] Tooltips are clear and concise
- [ ] Help icons provide additional context
- [ ] Welcome page covers all features

---

### Task 8.2: Date Format Preferences (Formerly 8.5)
**Priority:** MEDIUM  
**Estimated Complexity:** Low  
**Dependencies:** Task 2.5 (i18n)

#### Objective
Allow users to select their preferred date display format (French/European vs American), while ensuring backend data remains standardized.

#### Specifications
- **Constraints (CRITICAL):**
  - **Backend Persistence:** Dates in the data layer MUST always remain in the "French" format (or standard ISO).
  - **Frontend Responsibility:** The frontend is solely responsible for converting the standardized backend date into the user's preferred display format.
  - **Storage:** This preference is NOT persisted. It functions as a session-level setting.

- **Frontend:**
  - Add a "Date Format" dropdown in the **Preferences Page**.
    - Options: "French (dd/MM/yyyy)", "American (MM/dd/yyyy)".
  - Create a centralized date formatting utility/hook that respects this setting.
  - Apply this formatting to all date displays.
  - Default: French format.

#### Acceptance Criteria
- [ ] Date format dropdown available in Preferences
- [ ] User can switch between formats
- [ ] All dates in the UI update immediately to reflect the choice
- [ ] Backend data files remain completely unchanged
- [ ] Preference functions as a transient/session setting

---

## Phase 9: Deployment & Distribution

### Task 9.1: Cross-Platform Packaging
**Priority:** MEDIUM  
**Estimated Complexity:** High  
**Dependencies:** All core features

#### Objective
Package the application for Windows, macOS, and Linux as self-contained, distributable applications.

#### Specifications
- **Backend:** Self-contained .NET deployment.
- **Frontend:** Built production bundle embedded in backend.
- **Platforms:** Windows (.exe), macOS (.app), Linux (AppImage).

#### Acceptance Criteria
- [ ] Application runs on Windows, macOS, Linux
- [ ] No external dependencies (.NET installed) required
- [ ] Backend starts automatically

---

### Task 9.2: Installer Creation
**Priority:** MEDIUM  
**Estimated Complexity:** Medium  
**Dependencies:** Task 9.1

#### Objective
Create user-friendly installers for each platform.

#### Specifications
- **Windows:** MSI/EXE installer.
- **macOS:** DMG/PKG.
- **Linux:** AppImage/Snap.

#### Acceptance Criteria
- [ ] Installers created for all platforms
- [ ] Install/Uninstall works correctly
- [ ] Shortcuts created

---

### Task 9.3: User Documentation & Distribution
**Priority:** MEDIUM  
**Estimated Complexity:** Low  
**Dependencies:** Task 9.2

#### Objective
Create comprehensive user documentation and setup distribution.

#### Specifications
- **Docs:** Installation Guide, User Manual, Troubleshooting.
- **Distribution:** GitHub Releases.

#### Acceptance Criteria
- [ ] Documentation complete
- [ ] GitHub Releases set up

---

## Implementation Priority Order

### Phase -1: Frontend Architecture Migration (Pre-Sprint 0)
1. **Task -1.1: Infrastructure Layer Setup** - [x]
2. **Task -1.2: Zustand State Management Setup** - [/]
3. **Task -1.3: Custom Hooks Implementation** - [/]
4. **Task -1.4: Component Refactoring** - [/]
5. **Task -1.5: Context Cleanup & Verification** - [ ]

### Sprint 0: Foundation (Pre-Sprint 1)
6. **Task 0.1: Application Initialization & Configuration** - [x]

### Phase 1: Core Infrastructure
7. **Task 1.1: Session Persistence** - [x]
8. **Task 1.2: Welcome Page** - [x]

### Phase 2: User Preferences & i18n
9. **Task 2.1: Extend User Model** - [ ]
10. **Task 2.2: User Preferences Page** - [ ]
11. **Task 2.4: Contextual Preference Loading** - [ ]
12. **Task 2.5: Internationalization (i18n)** - [x]
13. **Task 2.6: Category Preferences** - [x]

### Phase 3: Application Structure (Sprint A)
14. **Task 3.1: Main Navigation Menu** - [ ]
15. **Task 3.2: User Profile Page** - [ ]

### Phase 4: Data Infrastructure & LLM (Sprint B)
16. **Task 4.1: LLM Configuration UI** - [ ]
17. **Task 4.2: Enhanced Activity Model** - [ ]

### Phase 5: Data Views - History & Backlog (Sprint C)
18. **Task 5.1: Backlog Page** - [ ]
19. **Task 5.2: Enhanced History View** - [ ]
20. **Task 5.3: Activity Statistics** - [ ]
21. **Task 5.4: Enhanced History Actions** - [ ]

### Phase 6: Derot Page (Sprint D)
22. **Task 6.1: Wikipedia Integration** - [ ]
23. **Task 6.2: Quiz Generation** - [ ]
24. **Task 6.3: Category Filtering** - [ ]
25. **Task 6.4: Resource Estimation** - [ ]

### Phase 7: Data Management
26. **Task 7.1: User Data Export** - [ ]

### Phase 8: User Guidance
27. **Task 8.1: Help & Tooltips** - [ ]
28. **Task 8.2: Date Preferences** - [ ]

### Phase 9: Deployment
29. **Task 9.1: Packaging** - [ ]
30. **Task 9.2: Installers** - [ ]
31. **Task 9.3: Documentation** - [ ]

**Priority:** HIGH  
**Estimated Complexity:** High  
**Dependencies:** Task 2.1 (for question count preference)

#### Objective
Create the main "Derot" page where users read Wikipedia articles and take quizzes.

#### Specifications
- **Backend:**
  - Create `WikipediaService.cs`:
    - Integrate with Wikipedia API
    - Fetch random article
    - Fetch article by category/interest
    - **Use User Language**: Fetch content from appropriate Wikipedia locale (en/fr) based on user preference
    - Parse and clean article content
  - Add endpoints:
    - `GET /api/wikipedia/random` - Get random article
    - `GET /api/wikipedia/article/{title}` - Get specific article
    - `GET /api/wikipedia/categories` - Get available categories

- **Frontend:**
  - Create `DerotPage.tsx` with:
    - Article display area (markdown rendering)
    - "Recycle" button (get new article without saving)
    - "Start Quiz" button
    - "Add to Backlog" button
    - Sidebar/overlay for quick access to:
      - History (modal/drawer)
      - Backlog (modal/drawer)
    - Current article state preservation during navigation

- **State Management:**
  - Preserve article content when opening history/backlog
  - Don't save to history until at least one answer submitted

#### Acceptance Criteria
- [ ] Wikipedia articles load and display correctly
- [ ] "Recycle" button loads new article without saving
- [ ] "Add to Backlog" saves current article
- [ ] History/backlog accessible without losing article state
- [ ] Article only saved to history after quiz submission

---

### Task 5.2: Derot Page - Quiz Generation & Evaluation
**Priority:** HIGH  
**Estimated Complexity:** High  
**Dependencies:** Task 5.1, Task 2.1

#### Objective
Implement quiz generation using LLM and answer evaluation.

#### Specifications
- **Backend:**
  - Create `LLMService.cs`:
    - Integrate with Ollama (or OpenAI as fallback)
    - Generate questions based on article content
    - Evaluate user answers semantically
    - Return LLM model information
  - Create `QuizService.cs`:
    - Generate quiz with configurable question count (from user preferences)
    - Evaluate answers using LLM
    - Calculate scores
    - Save quiz results to user activity
  - Add endpoints:
    - `POST /api/quiz/generate` - Generate quiz from article
    - `POST /api/quiz/evaluate` - Evaluate user answers
    - `POST /api/quiz/submit` - Submit quiz and save results

- **Frontend:**
  - Create `QuizView.tsx` component:
    - Display questions one at a time or all at once (configurable)
    - Text input for each answer
    - Submit button
    - Progress indicator
  - Create `QuizResultsView.tsx`:
    - Display final score (X/Y - Z%)
    - Show each question with:
      - User's answer
      - Expected answer
      - Evaluation result (correct/incorrect)
    - Actions: "Add to Backlog", "Try Another Article", "Return to History"

- **LLM Integration:**
  - Store LLM model name and version with each activity
  - Handle LLM errors gracefully (timeout, unavailable, etc.)

#### Acceptance Criteria
- [ ] Quiz generates correct number of questions (based on user preference)
- [ ] Questions are relevant to article content
- [ ] Answer evaluation is semantically accurate
- [ ] Results display correctly with all information
- [ ] Activity saved to history with LLM info
- [ ] Error handling for LLM failures

---

### Task 5.3: LLM Resource Estimation & Monitoring
**Priority:** LOW  
**Estimated Complexity:** Medium  
**Dependencies:** Task 5.1, Task 5.2

#### Objective
Estimate and display LLM resource requirements based on article size, helping users understand the computational demands of quiz generation.

#### Specifications
- **Backend:**
  - Create `LLMResourceEstimator.cs` service:
    - Calculate article size (word count, character count)
    - Estimate context size for each LLM phase:
      - **Phase 1:** Article reading/analysis (base context)
      - **Phase 2:** Question generation (prompt + article)
      - **Phase 3:** Answer correction (prompt + article + Q&A + validation)
    - Estimate CPU/RAM requirements based on:
      - Article size
      - Question count
      - LLM model size (e.g., 7B, 8B parameters)
    - Return resource estimates and warnings
  
  - Add endpoint: `POST /api/llm/estimate-resources`
    - Request body:
      ```json
      {
        "articleWordCount": 5000,
        "questionCount": 10,
        "modelName": "llama3:8b"
      }
      ```
    - Response:
      ```json
      {
        "articleSize": {
          "wordCount": 5000,
          "characterCount": 30000,
          "estimatedTokens": 6500
        },
        "contextSizes": {
          "phase1_reading": 6500,
          "phase2_generation": 7000,
          "phase3_correction": 8500
        },
        "resourceEstimate": {
          "minimumRAM_GB": 8,
          "recommendedRAM_GB": 16,
          "estimatedProcessingTime_seconds": 45,
          "warningLevel": "medium"
        },
        "warnings": [
          "Large article may require extended processing time",
          "Ensure at least 8GB RAM available"
        ]
      }
      ```

- **Frontend:**
  - Display resource estimate before starting quiz:
    - Show estimated processing time
    - Show RAM requirements
    - Display warnings for large articles
  - Add loading indicator during quiz generation
  - Optional: Show real-time progress for long operations
  - Add "Performance Tips" section in guide/help

- **Warning Levels:**
  - **Low:** < 2000 words, < 30 seconds
  - **Medium:** 2000-5000 words, 30-60 seconds
  - **High:** > 5000 words, > 60 seconds
  - Display appropriate warnings and recommendations

- **Performance Monitoring (Optional):**
  - Track actual processing time
  - Compare estimates vs. actual performance
  - Log performance metrics for optimization

#### Acceptance Criteria
- [ ] Resource estimation calculates article size correctly
- [ ] Context sizes estimated for all three LLM phases
- [ ] RAM requirements displayed based on model size
- [ ] Processing time estimate shown before quiz starts
- [ ] Warning levels (low/medium/high) calculated correctly
- [ ] Warnings displayed for large articles
- [ ] Frontend shows resource estimates before quiz generation
- [ ] Loading indicators show during LLM processing
- [ ] Performance tips available in help section
- [ ] Estimates are reasonably accurate (within 20% margin)

## Phase 6: User Export & Data Management

### Task 6.1: User Data Export Feature
**Priority:** LOW  
**Estimated Complexity:** Medium  
**Dependencies:** All previous tasks

#### Objective
Allow users to export their data (profile, preferences, backlog, and optionally history).

#### Specifications
- **Backend:**
  - Create `ExportService.cs`:
    - Generate JSON export of user data
    - Include: user profile, preferences, backlog
    - Optional: include full history
  - Add endpoint:
    - `GET /api/users/{id}/export?includeHistory=true` - Export user data

- **Frontend:**
  - Add export button in user profile or preferences page
  - Create `ExportModal.tsx`:
    - Checkbox: "Include activity history"
    - Export format: JSON
    - Download button
  - Generate downloadable file with user data

- **Export Format:**
  ```json
  {
    "user": {
      "id": "...",
      "name": "...",
      "createdAt": "...",
      "lastConnectionAt": "...",
      "preferences": { ... }
    },
    "backlog": [ ... ],
    "history": [ ... ] // Optional
  }
  ```

#### Acceptance Criteria
- [ ] Users can export their data
- [ ] Export includes profile and preferences
- [ ] Export includes backlog
- [ ] History inclusion is optional
- [ ] Downloaded file is valid JSON
- [ ] Export button accessible from profile/preferences

---

## Phase 7: User Guidance & Onboarding

### Task 7.1: Contextual Help & Tooltips
**Priority:** LOW  
**Estimated Complexity:** Low  
**Dependencies:** All page implementations

#### Objective
Add contextual help and tooltips throughout the application to guide users.

#### Specifications
- **Frontend:**
  - Add tooltips to all interactive elements:
    - "Recycle" button: "Load a new article without saving this one"
    - "Add to Backlog": "Save this article for later review"
    - "Start Quiz": "Begin quiz on this article (will save to history)"
    - Backlog icon in history: "This article is in your backlog"
    - LLM info: "AI model used to generate this quiz"
  - Add help icons (?) next to complex features
  - Create `HelpTooltip.tsx` reusable component

- **Welcome Page Updates:**
  - Ensure welcome page mentions all key features
  - Add "Did you know?" tips throughout the app
  - Create "Quick Tips" section in guide

#### Acceptance Criteria
- [ ] All interactive elements have tooltips
- [ ] Tooltips are clear and concise
- [ ] Help icons provide additional context
- [ ] Welcome page covers all features
- [ ] Tooltips respect current theme

---

## Phase 8: Internationalization & Interest Profiles

### Task 8.1: Internationalization (i18n) Implementation
**Priority:** HIGH  
**Estimated Complexity:** Medium  
**Dependencies:** None (but should be done early to avoid refactoring)

#### Objective
Implement a complete internationalization system supporting English and French, with all text content stored in translation resource files.

#### Specifications
- **Frontend:**
  - Install and configure i18n library (e.g., `react-i18next`)
  - Create translation resource files:
    - `/src/locales/en.json` - English translations
    - `/src/locales/fr.json` - French translations
  - Create translation structure covering:
    - Navigation menu items
    - Page titles and headers
    - Button labels
    - Form labels and placeholders
    - Error messages
    - Tooltips and help text
    - Welcome page content
    - Guide content
  - Create `useTranslation` hook wrapper for easy access
  - Add language selector in user preferences
  - Detect browser language on first visit (default)
  - Store language preference in user preferences (backend)

- **Backend:**
  - Add `Language` field to `UserPreferences` model:
    ```csharp
    public class UserPreferences
    {
        public int QuestionCount { get; set; } = 10;
        public string PreferredTheme { get; set; } = "derot-brain";
        public string Language { get; set; } = "auto"; // "en", "fr", or "auto"
        // ... other fields
    }
    ```
  - Update preferences endpoints to handle language

- **Translation File Structure:**
  ```json
  {
    "nav": {
      "derot": "Derot",
      "history": "History",
      "backlog": "Backlog",
      "profile": "Profile",
      "preferences": "Preferences",
      "guide": "Guide",
      "logout": "Logout"
    },
    "common": {
      "save": "Save",
      "cancel": "Cancel",
      "delete": "Delete",
      "edit": "Edit",
      "confirm": "Confirm"
    },
    "derot": {
      "recycle": "Recycle",
      "addToBacklog": "Add to Backlog",
      "startQuiz": "Start Quiz"
    }
    // ... etc
  }
  ```

#### Acceptance Criteria
- [x] All UI text comes from translation files (no hardcoded strings)
- [x] Users can switch between English and French
- [x] Language preference persists across sessions
- [x] Browser language auto-detected on first visit
- [x] All pages and components fully translated
- [ ] Date/time formatting respects selected language

---

### Task 8.2: Category Preferences Management
**Priority:** HIGH  
**Estimated Complexity:** Medium (simplified from High)  
**Dependencies:** Task 0.1 (Seed Data), Task 2.1 (User Preferences), Task 8.1 (i18n for labels)

#### Objective
Allow users to select their preferred Wikipedia categories for article filtering. Categories are loaded from seed data initialized in Task 0.1.

#### Specifications
- **Backend:**
  - **Use categories from seed data** (initialized in Task 0.1)
  - Categories are already available via `GET /api/categories`
  - No need to hardcode or create categories here
  
  - Update `UserPreferences` model:
    ```csharp
    public class UserPreferences
    {
        public int QuestionCount { get; set; } = 10;
        public string PreferredTheme { get; set; } = "derot-brain";
        public string Language { get; set; } = "auto";
        public List<string> SelectedCategories { get; set; } = new(); // Category IDs from seed data
    }
    ```
  
  - **Default for new users:** ALL 13 categories selected (all category IDs from seed data)
  - Initialization logic:
    ```csharp
    // On user creation
    var allCategories = await _categoryService.GetAllCategories();
    newUser.Preferences.SelectedCategories = allCategories.Select(c => c.Id).ToList();
    ```
  
  - Add endpoints:
    - `GET /api/users/{id}/preferences/categories` - Get user's selected categories
    - `PUT /api/users/{id}/preferences/categories` - Update selected categories
  
  - Validation:
    - At least one category must be selected
    - Categories must exist in seed data
    - Validate against `GET /api/categories`

- **Frontend:**
  - Fetch available categories from `GET /api/categories`
  - Update `UserPreferencesPage.tsx` to include category section:
    - **Section: "Wikipedia Categories"**
    - Display all categories from API as checkboxes
    - Use translated names based on current language (Name or NameFr)
    - Visual grouping (optional: by theme)
    - "Select All" / "Deselect All" buttons
    - Indication of how many categories are selected (e.g., "8/13 selected")
  
  - Category selection UI:
    - Checkboxes for each category
    - Labels from API (translated via category.Name or category.NameFr)
    - Responsive layout (grid or list)
    - Visual feedback on selection
  
  - Save behavior:
    - "Save" button updates preferences
    - Validation: at least 1 category required
    - Success/error notification
  
  - Default state for new users: All categories checked (loaded from API)

#### Acceptance Criteria
- [x] Categories loaded from seed data via API
- [x] User preferences include selected categories list (IDs)
- [x] New users have ALL categories selected by default
- [x] Users can check/uncheck categories in Preferences page
- [x] At least one category must remain selected
- [x] "Select All" and "Deselect All" buttons work correctly
- [x] Changes save successfully to backend
- [x] Category selection persists across sessions
- [x] Category names displayed in correct language (EN/FR)
- [x] Validation prevents selecting non-existent categories

---

### Task 8.3: Category Filtering on Derot Page
**Priority:** HIGH  
**Estimated Complexity:** Medium (simplified from High)  
**Dependencies:** Task 8.2, Task 5.1 (Derot Page)

#### Objective
Implement category filtering on the Derot page using user's selected categories from preferences, with ability to temporarily modify selection.

#### Specifications
- **Frontend (Derot Page):**
  - **Category Filter Section** at top of page:
    - Display user's selected categories from preferences
    - Show as checkboxes or chips/badges
    - User can temporarily check/uncheck categories
    - Visual indicator showing X/13 categories selected
  
  - **Filter Behavior:**
    - **When starting NEW activity:**
      - Categories loaded from user preferences
      - User can modify selection temporarily
      - Changes are NOT saved unless "Save" button clicked
      - Visual indicator: "⚠️ Temporary changes (not saved to preferences)"
    
    - **When working from Backlog/History:**
      - Category filter HIDDEN or DISABLED (greyed out)
      - Message: "Category filter not available when reworking an article"
      - Cannot modify categories
    
    - **After clicking "Recycle" button:**
      - Category filter becomes available again
      - **All categories UNCHECKED** (complete reset)
      - User must re-select categories or click "Load from Preferences"

  - **Temporary Modifications UI:**
    - Checkboxes for all 13 categories
    - Current selection highlighted
    - Counter: "X/13 categories selected"
    - **Warning indicator** when different from saved preferences
    - **"Save to Preferences" button** appears when modifications made:
      - Click opens confirmation modal:
        - "Save these X categories to your preferences?"
        - "This will update your default category selection"
        - Options: "Save", "Cancel"
    - **"Load from Preferences" button:**
      - Reloads categories from user preferences
      - Discards temporary changes
    - **"Reset" button:**
      - Unchecks ALL categories
      - Only available for new activity (not from backlog/history)

- **Backend:**
  - Update Wikipedia service to filter by categories:
    - `GET /api/wikipedia/random?categories=culture-arts,science`
  - Ensure at least one category is selected for filtering
  - Return random article from selected categories

- **State Management:**
  - Track current activity source: "new", "backlog", "history"
  - Track current category selection (temporary)
  - Track user's saved preferences (from backend)
  - Detect differences between temporary and saved
  - Reset state on "Recycle" click

#### Acceptance Criteria
- [ ] Category checkboxes display user's preferences on load
- [ ] Filter hidden/disabled when reworking article from backlog/history
- [ ] Filter available for new activities
- [ ] User can modify categories temporarily
- [ ] Visual indicator shows temporary changes
- [ ] "Save to Preferences" button appears when modified
- [ ] Confirmation modal explains what will be saved
- [ ] "Load from Preferences" button restores saved selection
- [ ] "Reset" button unchecks all categories
- [ ] Recycle button unchecks all categories (complete reset)
- [ ] Wikipedia API filters articles by selected categories
- [ ] At least one category must be selected to load article
- [ ] UX clearly communicates temporary vs. saved changes

---

### Task 8.4: Enhanced History and Backlog Actions
**Priority:** MEDIUM  
**Estimated Complexity:** Low  
**Dependencies:** Task 3.2 (Enhanced History), Task 4.3 (Backlog Page)

#### Objective
Update History and Backlog pages with improved action buttons and interactions.

#### Specifications
- **History Page Updates:**
  - Update `history-view.tsx` component:
    - **"Rework Topic" button** for each activity:
      - Icon: 🔄 or similar
      - Tooltip: "Rework this topic with a new quiz"
      - Action: Navigate to Derot page with article loaded
      - Disables profile filter (as per Task 8.3 rules)
    - **Book icon (📖) for backlog toggle:**
      - Clickable icon (not just indicator)
      - Filled/solid when in backlog
      - Outline/empty when not in backlog
      - Click toggles backlog status
      - No confirmation modal (instant toggle)
      - Visual feedback on toggle (animation/color change)
      - Tooltip: "Add to backlog" / "Remove from backlog"

- **Backlog Page Updates:**
  - Update `BacklogPage.tsx` component:
    - **"Rework Topic" button** for each item:
      - Same as history page
      - Navigates to Derot page with article
    - **Trash icon (🗑️) for removal:**
      - Click opens confirmation modal:
        - Title: "Remove from backlog?"
        - Message: "Are you sure you want to remove '{ArticleTitle}' from your backlog? This won't delete your activity history."
        - Actions: "Remove", "Cancel"
      - On confirm: Remove from backlog
      - Visual feedback (item fades out)

- **Backend:**
  - Update backlog endpoints:
    - `POST /api/users/{id}/backlog/{activityId}/toggle` - Toggle backlog status
    - Ensure DELETE endpoint has proper validation

#### Acceptance Criteria
- [ ] History page has "Rework Topic" button for each activity
- [ ] Book icon in history is clickable and toggles backlog status
- [ ] Book icon shows filled/outline state correctly
- [ ] Backlog page has "Rework Topic" button
- [ ] Trash icon in backlog opens confirmation modal
- [ ] Confirmation modal clearly explains the action
- [ ] Backlog removal works correctly
- [ ] Visual feedback on all actions
- [ ] Tooltips explain each action

---

### Task 8.5: Date Format Preferences
**Priority:** MEDIUM  
**Estimated Complexity:** Low  
**Dependencies:** Task 8.1 (Internationalization)

#### Objective
Allow users to select their preferred date display format (French/European vs American), while ensuring backend data remains standardized.

#### Specifications
- **Constraints (CRITICAL):**
  - **Backend Persistence:** Dates in the data layer MUST always remain in the "French" format (or standard ISO) as is currently done. The user's choice **DOES NOT** affect the format stored in backend JSON files.
  - **Frontend Responsibility:** The frontend is solely responsible for converting the standardized backend date into the user's preferred display format.
  - **Storage:** "Ce choix n'est pas persisté" - The user has specified this preference is NOT persisted. It functions as a session-level or temporary setting.

- **Frontend:**
  - Add a "Date Format" dropdown in the **Preferences Page** (alongside Language/Theme).
    - Options: "French (dd/MM/yyyy)", "American (MM/dd/yyyy)", etc.
  - Create a centralized date formatting utility/hook that respects this setting.
  - Apply this formatting to all date displays:
    - History dates
    - Profile dates
    - Backlog dates
  - Default: French format (as per current behavior).

#### Acceptance Criteria
- [ ] Date format dropdown available in Preferences
- [ ] User can switch between formats
- [ ] All dates in the UI update immediately to reflect the choice
- [ ] Backend data files remain completely unchanged
- [ ] Preference functions as a transient/session setting (not persisted to API)

---

## Phase 9: Deployment & Distribution

### Task 9.1: Cross-Platform Packaging
**Priority:** MEDIUM  
**Estimated Complexity:** High  
**Dependencies:** All core features (Phase 5 minimum)

#### Objective
Package the application for Windows, macOS, and Linux as self-contained, distributable applications that require no technical setup.

#### Specifications
- **Application Structure:**
  - Self-contained bundle including:
    - Frontend (built React app)
    - Backend (.NET runtime + API)
    - Embedded web server
    - All dependencies
  - Single executable or app bundle
  - No external dependencies required

- **Backend Packaging:**
  - Use .NET self-contained deployment
  - Include runtime for target platform
  - Embed Kestrel web server
  - Configure to run on localhost with random available port
  - Auto-start backend on application launch

- **Frontend Packaging:**
  - Build optimized production bundle
  - Embed in backend as static files
  - Serve via Kestrel
  - Configure API base URL dynamically

- **Platform-Specific Considerations:**
  - **Windows:**
    - Single .exe file or folder structure
    - No .NET installation required
    - Windows Defender compatibility
  
  - **macOS:**
    - .app bundle structure
    - Code signing (optional for V1)
    - Gatekeeper compatibility
  
  - **Linux:**
    - AppImage (single file, no installation)
    - Snap package (optional)
    - Flatpak (optional)

- **Configuration:**
  - Data directory: `~/DerotMyBrain/data/` or `%APPDATA%/DerotMyBrain/data/`
  - Logs directory: `~/DerotMyBrain/logs/` or `%APPDATA%/DerotMyBrain/logs/`
  - Auto-create directories on first run
  - Portable mode option (data in app directory)

#### Acceptance Criteria
- [ ] Application runs on Windows without .NET installation
- [ ] Application runs on macOS as .app bundle
- [ ] Application runs on Linux (AppImage minimum)
- [ ] Backend starts automatically with frontend
- [ ] No terminal/command line required to run
- [ ] Data persists in user directory
- [ ] Application is self-contained (no external dependencies)
- [ ] Port conflicts handled gracefully
- [ ] Application can run multiple instances (different ports)

---

### Task 9.2: Installer Creation
**Priority:** MEDIUM  
**Estimated Complexity:** Medium  
**Dependencies:** Task 9.1

#### Objective
Create user-friendly installers for each platform that handle installation, shortcuts, and uninstallation.

#### Specifications
- **Windows Installer:**
  - MSI or EXE installer (using WiX or Inno Setup)
  - Install to Program Files
  - Create Start Menu shortcut
  - Create Desktop shortcut (optional)
  - Add to Windows Apps list
  - Uninstaller included
  - Check for .NET runtime (install if needed)

- **macOS Installer:**
  - DMG disk image
  - Drag-and-drop to Applications folder
  - Optional: PKG installer for automated deployment
  - Code signing (recommended)
  - Notarization (for distribution)

- **Linux Installer:**
  - AppImage (no installation needed)
  - Snap package:
    - `sudo snap install derot-my-brain`
    - Auto-updates
  - Flatpak package (optional):
    - Flathub distribution
  - .deb package for Debian/Ubuntu (optional)
  - .rpm package for Fedora/RHEL (optional)

- **Installer Features:**
  - License agreement display
  - Installation directory selection
  - Shortcut creation options
  - Launch on startup option (optional)
  - Uninstaller that removes all data (with confirmation)

#### Acceptance Criteria
- [ ] Windows installer creates shortcuts and uninstaller
- [ ] macOS DMG allows drag-and-drop installation
- [ ] Linux AppImage runs without installation
- [ ] Installers handle upgrades gracefully
- [ ] Uninstallers remove application completely
- [ ] User data preserved during upgrades
- [ ] Installation process is intuitive for non-technical users
- [ ] No terminal commands required for any platform

---

### Task 9.3: User Documentation & Distribution
**Priority:** MEDIUM  
**Estimated Complexity:** Low  
**Dependencies:** Task 9.2

#### Objective
Create comprehensive, non-technical documentation for end users and set up distribution channels.

#### Specifications
- **User Documentation:**
  - **Installation Guide:**
    - Step-by-step with screenshots
    - Platform-specific instructions (Windows/macOS/Linux)
    - Troubleshooting common issues
    - System requirements
  
  - **Quick Start Guide:**
    - First-time setup
    - Creating first user
    - Taking first quiz
    - Understanding the interface
  
  - **User Manual:**
    - All features explained
    - Preferences configuration
    - LLM setup and configuration
    - Backlog and history management
    - Data export
  
  - **Troubleshooting Guide:**
    - LLM connection issues
    - Port conflicts
    - Data location
    - Performance issues
    - How to reset application

- **Distribution:**
  - GitHub Releases:
    - Release notes for each version
    - Download links for all platforms
    - Checksums for verification
  
  - Optional Distribution Channels:
    - Microsoft Store (Windows)
    - Mac App Store (macOS)
    - Flathub (Linux)
    - Snap Store (Linux)

- **Documentation Format:**
  - Markdown files in `/docs/user-guide/`
  - PDF versions for offline reading
  - In-app help links to online documentation
  - Video tutorials (optional)

#### Acceptance Criteria
- [ ] Installation guide available for all platforms
- [ ] Quick start guide helps new users get started
- [ ] User manual covers all features
- [ ] Troubleshooting guide addresses common issues
- [ ] Documentation uses simple, non-technical language
- [ ] Screenshots and visuals included
- [ ] GitHub Releases set up with download links
- [ ] Release notes document changes and new features
- [ ] Documentation accessible from within application

---

## Implementation Priority Order

### Phase -1: Frontend Architecture Migration (Pre-Sprint 0)
**⚠️ ABSOLUTE FIRST PRIORITY - MUST BE DONE BEFORE EVERYTHING ELSE**

1. **Task -1.1: Infrastructure Layer Setup** - CRITICAL
2. **Task -1.2: Zustand State Management Setup** - CRITICAL
3. **Task -1.3: Custom Hooks Implementation** - CRITICAL
4. **Task -1.4: Component Refactoring** - CRITICAL
5. **Task -1.5: Context Cleanup & Verification** - HIGH

**Estimated Duration:** 2-3 weeks  
**Rationale:** Current codebase violates architecture principles. Must be fixed before building new features.

---

### Sprint 0: Foundation (Pre-Sprint 1)
**⚠️ MUST BE DONE AFTER PHASE -1**
6. **Task 0.1: Application Initialization & Configuration** - CRITICAL

### Sprint 1: Core Infrastructure (Week 1)
7. Task 8.1: Internationalization (i18n) - **DO THIS FIRST** to avoid refactoring
8. Task 1.1: Session Persistence
9. Task 2.1: Extend User Model
10. Task 4.1: Main Navigation Menu (including header auth state)

### Sprint 2: User Experience (Week 2)
11. Task 1.2: Welcome Page
12. Task 2.2: User Preferences Page
13. Task 2.3: LLM Configuration UI
14. Task 4.2: User Profile Page (including account deletion)
15. Task 8.2: Category Preferences Management

### Sprint 3: Activity Enhancements (Week 3)
16. Task 3.1: Enhanced Activity History Model
17. Task 3.2: Enhanced History View UI
18. Task 3.3: Activity Statistics & Calendar View
19. Task 4.3: Backlog Page
20. Task 8.4: Enhanced History and Backlog Actions

### Sprint 4: Core Functionality (Week 4-5)
21. Task 5.1: Derot Page - Wikipedia Integration
22. Task 5.2: Derot Page - Quiz Generation
23. Task 5.3: LLM Resource Estimation & Monitoring
24. Task 8.3: Category Filtering on Derot Page

### Sprint 5: Polish & Export (Week 6)
25. Task 6.1: User Data Export
26. Task 7.1: Contextual Help & Tooltips
27. Task 8.5: Date Format Preferences

### Sprint 6: Deployment (Week 7-8)
28. Task 9.1: Cross-Platform Packaging
29. Task 9.2: Installer Creation
30. Task 9.3: User Documentation & Distribution

---

## Summary of New Features (Phase 8)

### Internationalization (Task 8.1)
- ✅ All text in translation resource files (en.json, fr.json)
- ✅ Language selector in preferences
- ✅ Auto-detection of browser language
- ✅ Full support for English and French

### Interest Profiles (Task 8.2)
- ✅ Create named collections of Wikipedia categories
- ✅ Manage profiles from Preferences page
- ✅ Edit, delete, and organize profiles
- ✅ New users start with no profiles (optional feature)

### Profile Filtering on Derot (Task 8.3)
- ✅ Dropdown to select interest profile
- ✅ On-the-fly category modifications (temporary)
- ✅ Clear UX for temporary vs. saved changes
- ✅ Save icon (💾) with confirmation modal
- ✅ Filter disabled when reworking from backlog/history
- ✅ Filter re-enabled after "Recycle" click
- ✅ Reset filter button

### Enhanced Actions (Task 8.4)
- ✅ "Rework Topic" button in history and backlog
- ✅ Clickable book icon (📖) to toggle backlog status
- ✅ Trash icon (🗑️) with confirmation modal in backlog
- ✅ Visual feedback on all actions

---

## Notes for Agents

### When Implementing Features:
1. **Read the Roadmap**: Check `Implementation-Roadmap.md` for detailed specifications
2. **Follow TDD (CRITICAL)**: 
   - Write tests FIRST before any implementation code
   - Red → Green → Refactor cycle
   - Minimum 80% code coverage
3. **Create TestUser Mock Data (CRITICAL)**:
   - Every feature MUST include mock data for "TestUser"
   - Use existing TestUser ID from `/data/users/users.json`
   - Cover edge cases and realistic scenarios
   - Document mock data structure
4. **Follow SOLID Principles**: Especially in backend code
5. **Follow Frontend Architecture Principles**: See `Docs/frontend_guidelines.md` for:
   - Separation of Concerns
   - Component-driven architecture
   - Composition over inheritance
   - Unidirectional data flow
   - Custom Hooks for business logic
   - Clean Architecture / Hexagonal (adapted for frontend)
   - Use Zustand for state management
6. **Use Existing Components**: Leverage shadcn/ui and existing theme system
7. **Test Thoroughly**: 
   - Backend: Unit tests for services, integration tests for APIs
   - Frontend: Component tests for complex UI
   - All tests must pass before considering task complete
8. **Update Documentation**: Mark tasks as complete in this file and roadmap
9. **Use TestUser**: For all automated testing and mock data creation

### Code Standards:
- **Backend:** C# naming conventions, XML documentation comments
- **Frontend:** React/TypeScript best practices, JSDoc comments, architecture principles (see `Docs/frontend_guidelines.md`)
- **Styling:** Use theme system, ensure mobile responsiveness
- **Error Handling:** Consistent patterns, user-friendly messages

### Testing Guidelines (CRITICAL):
- **TDD Mandatory**: Write tests before implementation
- **Backend:** 
  - Unit tests for all services (xUnit/NUnit)
  - Integration tests for all API endpoints
  - Mock external dependencies
- **Frontend:** 
  - Component tests for complex components (Vitest + React Testing Library)
  - Test user interactions and edge cases
- **Manual Testing:** Test on multiple browsers and screen sizes
- **Mock Data:** Always use "TestUser" for automated testing
- **Coverage:** Minimum 80% code coverage for new features
- **CI/CD:** All tests must pass in pipeline before merge

### TestUser Mock Data Requirements:
- **Location:** `/data/users/` directory
- **Files:**
  - `users.json` - TestUser profile and preferences
  - `user-{testuser-id}-history.json` - Activity history
  - `user-{testuser-id}-backlog.json` - Backlog items
- **Quality:**
  - Realistic and representative data
  - Cover edge cases (empty, full, maximum values)
  - Include timestamps and proper formatting
  - Document data structure in task acceptance criteria

### Documentation Updates
- Update `Project-Status.md` after completing each task
- Update `Specifications-fonctionnelles.md` if requirements change
- Document any new API endpoints in `API-endpoints.md`
- Update `Guide-Compilation-Execution.md` if build process changes

### Code Standards
- Backend: Follow C# naming conventions
- Frontend: Follow React/TypeScript best practices and architecture principles (see `Docs/frontend_guidelines.md`)
- Use existing components from shadcn/ui
- Maintain consistent error handling patterns
- Add JSDoc/XML comments for public APIs
