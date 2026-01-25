---
description: Steps to implement a new backend feature
---
1. **Model & Data**: Define or update Entities in `DerotMyBrain.Core` and configure them in `DerotMyBrain.Infrastructure\Data\DerotDbContext.cs`.
// turbo
2. **Migrations**: Create a new EF Core migration: `dotnet ef migrations add <Name> --project src/backend/DerotMyBrain.Infrastructure --startup-project src/backend/DerotMyBrain.API`.
// turbo
3. **Database Update**: Apply the migration: `dotnet ef database update --project src/backend/DerotMyBrain.Infrastructure --startup-project src/backend/DerotMyBrain.API`.
4. **Repository & Service**: Implement logic in Infrastructure and Core layers.
5. **API**: Create DTOs and Controller endpoints.
6. **TDD**: Ensure each step has unit and integration tests (e.g., in `DerotMyBrain.Tests\Integration`).
7. **Mock Data**: Update `DbInitializer.cs` to seed mock data for `TestUser`.
8. **Verify**: Run `dotnet test`.
