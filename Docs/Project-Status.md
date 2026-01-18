# Project Status and Roadmap

## Overview
This document tracks the implementation status of features defined in the Functional Specifications and the feature bucket list. It provides a high-level view of what's completed, in progress, and planned.

**Last Updated:** 2026-01-18

---

## Current Implementation Status

### ✅ Completed Features

#### 1. User Management (Basic)
- [x] **User Identification**: Simple login screen asking for a name
- [x] **Profile Selection**: Display list of existing users
- [x] **User Model with ID**: Users have unique IDs (GUID-based)
- [x] **History & Progress**: Basic tracking of user activities (Full stack implemented)

#### 2. User Interface (UI/UX)
- [x] **Project Structure**: Frontend (React/Vite) and Backend (.NET) initialized
- [x] **Theme System**: Dynamic switching between 5 themes (Curiosity Loop, Derot Brain, Knowledge Core, Mind Lab, Neo-Wikipedia)
- [x] **Theme Persistence**: Save user preference in localStorage
- [x] **Basic Layout**: Header and main content area

#### 3. Backend Infrastructure
- [x] **ASP.NET Core Web API**: Backend initialized
- [x] **JSON File Storage**: Repository pattern for user data
- [x] **User Service**: CRUD operations for users
- [x] **User Activity Model**: Basic activity tracking structure
- [x] **CORS Configuration**: Frontend-backend communication enabled

---

## 🚧 In Progress / Planned Features

### Phase -1: Frontend Architecture Migration (Priority: CRITICAL)

> [!CAUTION]
> **HIGHEST PRIORITY**: This phase MUST be completed BEFORE Phase 0 and all other features.

#### Architecture Compliance
- [ ] **Task -1.1: Infrastructure Layer Setup**
  - Create `/api` directory structure
  - Centralized HTTP client (axios)
  - Move UserService to userApi.ts
  - Remove axios/fetch mixing
  - **Status:** Not Started
  - **Roadmap Task:** -1.1
  - **⚠️ CRITICAL:** Foundation for all other work

- [ ] **Task -1.2: Zustand State Management Setup**
  - Install Zustand
  - Create `useAuthStore` (authentication state)
  - Create `usePreferencesStore` (user preferences)
  - Implement localStorage persistence
  - **Status:** Not Started
  - **Roadmap Task:** -1.2
  - **Dependencies:** Task -1.1

- [ ] **Task -1.3: Custom Hooks Implementation**
  - Create `useAuth()` hook
  - Create `useUser()` hook
  - Create `usePreferences()` hook
  - Create `useHistory()` hook
  - Enhance `useCategories()` hook
  - **Status:** Not Started
  - **Roadmap Task:** -1.3
  - **Dependencies:** Task -1.2

- [ ] **Task -1.4: Component Refactoring**
  - Refactor `App.tsx` (166 → ~80 lines)
  - Refactor `UserPreferencesPage.tsx` (487 → ~200 lines)
  - Refactor `UserSelectionPage.tsx` (124 → ~80 lines)
  - Refactor `history-view.tsx` (83 → ~50 lines)
  - Extract sub-components
  - **Status:** Not Started
  - **Roadmap Task:** -1.4
  - **Dependencies:** Task -1.3

- [ ] **Task -1.5: Context Cleanup & Verification**
  - Evaluate/simplify UserContext
  - Run verification checklist
  - Manual testing of all features
  - Update documentation
  - **Status:** Not Started
  - **Roadmap Task:** -1.5
  - **Dependencies:** Task -1.4

**See detailed plan:** `implementation_plan.md` in artifacts

---

### Phase 0: Application Foundation (Priority: CRITICAL)

**⚠️ MUST BE DONE AFTER PHASE -1 (Architecture Migration)**

