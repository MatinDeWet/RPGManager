---
name: add-entity-config
description: Scaffold an EF Core IEntityTypeConfiguration<T> mapping for a domain entity in RPGManager's Shared.Persistence (table, keys, columns, relationships, indexes). Use when mapping a new or existing entity to the database — auto-consumed by add-entity. Covers ToTable/HasKey/ValueGeneratedOnAdd, HasMaxLength/IsRequired, two-way HasOne/WithMany + HasMany/WithOne (the default), composite-key join tables, nullable columns, and GIN/pg_trgm + tsvector indexes. Distinct from add-options, which binds appsettings sections.
---

# Add an EF Core entity configuration (mapping)

One file: `Src/Shared/Shared.Persistence/Configuration/<Name>Config.cs`. This is the **only** place EF mapping lives — the domain entity stays persistence-ignorant (no EF attributes, no Fluent API on the entity itself).

- Implement `IEntityTypeConfiguration<T>` as an **`internal sealed`** class. **No `DbSet`, no manual registration** — `CoreContext.OnModelCreating` runs `ApplyConfigurationsFromAssembly` over the `Shared.Persistence` assembly, so any config placed in this folder is discovered automatically. (A config placed anywhere else — e.g. `WebApi.infrastructure` — would silently never be applied.)
- Map the table with `ToTable(nameof(<Name>), SchemaConstants.Default)` (`using Shared.Persistence.Constants;`).
- `HasKey(x => x.Id)` and `Property(x => x.Id).ValueGeneratedOnAdd()` for a surrogate-key entity.
- Set `HasMaxLength` / `IsRequired` to match the entity's guard-clause limits.

Template (model on `Shared.Persistence/Configuration/WorldConfig.cs`):

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Entities;
using Shared.Persistence.Constants;

namespace Shared.Persistence.Configuration;

internal sealed class TransactionConfig : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> entity)
    {
        entity.ToTable(nameof(Transaction), SchemaConstants.Default);

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        entity.Property(x => x.Description)
            .HasMaxLength(256)
            .IsRequired();

        // Two-way relationship — map BOTH sides (see below).
        entity.HasOne(x => x.User)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.UserId);
    }
}
```

## Relationships are two-way by default — map BOTH sides

**Unless the user explicitly says otherwise, every FK relationship is bidirectional and mapped from both configs over the same FK.** The dependent has a reference nav (`virtual Owner`), the principal has a collection nav (`virtual ICollection<Child>`), and both `…Config` classes describe the relationship.

```csharp
// child config (TransactionConfig) — dependent / owning side
entity.HasOne(x => x.User)
    .WithMany(x => x.Transactions)
    .HasForeignKey(x => x.UserId);

// owner config (UserConfig) — principal side, same FK
entity.HasMany(x => x.Transactions)
    .WithOne(x => x.User)
    .HasForeignKey(x => x.UserId);
```

Both sides describe the same `Transaction.UserId` FK; EF merges them into one relationship. The collection nav on the principal (`add-entity` adds it to the entity) and these two mappings always travel together. Only drop one side when the user explicitly asks for a one-directional (e.g. `WithMany()` with no inverse) relationship.

## Composite-key join tables (many-to-many with role/payload)

A join entity linking A and B (e.g. `CampaignMember` linking `Campaign` and `User` with a `Role`) is keyed on the **pair**, not a surrogate `Id`:

```csharp
internal sealed class CampaignMemberConfig : IEntityTypeConfiguration<CampaignMember>
{
    public void Configure(EntityTypeBuilder<CampaignMember> entity)
    {
        entity.ToTable(nameof(CampaignMember), SchemaConstants.Default);

        // Composite key: a user holds at most one row per campaign.
        entity.HasKey(x => new { x.CampaignId, x.UserId });

        entity.Property(x => x.Role)
            .IsRequired();

        // Map each side of the join two-way (the other side, Campaign, is mapped from CampaignConfig).
        entity.HasOne(x => x.User)
            .WithMany(x => x.CampaignMemberships)
            .HasForeignKey(x => x.UserId);
    }
}
```

Notes:
- The join entity inherits the **non-generic `Entity`** (not `Entity<long>`) so it keeps `DateCreated` but has no surrogate `Id` — see `add-entity`.
- No separate unique index is needed on `(CampaignId, UserId)`: the composite **primary key** already enforces it and serves point lookups.
- EF auto-creates the FK index for the **second** key column (e.g. `IX_CampaignMember_UserId`) but skips one for the first (it leads the PK). That second-column index is what makes "rows for this user" queries efficient — don't add a duplicate.

## Nullable columns

For a `string?` (or otherwise optional) property, **omit `.IsRequired()`** so the column is nullable; set only `.HasMaxLength(...)`. (The entity-side null handling lives in `add-entity`.)

## Indexing

- **ILIKE substring search** (`add-search`): add a **GIN + pg_trgm** index so the search is index-backed, not a sequential scan. `pg_trgm` is already enabled in `CoreContext.OnModelCreating`:

  ```csharp
  entity.HasIndex(x => x.Name)
      .HasMethod("gin")
      .HasOperators("gin_trgm_ops");
  ```

- **Full-text search**: map a generated `tsvector` column with `HasGeneratedTsVectorColumn` and give it its own GIN index.
- **Unique constraint**: `entity.HasIndex(x => x.IdentityId).IsUnique();` (or a composite `x => new { ... }`).

## After scaffolding

- **Add a migration** with the `add-migration` skill (it also covers the remove+re-add iteration loop and, for navigation-only changes, the `has-pending-model-changes` check that confirms no migration is needed).
- `dotnet build` — warnings are errors.
