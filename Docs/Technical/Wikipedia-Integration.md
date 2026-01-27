# Wikipedia API Integration

This document outlines the technical implementation and compliance requirements for the Wikipedia integration in Derot My Brain.

## Overview

The application integrates with the MediaWiki Action API to provide:
1.  **Discovery**: Random articles with summaries and images for the "Derot Zone".
2.  **Reading**: Full text extracts for active learning/reading sessions.

## Wikipedia API Compliance (Critical)

Wikipedia requires all API clients to follow their [User-Agent Policy](https://meta.wikimedia.org/wiki/User-Agent_policy). Failure to comply can result in IP blocks.

### User-Agent Structure
Our application sends a custom User-Agent in the following format:
`DerotMyBrain/1.0 (https://github.com/SamGVanN/Derot-my-brain; <Optional Contact Email>)`

### Configuration
To keep your contact email private while complying with Wikipedia's request for a point of contact, configure it in `appsettings.json` or via environment variables.

#### 1. Local Configuration (`appsettings.json`)
```json
{
  "Wikipedia": {
    "ContactEmail": "your-email@example.com"
  }
}
```

#### 2. Environment Variable (Recommended for Production/Public Repos)
```bash
Wikipedia__ContactEmail=your-email@example.com
```

> [!IMPORTANT]
> If no email is provided (or if it matches the default placeholder), the application falls back to just the GitHub URL. Providing an email is recommended for high-volume usage.

## Endpoints and Parameters

All requests are `GET` requests to `https://{lang}.wikipedia.org/w/api.php`.

### 1. Discovery (Explore View)
Used to fetch random articles for the teaser cards.

-   **Base URL**: `https://{lang}.wikipedia.org/w/api.php`
-   **Query Parameters**:
    -   `action=query`: Core query module.
    -   `format=json`: Response format.
    -   `generator=random`: Fetches random pages.
    -   `grnnamespace=0`: Limits to main articles (ignores talk pages, etc.).
    -   `grnlimit={count}`: Number of articles to fetch.
    -   `prop=extracts|pageimages`: Fetches both text summaries and images in one call.
    -   `exintro=1`: Returns only the lead section.
    -   `explaintext=1`: Returns plain text instead of HTML.
    -   `exsentences=3`: Limits summary to 3 sentences for teaser consistency.
    -   `pithumbsize=300`: Requests a 300px thumbnail.

### 2. Full Content (Read View)
Used to fetch the complete text of an article for reading.

-   **Query Parameters**:
    -   `action=query`
    -   `format=json`
    -   `prop=extracts`
    -   `explaintext=1`: Plain text is required for LLM processing and clean UI rendering.
    -   `titles={Title}`: The specific article title.

## Technical Implementation

-   **Infrastructure Layer**: [WikipediaClient.cs](file:///d:/Repos/Derot-my-brain/src/backend/DerotMyBrain.Infrastructure/Clients/WikipediaClient.cs) handles the `HttpClient` calls and JSON parsing using `System.Text.Json.Nodes`.
-   **Core Layer**: `WikipediaService.cs` orchestrates the logic.
-   **Frontend**: `wikipediaApi.ts` and `useWikipediaExplore.ts` manage state and user actions.

## Best Practices Followed
-   **Consolidated Calls**: Images and extracts are fetched in a single request to minimize latency.
-   **Error Handling**: Logging is implemented to capture API limits or network issues.
-   **Respect for Formatting**: Plain text extracts (`explaintext=1`) are favored over HTML to ensure compatibility with LLM quiz generation.