#### Application Initialization
- [x] **Application Initialization & Configuration**: Seed data and global config
  - Initialize Wikipedia categories (13 official categories)
  - Initialize themes (5 color palettes)
  - Global configuration management (LLM URL/Port)
  - Seed data stored in `/data/seed/`
  - Configuration stored in `/data/config/`
  - **Status:** Completed
  - **Completed Date:** 2026-01-18
  - **Roadmap Task:** 0.1
  - **⚠️ CRITICAL:** MUST be done FIRST before any other task

---

### Phase 1: Core Infrastructure & UX Enhancements (Priority: HIGH)

#### Session & Authentication
- [x] **Session Persistence**: Keep user logged in after page refresh
  - Store session in localStorage/sessionStorage
  - Validate session on app initialization
  - Redirect to login only if session invalid
  - **Status:** Completed
  - **Completed Date:** 2026-01-18
  - **Roadmap Task:** 1.1

#### Welcome & Onboarding
- [x] **Welcome Page for New Users**: First-time user onboarding
  - Show guide explaining app purpose and features
  - Options: Read Guide / Proceed / Don't Show Again
  - Store preference in localStorage
  - **Status:** Completed
  - **Completed Date:** 2026-01-18
  - **Roadmap Task:** 1.2

#### Backend Logging
- [x] **Backend Logging System**: File-based logging (Serilog/NLog)
  - Logs stored in `/Logs` at API root
  - Daily log rotation
  - Capture exceptions and startup events
  - **Status:** Completed
  - **Completed Date:** 2026-01-18
  - **Roadmap Task:** 1.3

---

### Phase 2: User Data Model Enhancements (Priority: HIGH)

#### User Preferences
- [x] **Extended User Model**: Add preferences and metadata
  - Question count preference (5/10/15/20)
  - Last connection date tracking
  - Theme preference (move from localStorage to user data)
  - **Status:** Completed
  - **Completed Date:** 2026-01-18
  - **Roadmap Task:** 2.1

- [ ] **User Preferences Page**: Dedicated settings page
  - Configure quiz question count
  - Select theme
  - Future: difficulty, language, etc.
  - **Status:** Not Started
  - **Roadmap Task:** 2.2

- [ ] **LLM Configuration UI**: LLM setup in preferences
  - Configure LLM URL and port
  - Test LLM connectivity
  - Success/error feedback modal
  - **Status:** Not Started
  - **Roadmap Task:** 2.3
  - **Dependencies:** Task 0.1

---

### Phase 3: Activity History Enhancements (Priority: HIGH)

#### Enhanced Activity Tracking
- [ ] **Enhanced Activity Model**: Track more detailed information
  - Last score AND best score for each activity
  - LLM model information (name, version)
  - Notation format: X/totalQuestions (with percentage)
  - Backlog status flag
  - **Status:** Not Started
  - **Roadmap Task:** 3.1

- [ ] **Enhanced History View**: Improved UI for activity history
  - Display last score and best score
  - Show LLM used (on hover or in details)
  - Backlog indicator (book icon 📖)
  - Add to backlog from history
  - Sorting and filtering options
  - **Status:** Not Started
  - **Roadmap Task:** 3.2

- [ ] **Activity Statistics & Calendar View**: Enhanced statistics panel
  - GitLab-style activity calendar
  - Best personal score display
  - Last activity with article name
  - Total activities count
  - **Status:** Not Started
  - **Roadmap Task:** 3.3
  - **Dependencies:** Task 3.1, Task 3.2

---

### Phase 4: Navigation & Page Structure (Priority: HIGH)

#### Navigation System
- [ ] **Main Navigation Menu**: App-wide navigation
  - Links to: Derot, History, Backlog, Profile, Preferences, Guide, Logout
  - Responsive design (sidebar/hamburger menu)
  - Active page highlighting
  - **Header authentication state changes:**
    - Not authenticated: Language + Theme selectors
    - Authenticated: User menu dropdown + Settings button + Logout
  - Logo clickable → home (history for authenticated users)
  - **Status:** Not Started
  - **Roadmap Task:** 4.1

#### User Pages
- [ ] **User Profile Page**: Display and edit user information
  - Show: name, ID, creation date, last connection, statistics
  - Edit name functionality
  - **Account deletion with confirmation modal**
  - **Status:** Not Started
  - **Roadmap Task:** 4.2

