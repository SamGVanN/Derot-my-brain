---
description: Steps to implement a new backend feature
---
1. **Model & Interface**: Define or update Entities AND Interfaces in `DerotMyBrain.Core`.
2. **Infrastructure**: Implement Interfaces in `DerotMyBrain.Infrastructure` and configure them in `DerotDbContext.cs`.
// turbo
2. **Migrations**: Create a new EF Core migration: `dotnet ef migrations add <Name> --project src/backend/DerotMyBrain.Infrastructure --startup-project src/backend/DerotMyBrain.API`.
// turbo
3. **Database Update**: Apply the migration: `dotnet ef database update --project src/backend/DerotMyBrain.Infrastructure --startup-project src/backend/DerotMyBrain.API`.
4. **Repository & Service**: Implement logic in Infrastructure and Core layers.
5. **API**: Create DTOs and Controller endpoints.
6. **TDD**: Ensure each step has unit and integration tests (e.g., in `DerotMyBrain.Tests\Integration`).
7. **Mock Data**: Update `DbInitializer.cs` to seed mock data for `TestUser`.
8. **Verify**: Run `dotnet test`. **Note**: Ensure no external DB (SQL Server, etc.) is used; stay on SQLite.
