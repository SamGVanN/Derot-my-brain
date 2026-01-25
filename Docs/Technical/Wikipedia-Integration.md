# Wikipedia (MediaWiki) Integration — Derot Zone

Status: Draft

Purpose
-------
Describe the design and API contract for integrating Wikipedia (MediaWiki) into the `Derot Zone` feature so that users can explore, read, and add articles to their backlog.

High-level goals
----------------
- Surface Wikipedia article cards (summary, thumbnail, language link)
- Allow adding an article to the user's Backlog
- Provide a `Read` action which creates a `UserActivity` of Type `Read`
- While browsing, activity is `Explore`
- Provide UI controls: header with changeable categories (session-only), direct URL input, and a `Recycle` button for random/refetched articles

UserActivity types
------------------
- `Explore` — user is browsing/searching articles; should be logged while the user is exploring
- `Read` — user explicitly reads an article (via the `Read` button or direct URL); creates a `UserActivity` Type `Read`
- `Quiz` — future: when user takes a quiz based on a source

Frontend requirements (Derot Zone)
---------------------------------
- Header with session-only categories and filters:
  - Categories are stored client-side for the current Derot session only (do not persist in `UserPreference` database)
  - Category filter values affect search/random fetches
- Text field for direct article URL:
  - Accepts a Wikipedia article URL from any supported language subdomain, extracts title & lang, and triggers a `Read` flow
- `Recycle` button:
  - Refetches random articles using the current header filters (or search terms)
  - Should call backend `/api/wikipedia/random` endpoint
- Article card UI:
  - Title, summary (short), thumbnail (if available), language, link to full article on Wikipedia
  - Buttons: `Read`, `Add to Backlog`
  - `Read` triggers a backend call that creates a `UserActivity` Type `Read` and returns article details
  - `Add to Backlog` triggers a backend call to add the article to the user's backlog (or create a backlog entry)
- State handling: loading, empty, error

Backend design
--------------
MediaWiki endpoints to use (no API key required):
- Summary: `https://{lang}.wikipedia.org/api/rest_v1/page/summary/{title}`
- Search: `https://{lang}.wikipedia.org/w/rest.php/v1/search/title?q={q}&limit={n}` or the classic `action=query&list=search`
- Random pages: `action=query&list=random` (MediaWiki API)

Proposed API contract (DerotMyBrain.API)
---------------------------------------
- GET `/api/wikipedia/search?q={q}&lang={lang}&limit={n}`
  - Response: list of `WikipediaArticleDto` (id/title/summary/thumbnail/lang/url)
- GET `/api/wikipedia/summary?title={title}&lang={lang}`
  - Response: `WikipediaArticleDto` full summary
- GET `/api/wikipedia/random?lang={lang}&count={n}&categories={csv}`
  - Response: list of `WikipediaArticleDto`
- POST `/api/wikipedia/read`
  - Body: `ReadRequest { title, lang, sourceUrl? }`
  - Action: fetch article summary, create `UserActivity` with Type=`Read` (attach article metadata), return created `UserActivity` id + article DTO
- POST `/api/wikipedia/explore`
  - Body: `ExploreEvent { query?, lang?, filters? }`
  - Action: optionally log `UserActivity` Type=`Explore` (or aggregate client-side) — lightweight server-side logging
- POST `/api/backlog` (if not present already)
  - Body: `BacklogAddRequest { title, lang, url, summary? }`
  - Action: add item to user's backlog (existing backlog APIs should be reused if available)

Data shapes (DTOs)
------------------
- `WikipediaArticleDto`:
  - `string Title`
  - `string PageId` (or numeric id)
  - `string Summary`
  - `string ThumbnailUrl` (nullable)
  - `string Language`
  - `string SourceUrl`
- `ReadRequest`:
  - `string Title`
  - `string Language`
  - `string SourceUrl` (optional)

Implementation notes
--------------------
- Core interface: add `IWikipediaService` in `DerotMyBrain.Core` with methods: `SearchAsync`, `GetSummaryAsync`, `GetRandomAsync`.
- Infrastructure: implement `MediaWikiWikipediaService` in `DerotMyBrain.Infrastructure` using `HttpClient` and resilient patterns (timeouts, retries)
- Controller: add `WikipediaController` in `DerotMyBrain.API/Controllers` to expose the endpoints above
- Caching: cache summaries with configurable TTL (6–24h). Use `IMemoryCache` for dev, support Redis in production
- Rate limiting: add basic safeguards (per-IP or per-user throttling) to avoid hitting MediaWiki limits
- UserActivity creation:
  - `Read` flow must create a `UserActivity` entity with Type=`Read` and attach article metadata (title, url, language, pageId)
  - `Explore` flow: create `UserActivity` Type=`Explore` when user begins exploring or when the frontend sends explicit event
- Backlog integration: reuse existing backlog service if present; otherwise add `IBacklogService` call from controller

Edge cases and UX
-----------------
- Disambiguation pages: surface disambiguation hint and provide links to options
- Redirects: MediaWiki summary endpoint follows redirects; ensure title/url reflect final page
- Missing images: show placeholder
- Invalid direct URLs: validate and surface helpful errors

Testing
-------
- Unit tests: mock `HttpClient` and test `IWikipediaService` implementation
- Integration tests: optional live call to MediaWiki behind a feature flag
- Frontend tests: component tests for `Derot Zone` UI, E2E for flows (Explore → Read → Backlog)

Security & Privacy
------------------
- No user credentials are sent to MediaWiki — calls are server-side to avoid leaking user IP via client when possible
- Cache article summaries; do not store full user browsing behavior without consent

Open decisions
--------------
- Should `Explore` events be logged on every search/refetch, or only on explicit user actions? Current draft supports both; default: log lightweight `Explore` events when user opens Derot Zone and when they explicitly send `explore` events.
- Backlog API shape: prefer reusing existing backlog model; if none exists we will add a minimal backlog entity.

Next steps
----------
1. Finalize API request/response DTOs and add them to `DerotMyBrain.Core/DTOs`.
2. Add `IWikipediaService` to `DerotMyBrain.Core` and implement it in `DerotMyBrain.Infrastructure`.
3. Add controller endpoints and wire DI in `Program.cs`.
4. Implement frontend `Derot Zone` components and flows.
