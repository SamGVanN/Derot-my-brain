# Feature: My Documents / Library

## Description
The **My Documents** page serves as the personal library for the user. It allows users to upload, manage, and utilize their own files (PDF, DOCX, TXT) as content sources for learning activities in the **Derot Zone**.

## Key Functionalities

### 1. Document Upload
Users can upload files to the system.
**Supported Formats**: PDF, DOCX, TXT (to be extended).

#### Upload Workflow
Upon selecting a file, the user is presented with **3 options** to define the immediate intent:

1.  **Just Upload**
    *   **Action**: Save the file to storage.
    *   **Destination**: Appears in the list on *My Documents* page.
    *   **Status**: Inactive / Inventory.
    
2.  **Upload & Add to Backlog**
    *   **Action**: Save to storage AND add an entry to the user library.
    *   **Destination**: *My Documents* + *Backlog* page.
    *   **Status**: To be processed.

3.  **Start Now**
    *   **Action**: Save to storage AND immediately launch a **User Activity** (Type: Read, SourceType: Document, SourceId: DocumentId).
    *   **Destination**: Redirects to **Derot Zone**.
    *   **Status**: Becomes a **User Focus** (SourceType: Document, SourceId: DocumentId) automatically once the activity starts.

### 2. Document Management
*   **List View**: See all uploaded documents.
*   **Actions per Document**:
    *   **Start Activity**: Launch a session in Derot Zone.
    *   **Add to Backlog**: If not already present.
    *   **Delete**: Remove the file (and associated data warnings).

## Technical Context
*   Reference the [Glossary](../Reference/Glossary.md) for terminology (`Content Source`, `User Activity`).
*   Reference the [Data Model](../Reference/DataModel.md) for the database schema.
*   Documents should be stored according to `Docs/Technical/Storage-Policy.md`.
