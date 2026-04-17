---
name: triva-webpage
description: >-
  ASP.NET Core MVC app (net10.0) for configurable pages, sections, and content
  components with SQL Server. Uses EF Core for migrations and Dapper repositories
  for CRUD. Use when editing TrivaWebPage, admin CRUD, models, migrations, Razor
  views under Views/Shared/AdminCrud, or repository/controller patterns in this repo.
---

# TrivaWebPage — project skill

## Stack and entry points

- **Runtime**: ASP.NET Core MVC, **.NET 10** (`TrivaWebPage/TrivaWebPage.csproj`).
- **Database**: SQL Server; connection string `ConnectionStrings:DefaultConnection` in `TrivaWebPage/appsettings.json`.
- **ORM split**:
  - **EF Core** (`AppDbContext`) for the model, relationships, and **migrations** under `TrivaWebPage/Migrations/`.
  - **Dapper** for data access at runtime: `GenericRepository<T>` and feature-specific repositories in `TrivaWebPage/Repositories/`.
- **Composition root**: `TrivaWebPage/Program.cs` — registers `AppDbContext`, then `AddDataAccess` from `DependencyInjection/RepositoryServiceCollectionExtensions.cs`.

When you add or change persisted fields, update **both** the entity model used by EF (and run migrations) **and** Dapper usage if the repository maps columns explicitly (check the concrete repository before assuming `GenericRepository` table name matches).

## Folder map

| Area | Path |
|------|------|
| Web app | `TrivaWebPage/` |
| DbContext & factory | `TrivaWebPage/Data/Connection/` |
| Domain models | `TrivaWebPage/Models/` (subfolders: `General`, `Contents`, `CardOptions`, …) |
| Repository interfaces | `TrivaWebPage/Abstractions/` (note: namespace typo `GeneralAbstactions` in existing code — match it when adding types there) |
| Repository implementations | `TrivaWebPage/Repositories/` (`GeneralRepositories`, `ContentRepositories`, `CardOptionRepositories`) |
| DI registration | `TrivaWebPage/DependencyInjection/RepositoryServiceCollectionExtensions.cs` |
| MVC controllers | `TrivaWebPage/Controllers/` |
| Admin ViewModels | `TrivaWebPage/ViewModels/Admin/` (`AdminViewModels.cs`, etc.) |
| Shared admin UI | `TrivaWebPage/Views/Shared/AdminCrud/` (`Index`, `Details`, `Form`, `Delete`) |
| Layout | `TrivaWebPage/Views/Shared/_Layout.cshtml` |

## Controller and admin UI conventions

- Admin list/detail/create/edit/delete actions follow a **repeatable pattern** (see `PagesController`): inject the domain repository interface (`IPage`, `IMediaFile`, etc.), set `ViewBag.DisplayName`, and return shared views under `~/Views/Shared/AdminCrud/...`.
- Forms use `ViewBag.FormAction` (`Create` / `Edit`) where applicable.
- POST actions use `[ValidateAntiForgeryToken]` and `CancellationToken` on async methods.
- Prefer **ViewModels** in `ViewModels/Admin` for forms; map to/from entities in the controller consistently with existing controllers.

## Adding a new admin entity (checklist)

1. Add model under `Models/` (and configure in `AppDbContext` + `OnModelCreating` if relationships are non-trivial).
2. Add abstraction under `Abstractions/` and implementation under `Repositories/` (reuse `GenericRepository<T>` when a simple table maps 1:1 to the entity name `Id`).
3. Register the interface → implementation in `RepositoryServiceCollectionExtensions.AddDataAccess`.
4. Add controller mirroring an existing simple CRUD controller.
5. Add or extend ViewModels for create/edit if the shared form needs new fields; ensure `Views/Shared/AdminCrud/Form.cshtml` can bind them (or extend it carefully).
6. Run EF migrations from the project directory: `dotnet ef migrations add ...` / `dotnet ef database update` (requires EF tools and a valid connection string).

## EF migrations

- `AppDbContextFactory` exists for design-time tooling (`Data/Connection/AppDbContextFactory.cs`).
- After model changes, add a migration and review generated SQL; keep snapshot and migration files in sync with the team.

## Packages (reference)

- EF Core SqlServer + Tools, Dapper, Microsoft.Data.SqlClient, Razor runtime compilation — see `TrivaWebPage.csproj`.

## Quality bar for edits

- Match existing **namespace**, **file layout**, and **async + CancellationToken** patterns.
- Do not rename `GeneralAbstactions` unless the user asks for a breaking refactor across the solution.
- Keep admin UX consistent with existing `AdminCrud` views unless explicitly redesigning.
