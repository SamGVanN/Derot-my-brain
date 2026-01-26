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
* **Definition**: A single record of a user’s interaction with a specific **Source**.
* **Characteristics**:
  - Bound to a **User Session**
  - Directly linked to a **Source** for traceability
  - Append-only
* **Types**:
  - **Read**
  - **Quiz**
* **Key Properties**:
  - User Id
  - Session Id
  - Source Id (FK)
  - Start Date / Finish Date
  - Metrics (Time, Score)

---

### Source
* **French**: *Source*
* **Definition**: The central hub for a piece of learning content. It materializes a Wikipedia article or a Document into the user's library.
* **Types**:
  - Wikipedia Article
  - Document
  - Custom
* **Key Properties**:
  - Id (PK: Hash for Wiki, Guid for Documents)
  - UserId (Individual library ownership)
  - TopicId (Optional grouping Folder)
  - IsTracked (Quick access flag)
  - ExternalId (PageTitle or Original DocumentId)
* **Rules**:
  - All learning metrics (Activities, Scores) are aggregated at the Source level.
  - Tracking a source (`IsTracked = true`) makes it appear in **My Focus Area**.

---

### Topic (Folder / Sujet)
* **French**: *Sujet* or *Dossier*
* **Definition**: An organizational entity used to group multiple **Sources** under a common theme.
* **Purpose**: Scaling learning from single sources to comprehensive areas (e.g., "Quantum Physics").
* **Rules**:
  - Belongs to a **UserId**.
  - Can be targeted by a **User Session** to include all child sources in activities (Future V2).

---

### Backlog
* **French**: *Backlog*
* **Definition**: List of **Sources** the user intends to process later.
* **Rules**:
  - Points to a specific `SourceId`.
  - Starting an activity from the Backlog links the **User Session** to that source and removes the item from the queue.

---

### My Library (Ma bibliothèque)
* **French**: *Ma bibliothèque*
* **Definition**: View displaying all user-uploaded **Documents** and their associated **Sources**.
* **Rules**:
  - A Document is always linked to exactly one **Source**.
  - Deleting a document prevents new activities but preserves history (the **Source** and its **User Activities** remain).

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
* **Definition**: View displaying all **Sources** where `IsTracked = true`.
* **Behavior**:
  - One card per Source.
  - Quick access to the timeline and aggregated statistics for that content.

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
