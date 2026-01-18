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
    └── user-{id}-backlog.json    # Backlog per user
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

## Phase 0: Application Foundation & Initialization

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
- [ ] Seed data (categories + themes) initialized on first startup
- [ ] 13 Wikipedia categories available via API
- [ ] 5 themes available via API
- [ ] Global configuration created with default LLM URL
- [ ] LLM configuration can be retrieved via API
- [ ] LLM configuration can be updated via API
- [ ] Initialization is idempotent (can run multiple times safely)
- [ ] Seed data is immutable (cannot be deleted/modified by users)
- [ ] Frontend can fetch and display LLM configuration
- [ ] Error handling for missing/corrupted seed data

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
- [ ] User remains logged in after page refresh
- [ ] Invalid/deleted users are redirected to login
- [ ] Session persists across browser tabs
- [ ] Explicit logout clears session properly

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
- [ ] New users see welcome page on first visit
- [ ] "Don't Show Again" option persists preference
- [ ] Guide is clear, concise, and beginner-friendly
- [ ] Welcome page respects current theme
- [ ] Users can access guide later from settings/help menu

---

## Phase 2: User Data Model Enhancements

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
- [ ] User model includes preferences and last connection date
- [ ] Last connection updates on each login
- [ ] Preferences can be retrieved and updated via API
- [ ] Existing users migrated with default preferences
- [ ] Frontend state includes user preferences

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
      - Question count selector: Radio buttons or dropdown for 5/10/15/20
      - Visual indication of current selection
    - **Theme Settings:**
      - Theme selector (reuse existing `ThemeSelector` component)
    - **Future Settings Placeholder:**
      - Section for difficulty, language, etc.
  - Add "Save" and "Cancel" buttons
  - Show success/error notifications on save
  - Add navigation link in main menu

- **API Integration:**
  - Call `PUT /api/users/{id}/preferences` on save
  - Optimistic UI updates with rollback on error

#### Acceptance Criteria
- [ ] Users can select question count (5/10/15/20)
- [ ] Preferences save successfully to backend
- [ ] UI updates immediately on save
- [ ] Error handling for failed saves
- [ ] Page accessible from navigation menu

---

## Phase 3: Activity History Enhancements

### Task 3.1: Enhanced Activity History Model
**Priority:** HIGH  
**Estimated Complexity:** Medium  
**Dependencies:** None

