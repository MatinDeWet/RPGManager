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
- Foreign keys: a `long <Owner>Id` plus a `virtual <Owner>` nav property (model on `User`).

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
            .WithMany()
            .HasForeignKey(x => x.UserId);
    }
}
```

## After scaffolding

- If the owning side needs a collection nav (e.g. `User.Transactions`), add it to that entity and wire `WithMany(x => x.<Collection>)`.
- **Add a migration** — it auto-applies on next startup (`app.ApplyDatabaseMigrationsAsync()` in `Program.cs`). See `Docs/EfMigrations.md`; migrations assembly is `Shared.Persistence`, startup project `WebApi.Presentation`.
- `dotnet build` — warnings are errors.

Next steps are usually: `add-secured-repo` (row-level security), then `add-feature` + `add-endpoint`.