- [ ] **Backlog Page**: Dedicated page for saved articles
  - List all backlog items
  - Start quiz from backlog
  - Remove items from backlog
  - Show last attempt and best score
  - **Status:** Not Started
  - **Roadmap Task:** 4.3

---

### Phase 5: Core Functionality - Derot Page (Priority: HIGH)

#### Wikipedia Integration
- [ ] **Wikipedia Service**: Fetch and display articles
  - Random article fetching
  - Article by category/interest
  - Content parsing and cleaning
  - **Status:** Not Started
  - **Roadmap Task:** 5.1

- [ ] **Derot Page - Reading Experience**:
  - Display Wikipedia article
  - "Recycle" button (new article without saving)
  - "Add to Backlog" button
  - Quick access to history/backlog (modal/drawer)
  - Preserve article state during navigation
  - **Status:** Not Started
  - **Roadmap Task:** 5.1

#### Quiz Generation & Evaluation
- [ ] **LLM Integration**: Ollama/OpenAI for quiz generation
  - Generate questions based on article
  - Configurable question count (from user preferences)
  - Semantic answer evaluation
  - Store LLM model information
  - **Status:** Not Started
  - **Roadmap Task:** 5.2

- [ ] **Quiz Experience**:
  - Display questions (configurable count: 5/10/15/20)
  - Text input for answers
  - Submit and evaluate
  - Show results with user/expected answers
  - Save to history only after at least one answer submitted
  - **Status:** Not Started
  - **Roadmap Task:** 5.2

- [ ] **LLM Resource Estimation**: Resource monitoring and warnings
  - Estimate CPU/RAM based on article size
  - Display processing time estimates
  - Warning levels for large articles
  - Performance tips
  - **Status:** Not Started
  - **Roadmap Task:** 5.3
  - **Dependencies:** Task 5.1, Task 5.2

---

### Phase 6: Data Management (Priority: LOW)

#### User Export
- [ ] **User Data Export**: Export user data to JSON
  - Export profile, preferences, backlog
  - Optional: include full history
  - Downloadable JSON file
  - **Status:** Not Started
  - **Roadmap Task:** 6.1

---

### Phase 7: User Guidance (Priority: LOW)

#### Contextual Help
- [ ] **Tooltips & Help**: Guide users throughout the app
  - Tooltips on all interactive elements
  - Help icons for complex features
  - "Did you know?" tips
  - **Status:** Not Started
  - **Roadmap Task:** 7.1

---

### Phase 8: Internationalization & Category Preferences (Priority: HIGH)

#### Internationalization (i18n)
- [x] **Task 8.1: Internationalization (i18n) Implementation**: Complete translation system
  - All UI text in resource files (en.json, fr.json)
  - Language selector in preferences
  - Auto-detection of browser language
  - Support for English and French
  - **Status:** Completed ✅
  - **Completed on:** 2026-01-18
  - **Roadmap Task:** 8.1

#### Category Preferences (Simplified - No Named Profiles)
- [x] **Task 8.2: Category Preferences Management**: Simple checklist in preferences
  - **Status:** Completed ✅
  - **Completed on:** 2026-01-18
  - **Description**: Users can select which Wikipedia categories they are interested in.
  - **Dependencies**: Task 2.1
  - **Changes**:
    - [UserPreferencesPage.tsx] Added category selection UI
    - [UserService.cs] Initialize new users with all categories
    - [User.cs] Added `SelectedCategories` property
  - 13 official Wikipedia categories
  - All categories checked by default for new users
  - Manage from Preferences page
  - "Select All" / "Deselect All" buttons
  - **Roadmap Task:** 8.2
  - **✨ SIMPLIFIED:** No named profiles, just category checkboxes

