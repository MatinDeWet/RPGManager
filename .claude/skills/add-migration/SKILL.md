---
name: add-migration
description: Create, regenerate, or verify an EF Core migration in the RPGManager solution after a model change. Use after add-entity/add-entity-config or any change to an entity or its configuration (e.g. "add a migration for the Campaign tables", "the model changed, generate the migration"). Wraps the dotnet ef workflow (correct projects/context/output dir), the remove+re-add loop for iterating, and the has-pending-model-changes verification. Migrations auto-apply on WebApi startup.

---

# Add / regenerate an EF Core migration

Run everything from the **solution root**. Requires the EF tool once per machine: `dotnet tool install --global dotnet-ef`. Full command reference is in `Docs/EfMigrations.md`; this skill is the day-to-day workflow and its gotchas.

- **Migrations project:** `Src/Shared/Shared.Persistence`
- **Startup project:** `Src/Services/WebApi/WebApi.Presentation`
- **DbContext:** `CoreContext`
- **Output dir:** `Data/Migrations/CoreMigrations` (keep new files alongside the existing ones)

## 1. Build first

`dotnet build` — the EF tool builds the model from compiled assemblies, so fix compile/analyzer errors before generating.

## 2. Add the migration

```bash
dotnet ef migrations add <Name> \
  --project Src/Shared/Shared.Persistence \
  --startup-project Src/Services/WebApi/WebApi.Presentation \
  --context CoreContext -o Data/Migrations/CoreMigrations
```

Name it for the change (`AddCampaign`, `AddWorldNameTrigramIndex`). The `Sensitive data logging is enabled` line in Development is **benign** — not a failure.

## 3. Read the generated migration

Open the `<timestamp>_<Name>.cs` and sanity-check the `Up`: expected tables/columns, FKs with the intended `onDelete`, and any indexes (composite PK on a join table, unique constraints, `gin`/`gin_trgm_ops` for trigram search). PostgreSQL allows multiple cascade paths, so a join table cascading from two parents is fine.

## Iterating: remove + re-add (before it's applied/shared)

If you spot a problem (or change the model again) **and the migration hasn't been pushed or applied to a shared DB**, regenerate rather than hand-editing:

```bash
dotnet ef migrations remove \
  --project Src/Shared/Shared.Persistence \
  --startup-project Src/Services/WebApi/WebApi.Presentation --context CoreContext
# adjust entity/config, dotnet build, then re-run step 2
```

`remove` also reverts the model snapshot. Once a migration is merged/applied elsewhere, don't remove it — add a new corrective migration instead.

## Verifying a navigation-only change

Adding the inverse side of a relationship (collection nav + the principal-side `HasMany/WithOne`) changes no schema. Confirm the snapshot agrees and you **don't** need a migration:

```bash
dotnet ef migrations has-pending-model-changes \
  --project Src/Shared/Shared.Persistence \
  --startup-project Src/Services/WebApi/WebApi.Presentation --context CoreContext
# expect: "No changes have been made to the model since the last migration."
```

Use this any time you're unsure whether an edit needs a migration.

## Applying

You normally **don't** run `database update` — the WebApi applies pending migrations on startup via `app.ApplyDatabaseMigrationsAsync()` (the Worker never applies EF migrations). Use `database update` only for an explicit out-of-band apply/rollback (see `Docs/EfMigrations.md`).

## After

- `dotnet build` — warnings are errors.
- Commit the migration `.cs`, its `.Designer.cs`, and the updated `CoreContextModelSnapshot.cs` together.
