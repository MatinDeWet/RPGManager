# RPGManager

**A Dungeons & Dragons 5e–compatible campaign world & session manager.**

RPGManager is a tool for Dungeon Masters to build, run, and reuse their tabletop content. Design persistent campaign worlds, run live sessions, and drive combat from one place — while the monsters and items you create live in a reusable library you can **import and export** across every campaign you run.

> _Status: early development. The platform foundation is in place; the RPG features described below are an active roadmap, not yet built. See [Project status](#project-status)._

---

## Overview

RPGManager treats a **world** as the home for your reusable content — its **monsters** and **items**. Worlds feed into **campaigns**, campaigns are played out over **sessions**, and sessions run their encounters through a **combat tracker**. Because monsters and items are authored once and shared through import/export, prep you do for one table carries straight into the next.

Under the hood it's a two-service .NET 10 system — an HTTP **WebApi** and a background **Worker** — sharing one PostgreSQL database and a Clean Architecture kernel.

## Features

### Available now
- **Solid platform foundation** — Clean Architecture, two services (WebApi + Worker) on a shared domain/persistence kernel.
- **.NET 10 + PostgreSQL** via EF Core, with migrations applied automatically on API startup.
- **Authentication** — OIDC / OAuth2 (JWT bearer) wired in.
- **Background jobs** — Hangfire with PostgreSQL storage and a protected dashboard.
- **CQRS** — an in-house command/query toolkit with in-memory domain events.
- **Blob storage** — S3 / MinIO integration for file content.
- **Hybrid caching**, **OpenAPI/Swagger**, and **user management**.

### Roadmap
- [ ] **Worlds** — top-level container for reusable content
- [ ] **Campaigns** — built on top of a world
- [ ] **Sessions** — schedule, run, and log play
- [ ] **Combat tracker** — initiative order and encounter management
- [ ] **Monsters** — create, edit, and **import / export**
- [ ] **Items** — create, edit, and **import / export**
- [ ] **Reusable content library** — share worlds, monsters, and items across campaigns

## Tech stack

| Layer            | Technology                                                |
| ---------------- | --------------------------------------------------------- |
| Runtime          | .NET 10 (`net10.0`)                                       |
| API              | ASP.NET Core, Swagger / OpenAPI (Swashbuckle)             |
| Database         | PostgreSQL via Npgsql + EF Core 10                        |
| Background jobs  | Hangfire (PostgreSQL storage)                             |
| Auth             | OIDC / OAuth2 — JWT Bearer + OpenID Connect               |
| CQRS             | In-house toolkit (`Src/Utilities/CQRS`)                   |
| Blob storage     | AWS S3 / MinIO (`AWSSDK.S3`)                              |
| Caching          | `Microsoft.Extensions.Caching.Hybrid` (L1 + distributed)  |
| Result handling  | Ardalis.Result + Ardalis.GuardClauses                     |
| Testing          | xUnit v3, Shouldly, NSubstitute                           |

## Architecture

Two services sit on a shared kernel. Each layer references only the one below it:

```
WebApi.Presentation  →  WebApi.infrastructure  →  WebApi.Application  →  WebApi.Domain  ┐
Worker.Presentation  →  Worker.infrastructure  →  Worker.Application  →  Worker.Domain  ┤
                         (both *.infrastructure also → Shared.Persistence)              ├→  Shared.Domain  →  Utilities/Domain
                                                                Shared.Persistence  ────┘
```

- **WebApi** serves the HTTP API; **Worker** hosts Hangfire background jobs.
- **Shared.Domain** holds the persistence entities reused by both services; **Shared.Persistence** holds the `CoreContext`, EF mappings, and migrations.
- Cross-cutting building blocks live under `Src/Utilities/*`; external-service clients under `Src/Integrations/*`.

For the full architecture guide and contribution conventions, see [CLAUDE.md](CLAUDE.md).

## Getting started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/)
- Docker (for `docker compose`) and/or a local PostgreSQL instance

### Configure environment
Copy the env templates and fill in real values:

```bash
cp .env.api.example    .env.api
cp .env.worker.example .env.worker
```

### Build & run

```bash
dotnet build                                                   # build the whole solution

# run the API (OpenAPI at /openapi in Development)
dotnet run --project Src/Services/WebApi/WebApi.Presentation

# run the Hangfire worker (dashboard at /hangfire)
dotnet run --project Src/Services/Worker/Worker.Presentation

# or run both services together
docker compose up                                             # API on :8080, Worker on :8081
```

## Database & migrations

The app uses PostgreSQL via Npgsql. EF Core migrations are **applied automatically by the WebApi on startup** — the Worker never applies them. For the `dotnet ef` command reference, see [Docs/EfMigrations.md](Docs/EfMigrations.md).

## Testing

```bash
dotnet test
```

Tests live under `Tests/` (xUnit v3 + Shouldly + NSubstitute), mirroring the source layout.

## Project layout

```
Src/
├── Services/
│   ├── WebApi/        Presentation · infrastructure · Application · Domain
│   └── Worker/        Presentation · infrastructure · Application · Domain
├── Shared/
│   ├── Shared.Domain        persistence entities
│   └── Shared.Persistence   CoreContext, EF config, migrations
├── Integrations/
│   └── BlobStorage.Integration   S3 / MinIO
└── Utilities/
    ├── CQRS · Caching · Domain · Identification · Repository
```

The solution file is `RPGManager.slnx` (XML slnx format).

## Project status

This repository is a mature Clean-Architecture foundation with the platform features above in place, but the RPG domain (worlds, campaigns, sessions, combat, monsters, items) has **not been implemented yet** — those are the [roadmap](#roadmap). Treat anything outside "Available now" as planned.

## Contributing & workflow

The project follows GitFlow — branch off `develop`, open a PR back into it. See [CONTRIBUTING.md](CONTRIBUTING.md) for the full guide. Commits are scanned for secrets via pre-commit hooks ([Docs/PreCommit.md](Docs/PreCommit.md)).

## License & disclaimer

Licensed under the GNU General Public License v3.0 — see [LICENSE.txt](LICENSE.txt).

_Dungeons & Dragons 5e compatible. RPGManager is an independent, fan-made project and is not affiliated with, endorsed, or sponsored by Wizards of the Coast._
