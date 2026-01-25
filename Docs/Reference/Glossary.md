# Project Glossary – Derot My Brain

This document defines the **normative vocabulary** used across the Derot My Brain project.
It is the single source of truth for terminology used in:
- Product & UX
- Backend & Frontend
- Architecture
- AI / LLM integration

Any term defined here **must be used consistently** in code, UI, documentation, and specifications.

---

## Core Concepts (V1)

### Derot Zone
* **French**: *Derot Zone*
* **Definition**: The interactive session space where a **User Activity** actually takes place.
* **Context**:
  - Reading content
  - Answering quizzes
* **Rules**:
  - A Derot Zone session is always linked to **one Content Source**
  - Entering the Derot Zone typically creates a **User Activity** or a **Backlog Item** or nothing if the user just wants to explore.

---


### User Activity
* **French**: *Activité Utilisateur*
* **Definition**: A single record of a user’s interaction with a specific **Content Source**.
* **Characteristics**:
  - Source-agnostic
  - Append-only (never deleted, partially updatable)
* **Types**:
  - **Read**
  - **Quiz**
* **Key Properties**:
  - User Id
  - Source Type
  - Source Id
  - Source Hash
  - Start Date
  - Finish Date
  - Time spent on reading
  - Time spent on quiz
  - Score (Quiz only)
  - QuestionsCount (Quiz only)

---

### Content Source
* **French**: *Source de Contenu*
* **Definition**: The origin of learning material used to generate User Activities. 
* **Types**:
  - Wikipedia Article : Content fetched dynamically from Wikipedia.
  - User Document : Static files uploaded by the user (PDF, DOCX, TXT, etc.).
* **Rules**:
  - Learning logic is independent of the source type
  - Aggregation relies exclusively on SourceHash
* **Independence**: The learning logic (Quiz generation, scoring, tracking) is independent
---

### SourceHash
* **Definition**: Deterministic identifier representing a unique Content Source.
* **Computation**:
  - hash(SourceType + SourceId)
* **Properties**:
  - Stable
  - Global
  - User-agnostic
* **Purpose**:
  - Link User Activities together
  - Enable aggregation and filtering

---

### UserFocus
* **French**: *Focus Utilisateur*
* **Definition**: User-scoped entity representing the explicit choice to follow progress on **one Content Source**.
* **Role**:
  - Visibility and aggregation only
  - Contains no learning data of its own
* **Key**:
  - (UserId, SourceHash)
* **Rules**:
  - Created only via explicit user action (Track)
  - Deleted via Untrack
  - Deleting a UserFocus never deletes User Activities

---

### Backlog
* **French**: *Backlog*
* **Definition**: List of Content Sources the user intends to process later, without creating activities yet.
* **Purpose**: Allows saving interesting topics without immediately starting an activity or cluttering the **Tracked Topics** list with untouched items.
* **Flow**: Backlog -> (User starts Activity) -> Delete from Backlog when UserActivity is created
* **Primary Use Case**:
  - Wikipedia exploration
* **Rules**:
  - Starting an activity from the Backlog:
    - Creates a User Activity
    - Creates a UserFocus
    - Removes the source from the Backlog

---

### My Documents (Ma bibliothèque)
* **French**: *Ma bibliothèque*
* **Definition**: Personal storage space for user-uploaded documents.
* **Rules**:
  - Documents are immutable once uploaded (except DisplayName property)
  - A document can be Source for multiple User Activities
  - Documents do not need to be added to the Backlog
  - Deleting a document:
    - Prevents new activities
    - Preserves all existing User Activities

---

### Knowledge Area
* **French**: *Domaine de connaissance*
* **Definition**: High-level thematic category used for filtering Wikipedia articles and filtering statistics.
* **Examples**:
  - History
  - Computer Science
  - Biology

---

## Pages & Views

### My Focus Area
* **French**: *Mes Focus*
* **Definition**: View displaying all UserFocus entities.
* **Behavior**:
  - One card per SourceHash
  - Each card can be expanded to reveal the full timeline of User Activities
  - Activities are ordered by descending date

---

### History
* **French**: *Historique*
* **Definition**: Exhaustive chronological log of all User Activities.
* **Rules**:
  - Displays tracked and untracked activities
  - Allows Track / Untrack actions
  - Is the source of truth for user learning history

---

## V2 – Advanced Learning Concepts (Future)

### StudyFocus (V2)
* **French**: *Focus d’Étude*
* **Definition**: Higher-level learning objective grouping multiple Content Sources.
* **Purpose**:
  - Exam preparation
  - Certification training
  - Multi-document quizzes
* **Notes**:
  - Aggregates multiple SourceHash values
  - Generates quizzes across combined content
  - Separate entity from UserFocus

---

## V3 – Embedded AI (Future)

### Embedded LLM (V3)
* **Definition**: Fully offline Large Language Model embedded in the application.
* **Technology**:
  - llama.cpp or equivalent
* **Comparison**:
  - **V1**: External LLM (e.g. Ollama running locally)
  - **V3**: Embedded, offline, zero mandatory external dependency

---

End of glossary.
