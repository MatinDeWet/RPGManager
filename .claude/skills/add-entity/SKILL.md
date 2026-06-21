---
name: add-entity
description: Scaffold a new persistence-ignorant domain entity in the RPGManager solution and its EF Core mapping. Use when adding a new database-backed entity (e.g. "add a Transaction entity with amount and date"). Creates the Entity<long> domain type (private setters, Create/Update factories, guard-clause validation, two-way navigations) and then creates the matching IEntityTypeConfiguration via the add-entity-config skill, then reminds you to add a migration.
---

# Add a domain entity (+ EF configuration)

Two files, layered: the **entity** in `Shared.Domain` (this skill), the **mapping** in `Shared.Persistence` (delegated to the `add-entity-config` skill). Both services share these through the common kernel. Keep the domain type **persistence-ignorant** — no EF attributes, no Fluent API on the entity.

## Default: relationships are two-way

**Unless the user explicitly says otherwise, every foreign key gets a two-way navigation.** The dependent entity exposes a reference nav (`virtual <Owner>`), the principal exposes a collection nav (`virtual ICollection<Child>`), and `add-entity-config` maps **both** sides over the same FK. Do not produce a one-directional relationship unless asked.

## 1. Entity — `Src/Shared/Shared.Domain/Entities/<Name>.cs`

- Inherit `Entity<long>` (from `Domain.Implementation`) — gives `Id` (protected setter, DB-generated) and `DateCreated`.
- **Private setters** on every property.
- Mutate only through a static `Create` factory and instance `Update` methods.
- Validate inputs with the shared guard clauses (`Guard.Against.ValidString(...)`, `using Ardalis.GuardClauses;` + `using Domain.Extensions;`). Put each validation in a `private static` helper so `Create` and `Update` share it.
- **Foreign keys:** a `long <Owner>Id` plus a `virtual <Owner>` nav property — **and** add the matching collection nav on the owner entity (see section 2). Model on `Shared.Domain/Entities/World.cs` ↔ `User`.

Template:

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

(In the config, leave the property without `.IsRequired()` so the column is nullable.)

## 2. Add the two-way navigation on the owner

On the **owner** entity (e.g. `Shared.Domain/Entities/User.cs`), add a collection nav with a private setter, initialised so it's never null:

```csharp
public virtual ICollection<Transaction> Transactions { get; private set; } = [];
```

`add-entity-config` then maps both `HasOne/WithMany` (child config) and `HasMany/WithOne` (owner config) over the same FK. Both halves — the collection nav here and the two mappings — always travel together.

## 3. Join / link entities (many-to-many with a role or payload)

For an entity that links two others (e.g. `CampaignMember` linking `Campaign` and `User` with a `Role`):

- Inherit the **non-generic `Entity`** (not `Entity<long>`) when the pair is the natural key — this keeps `DateCreated` but drops the surrogate `Id`. The config keys it on the pair (`HasKey(x => new { x.CampaignId, x.UserId })`).
- Hold both FK ids + both reference navs (`virtual Campaign`, `virtual User`); add the matching collection nav on **each** owner (`Campaign.Members`, `User.CampaignMemberships`).
- The `Create` factory can enforce invariants (e.g. an aggregate root's `Create` adds the first link to its collection so EF cascade-inserts it).

```csharp
public class CampaignMember : Entity
{
    public long CampaignId { get; private set; }
    public virtual Campaign Campaign { get; private set; } = null!;
    public long UserId { get; private set; }
    public virtual User User { get; private set; } = null!;
    public CampaignRole Role { get; private set; }

    public static CampaignMember Create(long userId, CampaignRole role) =>
        new() { UserId = userId, Role = role };
}
```

## 4. Create the EF configuration

Use the **`add-entity-config`** skill to scaffold `Src/Shared/Shared.Persistence/Configuration/<Name>Config.cs` (table, key(s), column constraints, the two-way relationship mappings, composite keys, and any indexes). Don't hand-roll the mapping here — that skill carries the current conventions.

## After scaffolding

- **Add a migration** with the `add-migration` skill — it auto-applies on next WebApi startup (`app.ApplyDatabaseMigrationsAsync()`).
- `dotnet build` — warnings are errors.

Next steps are usually: `add-secured-repo` (row-level security), then `add-feature` + `add-endpoint`.
