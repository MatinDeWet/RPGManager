---
name: add-entity
description: Scaffold a new persistence-ignorant domain entity and its EF Core configuration in the RPGManager solution. Use when adding a new database-backed entity (e.g. "add a Transaction entity with amount and date"). Creates the Entity<long> domain type (private setters, Create/Update factories, guard-clause validation) and the matching IEntityTypeConfiguration mapping, then reminds you to add a migration.
---

# Add a domain entity + EF configuration

Two files, layered: the entity in `Shared.Domain`, the mapping in `Shared.Persistence`. Both services share these through the common kernel. Keep the domain type **persistence-ignorant** — no EF attributes.

## 1. Entity — `Src/Shared/Shared.Domain/Entities/<Name>.cs`

- Inherit `Entity<long>` (from `Domain.Implementation`) — gives `Id` (protected setter, DB-generated) and `DateCreated`.
- **Private setters** on every property.
- Mutate only through a static `Create` factory and instance `Update` methods.
- Validate inputs with the shared guard clauses (`Guard.Against.ValidString(...)`, `using Ardalis.GuardClauses;` + `using Domain.Extensions;`). Put each validation in a `private static` helper so `Create` and `Update` share it.
- Foreign keys: a `long <Owner>Id` plus a `virtual <Owner>` nav property (model on `User`). **Always** make the relationship two-way — also add the matching collection nav on the owner and map both sides (see section 3).

Template (model on `Shared.Domain/Entities/User.cs`):

```csharp
using Ardalis.GuardClauses;
using Domain.Extensions;
using Domain.Implementation;

namespace Shared.Domain.Entities;

public class Transaction : Entity<long>
{
    public long UserId { get; private set; }

    public virtual User User { get; private set; } = null!;

    public string Description { get; private set; }

    public decimal Amount { get; private set; }

    public static Transaction Create(long userId, string description, decimal amount)
    {
        return new Transaction
        {
            UserId = userId,
            Description = ValidDescription(description),
            Amount = amount,
        };
    }

    public void Update(string description, decimal amount)
    {
        Description = ValidDescription(description);
        Amount = amount;
    }

    private static string ValidDescription(string description)
    {
        return Guard.Against.ValidString(description, nameof(description), maxLength: 256);
    }
}
```

### Optional (nullable) string values

For a **nullable** column, type the property `string?` and short-circuit `null` in the helper. Do **not** rely on `ValidString(..., allowNullOrWhiteSpace: true)` — it coerces `null`/blank into `string.Empty`, which writes `""` to the DB instead of `NULL`. Validate the max length only when a value is actually present:

```csharp
public string? Description { get; private set; }

private static string? ValidDescription(string? description)
{
    if (string.IsNullOrWhiteSpace(description))
    {
        return null;
    }

    return Guard.Against.ValidString(description, nameof(description), maxLength: 4096);
}
```

In the config, leave the property without `.IsRequired()` so the column is nullable (just set `.HasMaxLength(...)`).

## 2. Configuration — `Src/Shared/Shared.Persistence/Configuration/<Name>Config.cs`

- Implement `IEntityTypeConfiguration<T>` as an `internal sealed` class. **No `DbSet` and no manual registration** — `CoreContext.OnModelCreating` runs `ApplyConfigurationsFromAssembly` over the `Shared.Persistence` assembly, so any config placed here is discovered automatically. (A config placed anywhere else — e.g. `WebApi.infrastructure` — would silently never be applied.)
- Map the table with `ToTable(nameof(<Name>), SchemaConstants.Default)` (`using Shared.Persistence.Constants;`).
- `HasKey(x => x.Id)` and `Property(x => x.Id).ValueGeneratedOnAdd()`.
- Set `HasMaxLength` / `IsRequired` to match the entity's guard limits; configure relationships with `HasOne/WithMany/HasForeignKey`.

Template (model on `Shared.Persistence/Configuration/UserConfig.cs`):

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

        entity.HasOne(x => x.User)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.UserId);
    }
}
```

## 3. Two-way navigation (always)

Every FK relationship is mapped **two-way** — the owner exposes its children and both configs map the same FK explicitly.

On the **owner** entity (e.g. `Shared.Domain/Entities/User.cs`), add a collection nav with a private setter, initialised so it's never null:

```csharp
public virtual ICollection<World> Worlds { get; private set; } = [];
```

Then point both configs at the same FK:

```csharp
// child config (WorldConfig)
entity.HasOne(x => x.User)
    .WithMany(x => x.Worlds)
    .HasForeignKey(x => x.UserId);

// owner config (UserConfig) — same FK, owning side
entity.HasMany(x => x.Worlds)
    .WithOne(x => x.User)
    .HasForeignKey(x => x.UserId);
```

Both sides describe the same `World.UserId` FK; EF merges them into one relationship.

## After scaffolding

- **Add a migration** — it auto-applies on next startup (`app.ApplyDatabaseMigrationsAsync()` in `Program.cs`). See `Docs/EfMigrations.md`; migrations assembly is `Shared.Persistence`, startup project `WebApi.Presentation`. The `dotnet ef` command logs a benign `Sensitive data logging is enabled` warning in Development — it does not mean the migration failed.
- `dotnet build` — warnings are errors.

Next steps are usually: `add-secured-repo` (row-level security), then `add-feature` + `add-endpoint`.
