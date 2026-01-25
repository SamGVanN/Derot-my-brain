# Derot Zone — Wikipedia Integration Progress

Status: In progress

Summary
-------
- Created design doc: `Docs/Technical/Wikipedia-Integration.md` (draft)
- Updated functional specifications: `Docs/Planning/functional_specifications_derot_my_brain.md` with Derot Zone, `Explore` activity, Read/Backlog flows

Completed
---------
- Add Wikipedia integration docs
- Design backend API contract (high-level)
- Update functional specifications to include Derot Zone behavior and UI requirements

Next concrete steps (short-term)
--------------------------------
1. Add Core DTOs and `IWikipediaService` interface in `DerotMyBrain.Core`
2. Implement `MediaWikiWikipediaService` in `DerotMyBrain.Infrastructure` using `HttpClient`
3. Add `WikipediaController` endpoints in `DerotMyBrain.API`
4. Wire DI in `Program.cs` and add `IMemoryCache` caching with configurable TTL
5. Scaffold frontend components for `Derot Zone` header (filters), cards, and Recycle behavior

Notes / Decisions
-----------------
- `Explore` events: lightweight server-side logging is preferred; can be toggled to client-only if privacy concerns arise.
- Backlog additions do not create `UserActivity` by default.
- Direct URL input should validate and extract `lang` and `title`, then call `POST /api/wikipedia/read` to create `UserActivity` Type=`Read`.

How to proceed now
------------------
- I can scaffold the Core DTOs and `IWikipediaService` now. Confirm and I'll implement interfaces and basic DTOs.
