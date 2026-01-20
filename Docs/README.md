# Documentation Organization

This document explains the reorganized documentation structure for the Derot My Brain project.

## 📁 Documentation Structure

The documentation is organized into four main folders:

### 📋 Planning/ - Project Management & Tracking

**Purpose:** High-level project planning, status tracking, and functional specifications

- **[Project-Status.md](file:///d:/Repos/Derot-my-brain/Docs/Planning/Project-Status.md)** - Current project state, completed features, sprint tracking
- **[Implementation-Roadmap.md](file:///d:/Repos/Derot-my-brain/Docs/Planning/Implementation-Roadmap.md)** - Detailed task breakdown with specifications and acceptance criteria
- **[Specifications-fonctionnelles.md](file:///d:/Repos/Derot-my-brain/Docs/Planning/Specifications-fonctionnelles.md)** - Complete functional requirements and business logic

### 🏗️ Technical/ - Technical Specifications & Constraints

**Purpose:** All technical guidelines, architectural patterns, and development constraints

- **[Frontend-Architecture.md](file:///d:/Repos/Derot-my-brain/Docs/Technical/Frontend-Architecture.md)** - Frontend development guidelines (theming, React, Zustand, React Router v7)
- **[Backend-Guidelines.md](file:///d:/Repos/Derot-my-brain/Docs/Technical/Backend-Guidelines.md)** - Backend architecture patterns (.NET, SOLID, API design, logging)
- **[Testing-Strategy.md](file:///d:/Repos/Derot-my-brain/Docs/Technical/Testing-Strategy.md)** - TDD methodology, testing standards, coverage requirements
- **[Storage-Policy.md](file:///d:/Repos/Derot-my-brain/Docs/Technical/Storage-Policy.md)** - Storage constraints
- **[Architecture.md](file:///d:/Repos/Derot-my-brain/Docs/Technical/Architecture.md)** - System architecture overview and diagrams

### 👨‍💻 Developer/ - Developer Resources & Guides

**Purpose:** Getting started guides and prompt templates for AI agents

- **[Getting-Started.md](file:///d:/Repos/Derot-my-brain/Docs/Developer/Getting-Started.md)** - How to compile and run the project
- **[Prompt-Templates.md](file:///d:/Repos/Derot-my-brain/Docs/Developer/Prompt-Templates.md)** - Index of available prompt templates
- **Templates/** - Specialized prompt templates for different development scenarios:
  - [Implementation.md](file:///d:/Repos/Derot-my-brain/Docs/Developer/Templates/Implementation.md) - Full feature implementation
  - [Backend.md](file:///d:/Repos/Derot-my-brain/Docs/Developer/Templates/Backend.md) - Backend-only tasks
  - [Frontend.md](file:///d:/Repos/Derot-my-brain/Docs/Developer/Templates/Frontend.md) - Frontend-only tasks
  - [Migration.md](file:///d:/Repos/Derot-my-brain/Docs/Developer/Templates/Migration.md) - Refactoring and migrations
  - [QuickFix.md](file:///d:/Repos/Derot-my-brain/Docs/Developer/Templates/QuickFix.md) - Bug fixes
  - [UI-UX.md](file:///d:/Repos/Derot-my-brain/Docs/Developer/Templates/UI-UX.md) - UI/UX improvements

### 📚 Reference/ - Reference Materials & Templates

**Purpose:** Quick reference for APIs, schemas, themes, and prompts

- **[API-Endpoints.md](file:///d:/Repos/Derot-my-brain/Docs/Reference/API-Endpoints.md)** - API endpoint documentation
- **[JSON-Schemas.md](file:///d:/Repos/Derot-my-brain/Docs/Reference/JSON-Schemas.md)** - **DEPRECATED** - Old JSON file storage schemas (historical reference only)
- **[Color-Palettes.md](file:///d:/Repos/Derot-my-brain/Docs/Reference/Color-Palettes.md)** - Available theme color palettes (5 themes)
- **[LLM-Prompts.md](file:///d:/Repos/Derot-my-brain/Docs/Reference/LLM-Prompts.md)** - LLM prompt templates

### 📦 Features/ - Feature Proposals

- Feature ideas and proposals for future development

### 📁 Archives/ - Historical Documents

- Changelogs and archived documentation

---

## 🔄 Document Relationships

```
Planning/Project-Status.md (What's done, what's next)
    ↓
Planning/Implementation-Roadmap.md (How to build each feature)
    ↓
Planning/Specifications-fonctionnelles.md (What each feature should do)
    ↓
Technical/* (How to implement technically)
    ↓
Developer/Getting-Started.md (How to run the code)
```

---

## 🎯 Workflow for Agents

### Starting a New Feature Implementation

1. **Check Planning/Project-Status.md**
   - Verify the feature is planned
   - Check dependencies are completed
   - Note the current sprint/phase

2. **Read Planning/Implementation-Roadmap.md**
   - Find the specific task (e.g., "Task 3.1: Main Navigation Menu")
   - Review objectives, specifications, and acceptance criteria
   - Note dependencies and complexity

3. **Reference Planning/Specifications-fonctionnelles.md**
   - Read the corresponding functional section
   - Understand business requirements
   - Check data structures and UI requirements

4. **Review Technical Guidelines**
   - **Frontend**: Read [Technical/Frontend-Architecture.md](file:///d:/Repos/Derot-my-brain/Docs/Technical/Frontend-Architecture.md)
   - **Backend**: Read [Technical/Backend-Guidelines.md](file:///d:/Repos/Derot-my-brain/Docs/Technical/Backend-Guidelines.md)
   - **Testing**: Read [Technical/Testing-Strategy.md](file:///d:/Repos/Derot-my-brain/Docs/Technical/Testing-Strategy.md)
   - **Storage**: Read [Technical/Storage-Policy.md](file:///d:/Repos/Derot-my-brain/Docs/Technical/Storage-Policy.md)

5. **Use Appropriate Prompt Template**
   - See [Developer/Prompt-Templates.md](file:///d:/Repos/Derot-my-brain/Docs/Developer/Prompt-Templates.md) for guidance
   - Choose template based on task type (Implementation, Backend, Frontend, etc.)

6. **Implement the Feature**
   - Follow TDD methodology (Red-Green-Refactor)
   - Follow architectural guidelines
   - Write tests as specified

7. **Update Planning/Project-Status.md**
   - Mark the feature as completed
   - Update the status section
   - Note any deviations or issues

---

## 📊 Quick Reference

| Need to... | Use this document |
|------------|-------------------|
| Check project status | [Planning/Project-Status.md](file:///d:/Repos/Derot-my-brain/Docs/Planning/Project-Status.md) |
| Start implementing a feature | [Planning/Implementation-Roadmap.md](file:///d:/Repos/Derot-my-brain/Docs/Planning/Implementation-Roadmap.md) |
| Understand requirements | [Planning/Specifications-fonctionnelles.md](file:///d:/Repos/Derot-my-brain/Docs/Planning/Specifications-fonctionnelles.md) |
| Frontend architecture | [Technical/Frontend-Architecture.md](file:///d:/Repos/Derot-my-brain/Docs/Technical/Frontend-Architecture.md) |
| Backend architecture | [Technical/Backend-Guidelines.md](file:///d:/Repos/Derot-my-brain/Docs/Technical/Backend-Guidelines.md) |
| Testing approach | [Technical/Testing-Strategy.md](file:///d:/Repos/Derot-my-brain/Docs/Technical/Testing-Strategy.md) |
| Storage policy | [Technical/Storage-Policy.md](file:///d:/Repos/Derot-my-brain/Docs/Technical/Storage-Policy.md) |
| Build and run project | [Developer/Getting-Started.md](file:///d:/Repos/Derot-my-brain/Docs/Developer/Getting-Started.md) |
| Find prompt templates | [Developer/Prompt-Templates.md](file:///d:/Repos/Derot-my-brain/Docs/Developer/Prompt-Templates.md) |
| See color palettes | [Reference/Color-Palettes.md](file:///d:/Repos/Derot-my-brain/Docs/Reference/Color-Palettes.md) |
| API structure | [Reference/API-Endpoints.md](file:///d:/Repos/Derot-my-brain/Docs/Reference/API-Endpoints.md) |

---

## 💡 Tips for Better Agent Comprehension

### For Planning Agents
- Start with **Planning/Project-Status.md** to understand context
- Use **Planning/Implementation-Roadmap.md** to create detailed plans
- Reference **Planning/Specifications-fonctionnelles.md** for requirements

### For Implementation Agents
- Read the specific task in **Planning/Implementation-Roadmap.md**
- Check **Planning/Specifications-fonctionnelles.md** for detailed requirements
- Follow **Technical/** guidelines for your area (Frontend, Backend, Testing)
- Use appropriate **Developer/Templates/** prompt template
- Follow the acceptance criteria exactly
- Update **Planning/Project-Status.md** when done

### For Review Agents
- Compare implementation against **Planning/Implementation-Roadmap.md** acceptance criteria
- Verify alignment with **Planning/Specifications-fonctionnelles.md**
- Check compliance with **Technical/** guidelines
- Verify **Planning/Project-Status.md** is updated correctly

---

## 📝 Maintenance Guidelines

### When Adding New Features
1. Add to **Planning/Project-Status.md** (in appropriate phase)
2. Create detailed task in **Planning/Implementation-Roadmap.md**
3. Add functional specification in **Planning/Specifications-fonctionnelles.md**

### When Completing Features
1. Mark as complete in **Planning/Project-Status.md**
2. Update task status in **Planning/Implementation-Roadmap.md**
3. Verify implementation matches **Planning/Specifications-fonctionnelles.md**

### When Changing Requirements
1. Update **Planning/Specifications-fonctionnelles.md** first
2. Modify task in **Planning/Implementation-Roadmap.md**
3. Update status in **Planning/Project-Status.md**
4. Document the change reason

---

## ✅ Summary

The documentation is now organized for **maximum agent comprehension and delegation**:

1. **Planning/** = "What's the current state and what should it do?"
2. **Technical/** = "How should I implement this technically?"
3. **Developer/** = "How do I get started and what templates should I use?"
4. **Reference/** = "Where can I find quick reference materials?"

Each folder serves a specific purpose and they work together to provide complete context for any agent working on the project.

---

**Last Updated:** 2026-01-19
