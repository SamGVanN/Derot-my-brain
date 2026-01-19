# Project Status and Roadmap

## Overview
This document tracks the implementation status of features defined in the Functional Specifications and the feature bucket list. It provides a high-level view of what's completed, in progress, and planned.

**Last Updated:** 2026-01-19

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
- [x] **Logging**: Serilog integrated

#### 4. Testing & Quality Assurance
- [x] **Backend Test Infrastructure**: xUnit + Moq project created (`DerotMyBrain.Tests`)
- [x] **Frontend Test Infrastructure**: Vitest + React Testing Library configured
- [x] **Unit Tests**: Core services (UserService, ConfigurationService) covered >80%
- [x] **Mock Data**: Comprehensive TestUser data created (Backlog, Enhanced History)


---

## 🚧 In Progress / Planned Features

### Phase -1: Frontend Architecture Migration (Priority: CRITICAL)

> [!CAUTION]
> **HIGHEST PRIORITY**: This phase MUST be completed BEFORE Phase 0 and all other features.

#### Architecture Compliance
- [x] **Task -1.1: Infrastructure Layer Setup**
  - **Status:** Completed
  - **Completed Date:** 2026-01-18

- [x] **Task -1.2: Zustand State Management Setup**
  - **Status:** Completed
  - **Completed Date:** 2026-01-18

- [x] **Task -1.3: Custom Hooks Implementation**
  - **Status:** Completed
  - **Completed Date:** 2026-01-18

- [x] **Task -1.4: Component Refactoring**
  - **Status:** Completed
  - **Completed Date:** 2026-01-19

- [x] **Task -1.5: Context Cleanup & Verification**
  - **Status:** Completed
  - **Completed Date:** 2026-01-19

**See detailed plan:** `implementation_plan.md` in artifacts

---

### Phase 0: Application Foundation (Priority: CRITICAL)

**⚠️ MUST BE DONE AFTER PHASE -1 (Architecture Migration)**

#### Application Initialization
- [x] **Task 0.1: Application Initialization & Configuration**
  - **Status:** Completed
  - **Completed Date:** 2026-01-18

---

### Phase 1: Core Infrastructure & UX Enhancements (Priority: HIGH)

#### Session & Authentication
- [x] **Task 1.1: Session Persistence**
  - **Status:** Completed
  - **Completed Date:** 2026-01-18

#### Welcome & Onboarding
- [x] **Task 1.2: Welcome Page for New Users**
  - **Status:** Completed
  - **Completed Date:** 2026-01-18

#### Backend Logging
- [x] **Task 1.3: Backend Logging System**
  - **Status:** Completed
  - **Completed Date:** 2026-01-18

---

### Phase 2: User Preferences & i18n (Priority: HIGH)

#### User Preferences
- [x] **Task 2.1: Extended User Model**
  - **Status:** Completed
  - **Completed Date:** 2026-01-18

- [x] **Task 2.2: User Preferences Page (Cleanup)**
  - [x] Configure quiz question count
  - [x] Select theme
  - [x] Select language
  - [x] Language/theme mismatch indicators working correctly
  - [x] Comprehensive test coverage (41 tests passing)
  - **Status:** Completed ✅
  - **Completed Date:** 2026-01-19
  - **Roadmap Task:** 2.2

- [x] **Task 2.4: Contextual Preference Loading**
  - **Status:** Completed
  - **Completed Date:** 2026-01-18

#### Internationalization & Categories
- [x] **Task 8.1: Internationalization (i18n) Implementation**
  - **Status:** Completed ✅
  - **Completed on:** 2026-01-18

- [x] **Task 8.2: Category Preferences Management**
  - **Status:** Completed ✅
  - **Completed on:** 2026-01-18

---

### Phase 3: Application Structure (Sprint A)

#### Navigation & Profile
- [ ] **Task 3.1: Main Navigation Menu** (Formerly 4.1)
  - **Status:** Not Started
  - Links to: Derot, History, Backlog, Profile, Preferences, Guide, Logout
  - Header authentication state changes

- [ ] **Task 3.2: User Profile Page** (Formerly 4.2)
  - **Status:** Not Started
  - Edit name, view stats
  - Account deletion

---

### Phase 4: Data Infrastructure & LLM (Sprint B)

#### Core Data & Configuration
- [ ] **Task 4.1: LLM Configuration UI** (Formerly 2.3)
  - **Status:** Not Started
  - Configure LLM URL/Port
  - Connection Testing

- [ ] **Task 4.2: Enhanced Activity Model** (Formerly 3.1)
  - **Status:** Not Started
  - Track Last/Best Score
  - Tracked Topic (Backlog) flag
  - LLM Info tracking

---

### Phase 5: Data Views - History & Backlog (Sprint C)

#### Visualization
- [ ] **Task 5.1: Backlog Page** (Formerly 4.3)
  - **Status:** Not Started
  - View and manage saved topics

- [ ] **Task 5.2: Enhanced History View** (Formerly 3.2 + 8.4)
  - **Status:** Not Started
  - Split cards, Last vs Best score
  - Helper actions (Track/Untrack)

- [ ] **Task 5.3: Activity Statistics** (Formerly 3.3)
  - **Status:** Not Started
  - Calendar view, global stats

---

### Phase 6: Core Functionality - Derot Page (Sprint D)

#### The Main Loop
- [ ] **Task 6.1: Wikipedia Integration** (Formerly 5.1)
  - **Status:** Not Started
  - Fetch articles, display content

- [ ] **Task 6.2: Quiz Generation & Evaluation** (Formerly 5.2)
  - **Status:** Not Started
  - LLM Question generation
  - Answer evaluation

- [ ] **Task 6.3: Category Filtering on Derot Page** (Formerly 8.3)
  - **Status:** Not Started
  - Filter by preferences
  - Temporary logic

- [ ] **Task 6.4: LLM Resource Estimation** (Formerly 5.3)
  - **Status:** Not Started
  - Perf monitoring

---

### Phase 7: Data Management (Phase 6)

- [ ] **Task 7.1: User Data Export** (Formerly 6.1)
  - **Status:** Not Started

---

### Phase 8: User Guidance (Phase 7)

- [ ] **Task 8.1: Contextual Help** (Formerly 7.1)
  - **Status:** Not Started

---

### Phase 9: Deployment (Phase 9)

- [ ] **Task 9.1: Cross-Platform Packaging**
- [ ] **Task 9.2: Installer Creation**
- [ ] **Task 9.3: User Documentation**

---

## Feature Bucket List Status (Updated)

### ✅ Feature Status Check
**Core Features:**
- [x] Session persistence
- [x] Welcome page
- [x] User preferences & categories
- [x] i18n support
- [ ] Navigation System (Phase 3)
- [ ] User Profile (Phase 3)
- [ ] LLM Config (Phase 4)
- [ ] Enhanced History/Backlog (Phase 5)
- [ ] Derot Page/Quiz (Phase 6)

**Terminology Update (2026-01-18):**
- **Backlog** renamed to **Tracked Topics** (Sujets Suivis)
- **Favorites** merged into **Tracked Topics**
- **Read** activity defined by scroll-to-bottom OR click "Passer au Quiz"
- **My Brain** menu introduced to group History + Tracked Topics

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
  - `user-{testuser-id}-tracked.json` - Tracked Topics (ex-Backlog) items
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
