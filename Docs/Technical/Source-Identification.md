# Source Identification Strategy

This document explains how Derot My Brain identifies and distinguishes between different types of content sources.

## Overview

The `Source` entity is the central hub for all content related to a user activity. To ensure data integrity and avoid unnecessary duplication, we use two distinct strategies for generating the technical `SourceId`.

## Identification Strategies

### 1. Deterministic Hashing (Web Content)
For sources like **Wikipedia articles** or **YouTube videos**, we use a deterministic hash based on the content's origin and its public identifier.

- **Formula**: `SHA-256(SourceType + ":" + ExternalId)`
- **Behavior**: If two different users (or the same user at different times) access the same Wikipedia article, the system generates the **same ID**.
- **Benefit**:
  - Prevents duplication of the same article in the system.
  - Allows for future global statistics on a per-article basis.
  - Enables "Source Hub" logic where multiple activities point to the same existing source record.

### 2. GUID Identification (Personal Documents)
For **uploaded documents** (PDF, Docx, etc.), we use a unique GUID generated at the time of upload.

- **Formula**: `Guid.NewGuid().ToString()`
- **Behavior**: Every upload creates a **new, unique ID**, even if the file name is identical to an existing one.
- **Benefit**:
  - Treats each upload as a personal, isolated resource.
  - Avoids collisions between different versions of the "same" document uploaded by different users.
  - Simplifies file tracking and deletion (one GUID = one physical file in storage).

## Implementation Details

The logic is centralized in the [SourceHasher.cs](file:///d:/Repos/Derot-my-brain/src/backend/DerotMyBrain.Core/Utils/SourceHasher.cs) utility class.

```csharp
public static string GenerateId(SourceType sourceType, string sourceId)
{
    if (sourceType == SourceType.Document)
    {
        return sourceId; // Already a GUID
    }
    
    // Wikipedia, etc.
    return ComputeSha256($"{sourceType}:{sourceId}");
}
```

## Impact on "Read" Mode
When starting a Read Activity from the **Explore View** (Wikipedia), the backend:
1. Receives the article title/URL.
2. Computes the hash.
3. Checks if a `Source` already exists with this hash.
4. If not, creates it automatically before starting the activity.
