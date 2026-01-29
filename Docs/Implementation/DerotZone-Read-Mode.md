# Implementation Guide: DerotZone Read Mode

## Objective
Enable a source-agnostic Read mode in the DerotZone that retrieves and displays text content for both Wikipedia Articles and Documents, ensuring the frontend remains "dumb" and the backend handles source resolution and persistence.

## Current State & Gaps
- **Backend**: `ActivityService.ReadAsync` exists but needs to be smarter about source resolution and DTO mapping.
- **Frontend**: `ReadView.tsx` exists but doesn't receive the `ArticleContent` because it's filtered out during DTO mapping in `ActivitiesController`.

## Proposed Steps

### Phase 1: Backend Fixes (Source Resolution & Persistence)

1.  **ActivitiesController.cs**
    - Update `ReadRequest` to make `SourceType` optional.
    - **CRITICAL**: Update `MapToDto` to include `ArticleContent`.
2.  **ActivityService.cs**
    - Refine `ReadAsync` to:
        - Check if `SourceId` is a GUID (Document) or Meta-Data (Wiki Title/URL).
        - Use `SourceHasher` to resolve the technical ID.
        - Auto-create `Source` and `OnlineResource` for new Wikipedia articles.
        - Persist `ArticleContent` to the `UserActivity` record.
3.  **WikipediaContentSource.cs**
    - Ensure it uses the library language for fetching if not explicitly provided in URL.

### Phase 2: Frontend Integration (Display & Flow)

1.  **ReadView.tsx**
    - Verify it renders the `articleContent` correctly (already uses `whitespace-pre-wrap`).
    - Add a "Source Provider" label (already partially there).
2.  **useWikipediaExplore.ts**
    - Update `onRead` to send only the necessary `SourceId` (Title or URL).

## Technical Reference
- **Source Identification**: See [Source-Identification.md](file:///d:/Repos/Derot-my-brain/Docs/Technical/Source-Identification.md).
- **Persistence**: Content is stored in `UserActivity.ArticleContent` (Text) and `UserActivity.Payload` (JSON metadata).

## Verification
1.  Read a Wikipedia article from Explore -> Verify text display.
2.  Read a Document from Library -> Verify text display.
3.  Check DB `Activities` table -> Verify `ArticleContent` is NOT null after a Read.