#### Objective
Enhance the activity history to track LLM information, best scores, and backlog status.

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
        public bool IsInBacklog { get; set; }
    }
    
    public class LLMInfo
    {
        public string ModelName { get; set; } // e.g., "llama3:8b"
        public string Version { get; set; } // e.g., "v1.0"
    }
    ```
  - Update `UserService.cs` to:
    - Track best score across all attempts
    - Update last attempt date on each quiz completion
    - Store LLM information with each activity
  - Add migration logic for existing activities

- **Frontend:**
  - Update `UserActivity` TypeScript interface
  - Update history display to show:
    - Last score: `X/Y` (e.g., 7/10)
    - Best score: `X/Y` (e.g., 9/10)
    - Percentage calculated as: `(score / totalQuestions) * 100`
    - LLM used (shown on hover or in details)
    - Backlog indicator (book icon)

#### Acceptance Criteria
- [ ] Activities track both last and best scores
- [ ] Notation format is `X/totalQuestions` with percentage
- [ ] LLM information stored with each activity
- [ ] Backlog status tracked per activity
- [ ] Existing activities migrated successfully

---

### Task 3.2: Enhanced History View UI
**Priority:** MEDIUM  
**Estimated Complexity:** Medium  
**Dependencies:** Task 3.1

#### Objective
Update the history view to display enhanced activity information with backlog indicators.

#### Specifications
- **Frontend:**
  - Update `history-view.tsx` component:
    - Display activities in a grid/table format
    - Columns:
      - Topic (clickable to restart activity)
      - First Attempt Date
      - Last Attempt Date
      - Last Score (X/Y - Z%)
      - Best Score (X/Y - Z%)
      - LLM Used (tooltip on hover)
      - Backlog Status (book icon if in backlog)
      - Actions (Add to Backlog button if not already)
  - Add visual indicator for backlog items (e.g., 📖 icon)
  - Add sorting options (by date, score, topic)
  - Add filtering options (all, backlog only, not in backlog)

- **Interactions:**
  - Click topic to restart quiz on that article
  - Click "Add to Backlog" to add activity
  - Hover over LLM info to see model details

#### Acceptance Criteria
- [ ] History displays all enhanced information
- [ ] Backlog items clearly indicated with icon
- [ ] Scores shown in X/Y format with percentage
- [ ] LLM information visible on hover
- [ ] Users can add activities to backlog from history
- [ ] Sorting and filtering work correctly

---

## Phase 4: Navigation & Page Structure

### Task 4.1: Main Navigation Menu
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

#### Acceptance Criteria
- [ ] Navigation menu accessible from all pages
- [ ] All pages reachable via navigation
- [ ] Active page highlighted in menu
- [ ] Navigation responsive on mobile
- [ ] Logout functionality works correctly

---

### Task 4.2: User Profile Page
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

- **Backend:**
  - Add endpoint: `PUT /api/users/{id}` to update user name
  - Add validation for user updates

#### Acceptance Criteria
- [ ] Profile displays all user information
- [ ] User can edit their name
- [ ] Changes save successfully
- [ ] Validation prevents invalid names
- [ ] Statistics (activity count, backlog count) accurate

---

### Task 4.3: Backlog Page
**Priority:** HIGH  
**Estimated Complexity:** Medium  
**Dependencies:** Task 3.1

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

## Phase 5: Derot Page (Main Quiz Experience)

### Task 5.1: Derot Page - Wikipedia Integration
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
- [ ] All UI text comes from translation files (no hardcoded strings)
- [ ] Users can switch between English and French
- [ ] Language preference persists across sessions
- [ ] Browser language auto-detected on first visit
- [ ] All pages and components fully translated
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
- [ ] Categories loaded from seed data via API
- [ ] User preferences include selected categories list (IDs)
- [ ] New users have ALL categories selected by default
- [ ] Users can check/uncheck categories in Preferences page
- [ ] At least one category must remain selected
- [ ] "Select All" and "Deselect All" buttons work correctly
- [ ] Changes save successfully to backend
- [ ] Category selection persists across sessions
- [ ] Category names displayed in correct language (EN/FR)
- [ ] Validation prevents selecting non-existent categories

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

## Implementation Priority Order

### Sprint 0: Foundation (Pre-Sprint 1)
**⚠️ MUST BE DONE FIRST BEFORE ANY OTHER TASK**
1. **Task 0.1: Application Initialization & Configuration** - CRITICAL

### Sprint 1: Core Infrastructure (Week 1)
2. Task 8.1: Internationalization (i18n) - **DO THIS FIRST** to avoid refactoring
3. Task 1.1: Session Persistence
4. Task 2.1: Extend User Model
5. Task 4.1: Main Navigation Menu

### Sprint 2: User Experience (Week 2)
6. Task 1.2: Welcome Page
7. Task 2.2: User Preferences Page
8. Task 4.2: User Profile Page
9. Task 8.2: Category Preferences Management

### Sprint 3: Activity Enhancements (Week 3)
10. Task 3.1: Enhanced Activity History Model
11. Task 3.2: Enhanced History View UI
12. Task 4.3: Backlog Page
13. Task 8.4: Enhanced History and Backlog Actions

### Sprint 4: Core Functionality (Week 4-5)
14. Task 5.1: Derot Page - Wikipedia Integration
15. Task 5.2: Derot Page - Quiz Generation
16. Task 8.3: Category Filtering on Derot Page

### Sprint 5: Polish & Export (Week 6)
17. Task 6.1: User Data Export
18. Task 7.1: Contextual Help & Tooltips

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

### General Guidelines
- Each task should be implemented in a separate branch
- Follow SOLID principles for backend code
- Use existing theme system for all UI components
- Ensure mobile responsiveness for all pages
- Write unit tests for backend services
- Use TypeScript strict mode for frontend code

### Testing Requirements
- Backend: Unit tests for all services
- Frontend: Component tests for complex components
- Integration tests for API endpoints
- Manual testing checklist for each task

### Documentation Updates
- Update `Project-Status.md` after completing each task
- Update `Specifications-fonctionnelles.md` if requirements change
- Document any new API endpoints in `API-endpoints.md`
- Update `Guide-Compilation-Execution.md` if build process changes

### Code Standards
- Backend: Follow C# naming conventions
- Frontend: Follow React/TypeScript best practices
- Use existing components from shadcn/ui
- Maintain consistent error handling patterns
- Add JSDoc/XML comments for public APIs
