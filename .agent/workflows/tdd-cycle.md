---
description: Red-Green-Refactor cycle for TDD
---
// turbo-all
1. Identify the unit of code to be tested (Service method, Hook, or Component).
2. Create/Update a test file in the appropriate test project (e.g., `src/backend/DerotMyBrain.Tests/Units/MyServiceTests.cs`).
3. Write a failing test (RED) covering a specific scenario.
4. Run tests: `dotnet test` or `npm test`.
5. Write minimal code to make the test pass (GREEN).
6. Run tests again to verify success.
7. Refactor the code for architectural compliance while keeping tests GREEN.
8. Verify results with `dotnet test` or `npm test`.
