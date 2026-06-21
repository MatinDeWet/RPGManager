# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

RPGManager is a .NET 10 (C#) system backed by PostgreSQL via EF Core. It hosts **two Clean Architecture services** — an ASP.NET Core **`WebApi`** (HTTP API) and a **`Worker`** (Hangfire background jobs) — that share a common kernel (`Shared.Domain` + `Shared.Persistence`) and the same PostgreSQL database, alongside reusable `Utilities/*` libraries. The solution file is `RPGManager.slnx` (XML-based slnx format).

## Branching & Workflow

This repo follows **GitFlow** — see [CONTRIBUTING.md](CONTRIBUTING.md) for the full guide.

- **`main`** and **`develop`** are protected; **never commit to them directly.** All changes go through a pull request, and the `build` CI check must pass before merge.
- **`develop`** is the default branch and the everyday integration target.
- Branch off `develop` with `feature/*` for new work; PR back into `develop`.
- `release/*` (off `develop`) and `hotfix/*` (off `main`) merge into **both** `main` and `develop`.
- When doing work here, create an appropriately-named branch and open a PR — do not push to `main`/`develop`.

## Build & Run

All commands run from the solution root (`/RPGManager`).

```bash
dotnet build                                                   # build whole solution
dotnet run --project Src/Services/WebApi/WebApi.Presentation    # run the API (OpenAPI at /openapi in Development)
dotnet run --project Src/Services/Worker/Worker.Presentation    # run the Hangfire worker
docker compose up                                              # API + Worker together (see compose.yaml)
```

- **Warnings are errors.** `Directory.Build.props` sets `TreatWarningsAsErrors`, `AnalysisMode=All`, and `EnforceCodeStyleInBuild` for every project. A build fails on any analyzer or code-style violation, not just compile errors — fix them rather than suppressing.
- Target framework, `ImplicitUsings`, and `Nullable` are set centrally in `Directory.Build.props`; do not re-declare them per-project.
- NuGet versions are centrally managed in `Directory.Packages.props` (`ManagePackageVersionsCentrally`). Reference packages with `<PackageReference Include="X" />` (no `Version`); add new versions to `Directory.Packages.props`.

### Tests
Tests live under `Tests/` (xUnit v3 + Shouldly + NSubstitute), mirroring the source layout — e.g. `Tests/Services/WebApi/WebApi.infrastructure.UnitTests` and `Tests/Utilities/*`. Run the whole suite with `dotnet test`. When adding a test project, place it under `Tests/` and register it in `RPGManager.slnx`; `Directory.Build.props` names each project's TRX file after the project.

### Pre-commit hooks
Commits are scanned for secrets via [pre-commit](https://pre-commit.com) (config in `.pre-commit-config.yaml`): a `gitleaks` secret scan plus `detect-private-key`, `check-added-large-files`, and `check-merge-conflict`. These hooks are intentionally non-mutating — formatting stays the job of `.editorconfig`/analyzers. Run `pre-commit install` once per clone; see `Docs/PreCommit.md` for the full reference.

## Database & Migrations

- Provider: Npgsql / PostgreSQL. Connection string key is `CoreDB` (see `appsettings.json` / `ConnectionStrings__CoreDB` env var).
- The `CoreContext` and its registration live in the shared **`Src/Shared/Shared.Persistence`** library (`AddPersistence(...)` in `PersistenceDI.cs`), reused by both the WebApi and Worker services. Default query behavior is **`NoTracking`** — call `.AsTracking()` explicitly when you need change tracking.
- Application tables live in the `public` schema; the EF migrations history table lives in a separate `migrations` schema; Hangfire (used by the Worker) keeps its own tables in a dedicated `hangfire` schema. All three names are in `SchemaConstants` (`Shared.Persistence`).
- **EF migrations are applied automatically on startup by the WebApi only** via `app.ApplyDatabaseMigrationsAsync()` in `WebApi.Presentation/Program.cs`; the Worker never applies EF migrations (Hangfire creates its own schema on its first run). Migration files live in `Src/Shared/Shared.Persistence/Data/Migrations/CoreMigrations/`.
- The migrations assembly is `Shared.Persistence` and the startup project is `WebApi.Presentation`. See `Docs/EfMigrations.md` for the full `dotnet ef` command reference (requires `dotnet tool install --global dotnet-ef`).

## Architecture

Two services (`WebApi` and `Worker`) sit on a shared kernel. Layered dependency flow (each references only the one below it):

```
WebApi.Presentation  →  WebApi.infrastructure  →  WebApi.Application  →  WebApi.Domain  ┐
Worker.Presentation  →  Worker.infrastructure  →  Worker.Application  →  Worker.Domain  ┤
                         (both *.infrastructure also → Shared.Persistence)              ├→  Shared.Domain  →  Utilities/Domain
                                                                Shared.Persistence  ────┘
```

- **`Src/Utilities/Domain`** (namespace `Domain.*`) — solution-wide building blocks: `IEntity`/`IEntity<TKey>` contracts, the abstract `Entity<T>` base class (provides `Id` with a protected setter and `DateCreated`), and `GuardClauseExtensions` (e.g. `Guard.Against.ValidString(...)`, built on Ardalis.GuardClauses). Intentionally separate from the domain entities.
- **`Src/Shared/Shared.Domain`** (namespace `Shared.Domain.*`) — the **shared persistence entities** (`Card`, `User`, `TravelHistoryFile`) reused by both services. Entities inherit `Entity<long>`, use **private setters**, and are mutated only through **static `Create` factory methods and instance `Update` methods** that validate via guard clauses. Keep domain types persistence-ignorant: no EF/ORM **attributes** and no mapping config on the entity — all mapping lives in the `IEntityTypeConfiguration<T>`. This rule is about coupling to the ORM, **not** about banning provider value types: when a feature genuinely needs a database-native type (e.g. an `NpgsqlTsVector` property backing PostgreSQL full-text search), put the property on the entity and map it the idiomatic provider way (`HasGeneratedTsVectorColumn` in the config). **Prefer the idiomatic EF Core/Npgsql mapping over contortions** (shadow properties + `EF.Property<>(...)`, hand-written `HasComputedColumnSql`) whose only purpose is to keep such a type off the entity.
- **`Src/Shared/Shared.Persistence`** (namespace `Shared.Persistence.*`) — `CoreContext` (`DbContext`), the EF `IEntityTypeConfiguration<T>` mappings, `SchemaConstants`, the EF migrations, and `AddPersistence(...)` (`PersistenceDI.cs`). `EnableSensitiveDataLogging` is on only in Development/Staging. Both services build their data access on this.
- **`WebApi.Domain` / `Worker.Domain`** — per-service domain types (thin today), each referencing `Shared.Domain`.
- **`WebApi.Application`** — application/use-case layer (CQRS handlers). **`Worker.Application`** — the background-job use-cases (kept free of any Hangfire types).
- **`WebApi.infrastructure`** (note the lowercase `infrastructure` — match it) — the secured/unsecured repo facades + row-level-security locks; `AddInfrastructure(...)` calls `AddPersistence` then registers the repos. **`Worker.infrastructure`** — the worker's unsecured repos plus Hangfire wiring (`AddWorkerInfrastructure(...)`: `AddPersistence` + Hangfire PostgreSQL storage in the `hangfire` schema + the job server).
- **`WebApi.Presentation`** — the API composition root. `Program.cs` is minimal; service registration is grouped into extension methods under `Common/DIExtensions/` (`AddApplicationServices`, `ApplyDatabaseMigrationsAsync`). **`Worker.Presentation`** — the worker host: a minimal ASP.NET app that runs the Hangfire server and serves the **Hangfire dashboard at `/hangfire`**, protected by interactive **Keycloak OIDC login** requiring the **`Hangfire`** role (`AddDashboardAuthentication` + `UseWorkerDashboard`; the `Authentication` config section needs `Authority`/`ClientId`/`ClientSecret`). `Program.cs` composes `AddWorkerApplication` + `AddWorkerInfrastructure` + `AddDashboardAuthentication`, maps the dashboard, and schedules recurring jobs (`RegisterRecurringJobs`).

### Conventions to follow when extending

- **New EF mapping:** add an `IEntityTypeConfiguration<T>` class under `Shared.Persistence/Configuration/`. `CoreContext.OnModelCreating` calls `ApplyConfigurationsFromAssembly`, so configurations are discovered automatically — no `DbSet`/manual registration needed. Map tables via `ToTable(nameof(Entity), SchemaConstants.Default)`.
- **New entity:** inherit `Entity<TKey>`, keep setters private, expose `Create`/`Update` factory methods, and validate inputs with the shared guard-clause extensions.
- After model changes, add a migration (it will auto-apply on next startup).

## Docker

`compose.yaml` builds the API (`Src/Services/WebApi/WebApi.Presentation/Dockerfile`) and the Worker (`Src/Services/Worker/Worker.Presentation/Dockerfile`). Each service has its **own** env file — the API reads `.env.api`, the Worker reads `.env.worker` (both gitignored; copy from the committed `.env.api.example` / `.env.worker.example` templates and fill in real values). The per-service Dockerfiles COPY each referenced `.csproj` individually before `dotnet restore` for layer caching — when you change a project's references, update its Dockerfile's COPY list too. Credentials in `appsettings.json`/`compose.yaml`/the env files are local-dev defaults only — use real secrets for any non-local environment.