- [ ] **Category Filtering on Derot Page**: Filter articles by categories
  - Category checkboxes loaded from user preferences
  - Temporary modifications possible (not saved unless confirmed)
  - Clear UX for temporary vs. saved changes
  - "Save to Preferences" button with confirmation modal
  - Filter disabled when reworking from backlog/history
  - "Reset" button unchecks all categories
  - "Recycle" button unchecks all categories (complete reset)
  - **Status:** Not Started
  - **Roadmap Task:** 8.3
  - **✨ SIMPLIFIED:** No profile dropdown, direct category selection

- [ ] **Enhanced History and Backlog Actions**: Improved interactions
  - "Rework Topic" button in history and backlog
  - Clickable book icon (📖) to toggle backlog status
  - Trash icon (🗑️) with confirmation in backlog
  - Visual feedback on all actions
  - **Status:** Not Started
  - **Roadmap Task:** 8.4

#### Date Formatting
- [ ] **Date Format Preferences**: User-selected date format (Not Persisted)
  - Options: French (dd/MM/yyyy), American (MM/dd/yyyy)
  - Dropdown in preferences
  - **Status:** Not Started
  - **Roadmap Task:** 8.5
  - **Note:** Choice is transient/session-based, does not affect backend data.

---

### Phase 9: Deployment & Distribution (Priority: MEDIUM)

#### Cross-Platform Packaging
- [ ] **Cross-Platform Packaging**: Self-contained application bundles
  - Windows, macOS, Linux support
  - No .NET installation required
  - Embedded backend + frontend
  - Self-contained deployment
  - **Status:** Not Started
  - **Roadmap Task:** 9.1
  - **Dependencies:** Phase 5 (core features)

#### Installer Creation
- [ ] **Installer Creation**: User-friendly installers
  - Windows: MSI/EXE installer
  - macOS: DMG disk image
  - Linux: AppImage/Snap/Flatpak
  - Shortcuts and uninstallers
  - **Status:** Not Started
  - **Roadmap Task:** 9.2
  - **Dependencies:** Task 9.1

#### User Documentation
- [ ] **User Documentation & Distribution**: End-user guides
  - Installation guide (all platforms)
  - Quick start guide
  - User manual
  - Troubleshooting guide
  - GitHub Releases setup
  - **Status:** Not Started
  - **Roadmap Task:** 9.3
  - **Dependencies:** Task 9.2

## Feature Bucket List Status (Updated)

### ✅ Addressed in Roadmap
**Original Bucket List:**
- [x] Session persistence on page refresh
- [x] Welcome page for new users
- [x] Activity history with last and best scores
- [x] LLM information tracking
- [x] Notation format (X/Y with percentage)
- [x] Configurable question count (5/10/15/20)
- [x] User preferences storage
- [x] Navigation menu
- [x] User profile page
- [x] User preferences page
- [x] User history page (enhanced)
- [x] User backlog page
- [x] Backlog indicators in history
- [x] Derot page (Wikipedia + Quiz)
- [x] Quick access to history/backlog from Derot page
- [x] Add current article to backlog
- [x] Save to history only after answer submission
- [x] User data export feature
- [x] Contextual help and tooltips

**Second Bucket List (Phase 8):**
- [x] All text in translation resources (i18n - English + French)
- [x] User can create "Interest Profiles" (category collections)
- [x] Profile dropdown on Derot page with filtering
- [x] On-the-fly category modifications (temporary, not persisted)
- [x] Save icon (💾) with confirmation modal for profile changes
- [x] Filter disabled when reworking from backlog/history
- [x] Filter re-enabled after "Recycle" click
- [x] Reset filter button during new activity
- [x] "Rework Topic" button in history
- [x] Clickable book icon (📖) to toggle backlog in history
- [x] "Rework Topic" button in backlog
- [x] Trash icon (🗑️) with confirmation modal in backlog
- [x] Clear UX for temporary vs. persistent changes

