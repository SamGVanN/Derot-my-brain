# Derot Zone — Wikipedia Integration Progress (Updated)

Status: Integrated — Ready for Quiz phase.

## Done
- **Backend Flow**:
    - `Explore` session created on entry, UserActivity of type Explore.
    - `StopExplore` endpoint tracks duration and backlog counts.
    - `Read` transition fetches real Wikipedia content via `WikipediaContentSource`, creates a `Read` activity, and links it to the `Explore` session.
    - DTOs updated to carry `ExploreDurationSeconds`.
- **Frontend Flow**:
    - `useWikipediaExplore` tracks session start time.
    - `StopExplore` called on exit (redirect to Focus Area).
    - `Read` button on cards triggers read activity creation and URL update, in same session than Explore.
    - `ReadView` fetches and displays the actual article content from the backend.
- **Documentation**:
    - Overhauled `Implementation-Roadmap.md` and `Project-Status.md`.
    - Updated `functional_specifications_derot_my_brain.md`.

## Known Issues
- E2E tests (`DerotZone.e2e.spec.ts`) still need stabilization regarding backend teardown.
- Quiz mode is still using static placeholders (Next Phase).

## Next Steps
1. **Quiz Generation**: Connect `QuizView` to LLM service via backend.
2. **E2E Stability**: Refactor test runner to handle ports more robustly.
