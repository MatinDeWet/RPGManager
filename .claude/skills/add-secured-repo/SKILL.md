---
name: add-secured-repo
description: Wire a domain entity into RPGManager's secured repository + lock (row-level security) pattern. Use when an entity's data must be scoped to the current user (e.g. "secure the Transaction entity so users only see their own"). Creates a Lock<T> (read filter + write access check), exposes a queryable on ISecuredQueryRepo, and adds matching lock unit tests.
---

# Secure an entity with a Lock

RPGManager enforces row-level security in-house (under `Src/Utilities/Repository/`), not via a NuGet. Each entity reachable through a secured repo needs exactly one `Lock<T>` (fail-closed: no lock = access refused). Writes are enforced generically by `SecuredCommandRepo`; you only add the lock and a read queryable.

The current user's internal id comes from `IIdentityInfo.GetInternalUserId()` (a `long`), populated per request by `CurrentUserMiddleware`.

## 1. Lock — `Src/Services/WebApi/WebApi.infrastructure/Repositories/Locks/<Name>Lock.cs`

Auto-registers via `AddSecuredRepositories` (Scrutor scans `IProtected`) — no manual DI. Model on `UserLock.cs` / `CardLock.cs`.

```csharp
using Repository.Enums;
using Repository.Lock;
using WebApi.Domain.Entities;
using WebApi.infrastructure.Data.Contexts;

namespace WebApi.infrastructure.Repositories.Locks;

/// <summary>
/// Row-level protection for <see cref="Transaction"/>: a user may only see and modify their own.
/// </summary>
internal sealed class TransactionLock(CoreContext context) : Lock<Transaction>
{
    public override IQueryable<Transaction> Secured(long userId)
    {
        return context.Set<Transaction>().Where(x => x.UserId == userId);
    }

    public override Task<bool> HasAccess(Transaction obj, long userId, RepositoryOperationEnum operation, CancellationToken cancellationToken)
    {
        return Task.FromResult(obj.UserId == userId);
    }
}
```

`Secured` is the read filter; `HasAccess` gates Insert/Update/Delete (throws `UnauthorizedAccessException` on deny). For richer rules (e.g. shared ownership via a join), query `context` inside these methods — see the EntitySecurity pattern.

## 2. Expose a read queryable on the secured query repo

- Interface — `Src/Services/WebApi/WebApi.Application/Repositories/QueryRepos/SecuredRepos/ISecuredQueryRepo.cs`: add `IQueryable<Transaction> Transactions { get; }`.
- Impl — `Src/Services/WebApi/WebApi.infrastructure/Repositories/QueryRepos/SecuredRepos/SecuredQueryRepo.cs`: add `public IQueryable<Transaction> Transactions => GetQueryable<Transaction>();`.

`GetQueryable<T>()` resolves the lock and applies `Secured(currentUserId)` automatically.

## 3. Lock unit tests (match existing coverage)

The repo tests every lock — mirror `UserLockTests`/`CardLockTests`.

- Test double — `Tests/Services/WebApi/WebApi.infrastructure.UnitTests/TestDoubles/Test<Name>.cs`: build instances and set the DB-generated `Id` via reflection (see `TestCard`).
- Tests — `.../Repositories/Locks/<Name>LockTests.cs`: cover `Secured` (returns only owned rows / empty when none), `HasAccess` true/false across all `RepositoryOperationEnum` values (`[Theory]`/`[InlineData]`), and `IsMatch` for the type and an unrelated type. Uses `MockQueryable.NSubstitute` (`BuildMockDbSet()`), `NSubstitute`, `Shouldly`, `xunit.v3` (`TestContext.Current.CancellationToken`).

## Notes

- **Pre-auth flows only** (e.g. login-time upsert) use the *unsecured* repos (`IUnsecuredQueryRepo`/`IUnsecuredCommandRepo`). Everything user-facing uses the secured ones.
- `dotnet build` (warnings-as-errors) and run `dotnet test` for the infrastructure test project to confirm the new lock tests pass.