**Third Bucket List (bucket-list.md - 2026-01-18):**
- [x] User profile with editable display name (Task 4.2)
- [x] Unique user ID preserved on name change (already implemented)
- [x] Account deletion with confirmation modal (Task 4.2)
- [x] LLM configuration UI in preferences (Task 2.3)
- [x] LLM URL/port configuration with validation (Task 2.3)
- [x] Activity calendar (GitLab-style) in history (Task 3.3)
- [x] Best personal score display (Task 3.3)
- [x] Header authentication state changes (Task 4.1)
- [x] User menu dropdown when authenticated (Task 4.1)
- [x] Settings button in header (Task 4.1)
- [x] Logo clickable → home (Task 4.1)
- [x] Authenticated home = history page (Task 4.1)
- [x] LLM resource estimation (CPU/RAM) (Task 5.3)
- [x] Processing time estimates (Task 5.3)
- [x] Cross-platform deployment (Windows/macOS/Linux) (Phase 9)
- [x] No terminal commands required for end users (Phase 9)
- [x] User-friendly installers (Phase 9)

### 📋 Implementation Details
All features from the bucket list have been broken down into specific tasks in the **Implementation-Roadmap.md** document. Each task includes:
- Clear objectives
- Detailed specifications
- Acceptance criteria
- Dependencies
- Priority and complexity ratings

---

## Technology Stack

### Frontend
- **Framework:** React 18 + TypeScript
- **Build Tool:** Vite
- **UI Library:** shadcn/ui (Radix UI primitives)
- **Styling:** Tailwind CSS
- **State Management:** Zustand (recommended over Redux for this app's complexity)
- **Routing:** React Router (to be added)
- **Internationalization:** react-i18next

### Backend
- **Framework:** ASP.NET Core 9.0 Web API
- **Language:** C# 13
- **Storage:** JSON files (no SQL database)
- **Architecture:** Repository pattern, Service layer

### AI/LLM (Planned)
- **Primary:** Ollama (local LLM)
- **Models:** llama3:8b, qwen2.5:7b, mistral:7b
- **Fallback:** OpenAI API (optional)

### Development Tools
- **Version Control:** Git
- **IDE:** Visual Studio / VS Code
- **Package Managers:** npm (frontend), NuGet (backend)

---

## Implementation Timeline

### Sprint 1: Core Infrastructure (Week 1)
- Session Persistence
- Extended User Model
- Main Navigation Menu

### Sprint 2: User Experience (Week 2)
- Welcome Page
- User Preferences Page
- User Profile Page

### Sprint 3: Activity Enhancements (Week 3)
- Enhanced Activity History Model
- Enhanced History View UI
- Backlog Page

### Sprint 4: Core Functionality (Week 4-5)
- Wikipedia Integration
- Derot Page Implementation
- Quiz Generation & Evaluation

### Sprint 5: Polish & Export (Week 6)
- User Data Export
- Contextual Help & Tooltips
- Date Format Preferences
- Final testing and bug fixes

### Sprint 6: Deployment (Week 7-8)
- Cross-Platform Packaging
- Installer Creation
- User Documentation & Distribution

---

## Next Steps

1. **Review Implementation Roadmap**: See `Implementation-Roadmap.md` for detailed task breakdowns
2. **Start with Sprint 1**: Begin with session persistence and navigation
3. **Iterative Development**: Complete one task at a time, test thoroughly
4. **Update Documentation**: Keep this file and roadmap updated as features are completed

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
7. **Test Thoroughly**: Write unit tests for backend, component tests for complex UI
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
  - Document data structure in acceptance criteria

---

## Related Documentation

- **[Implementation-Roadmap.md](file:///d:/Repos/Derot-my-brain/Docs/Implementation-Roadmap.md)**: Detailed task breakdowns with specifications
- **[Specifications-fonctionnelles.md](file:///d:/Repos/Derot-my-brain/Docs/Specifications-fonctionnelles.md)**: Original functional specifications
- **[Guide-Compilation-Execution.md](file:///d:/Repos/Derot-my-brain/Docs/Guide-Compilation-Execution.md)**: How to build and run the project
- **[ArchitectureDiagram.md](file:///d:/Repos/Derot-my-brain/Docs/ArchitectureDiagram.md)**: System architecture overview
