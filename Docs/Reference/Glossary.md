# Project Glossary

This document defines key terms and concepts used in the **Derot-my-brain** project. It serves as a reference for developers and AI agents to ensure consistent terminology.

## Core Concepts

### Derot Zone
* **French**: *Derot Zone*
* **Definition**: The main interactive interface where learning activities take place. It is the session view where a user reads content or takes quizzes.
* **Context**: A user "enters" the Derot Zone to start an **Activity**.

### User Activity
* **French**: *Activité Utilisateur*
* **Definition**: A single record of a user's engagement with specific **Content**.
* **Source Agnostic**: An activity can be based on any **Content Source** (Wikipedia Article, PDF, etc.).
* **Types**:
    * **Read**: The user reads/consults a document or article.
    * **Quiz**: The user takes a quiz to test their knowledge.
* **Data Structure**: Represented by the `UserActivity` entity in the backend.

### Tracked Topic
* **French**: *Sujet Suivi* (or simply *Tracked Topic*)
* **Definition**: A synthesis of a user's progress on a specific piece of **Content**. It aggregates data from multiple **User Activities** related to that specific topic/document.
* **Abstraction**: A Tracked Topic represents the *subject matter*, regardless of whether the source is a Wikipedia page or a custom uploaded document.
* **Key Metrics**:
    * **Best Score**: The highest score achieved in a quiz for this topic.
    * **Last Attempt Score**: The score of the most recent quiz.
    * **Last Activity Date**: When the user last engaged with this topic.
* **Visibility**: Managed on the **Tracked Topics Page**. Users can "untrack" a topic, which hides it from the list but preserves the underlying **User Activity** history.

### Content Source
* **French**: *Source de Contenu*
* **Definition**: The origin of the material used for an activity. The system is designed to be polymorphic regarding sources.
* **Types**:
    * **Wikipedia Article**: Content fetched dynamically from Wikipedia.
    * **User Document**: Static files uploaded by the user (PDF, DOCX, TXT, etc.).
* **Independence**: The learning logic (Quiz generation, scoring, tracking) is independent of the source type.

### Backlog
* **French**: *Backlog* (or *Liste de lecture*)
* **Definition**: A collection of **Content** (Articles or Documents) that the user intends to process later.
* **Purpose**: Allows saving interesting topics without immediately starting an activity or cluttering the **Tracked Topics** list with untouched items.
* **Flow**: Backlog -> (User starts Activity) -> Tracked Topic.

### My Documents
* **French**: *Mes Documents*
* **Definition**: A personal storage space for user-uploaded files (PDF, DOCX, etc.).
* **Purpose**: Acts as an inventory/library of potential **Content Sources**. Uploading a document here makes it available to be used in an **Activity** or added to the **Backlog**.
* **Upload Workflow**: When uploading a document, the user chooses between:
    1.  **Just Upload**: Stores in *My Documents* (Inventory only).
    2.  **Upload & Backlog**: Stores in *My Documents* + adds to *Backlog*.
    3.  **Start Now**: Stores in *My Documents* + starts an **Activity** immediately (becomes a *Tracked Topic*).
* **Distinction**: "My Documents" is for *storage*. "Tracked Topics" is for *learning progress*.

### Knowledge Area
* **French**: *Catégorie* or *Domaine de Connaissance*
* **Definition**: A broad category that groups related **Topics** (e.g., "History", "Science", "Programming").
* **Usage**: Used for filtering and sorting in the **History**, **Tracked Topics**, and **Backlog** pages.

## Pages & Views

### Tracked Topics Page
* **French**: *Page Tracked Topics*
* **Definition**: A dashboard displaying the user's active **Tracked Topics**.
* **Features**:
    * **Tracked Topic Card**: Displays summary stats.
    * **View Stats**: Expands the card to show a timeline specific to that topic.
    * **Try Again**: Option to start a new **User Activity** (Read or Quiz) for that topic.

### History Page
* **French**: *Historique*
* **Definition**: A chronological log of ALL **User Activities**.
* **Scope**: Includes activities from **Wikipedia** AND **My Documents**. Every Read or Quiz session helps populate this list.

## Entities & Data

### Quiz
* **French**: *Quiz*
* **Definition**: A type of activity where the user answers questions generated from the **Content Source**.
* **Metrics**: Generates a score (percentage).

### Score
* **French**: *Score*
* **Definition**: The result of a Quiz activity, typically expressed as a percentage or a ratio of correct answers.

### Title
* **French**: *Titre*
* **Definition**: The user-facing name of a **User Activity**, **Tracked Topic**, or **Content Source**.
* **Usage**:
    *   **Wikipedia**: The article title (e.g., "Napoleon").
    *   **Documents**: The filename (e.g., "my-notes.pdf") by default, but can be renamed by the user.
*   **Unique Reference**: `Title` is mutable (can be changed). Systems should rely on `SourceUrl` or `Id` for technical uniqueness.

## User Roles (Testing)

### TestUser
* **ID**: `test-user-id-001`
* **Role**: The standard user account used for all development testing and mock data seeding.
