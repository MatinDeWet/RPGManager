---
name: add-secured-repo
description: Wire a domain entity into RPGManager's secured repository + lock (row-level security) pattern. Use when an entity's data must be scoped to the current user (e.g. "secure the Transaction entity so users only see their own"). Creates a Lock<T> (read filter + write access check), exposes a queryable on ISecuredQueryRepo, and adds matching lock unit tests.
---

# Secure an entity with a Lock

RPGManager enforces row-level security in-house (under `Src/Utilities/Repository/`), not via a NuGet. Each entity reachable through a secured repo needs exactly one `Lock<T>` (fail-closed: no lock = access refused). Writes are enforced generically by `SecuredCommandRepo`; you only add the lock and a read queryable.

The current user's internal id comes from `IIdentityInfo.GetInternalUserId()` (a `long`), populated per request by `CurrentUserMiddleware`.

## 1. Lock — `Src/Services/WebApi/WebApi.infrastructure/Repositories/Locks/<Name>Lock.cs`

Auto-registers via `AddSecuredRepositories` (Scrutor scans `IProtected`) — no manual DI. Model on `UserLock.cs`.

```csharp
using Repository.Enums;
using Repository.Lock;
using Shared.Domain.Entities;
using Shared.Persistence.Data.Contexts;

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

`Secured` is the read filter; `HasAccess` gates Insert/Update/Delete (throws `UnauthorizedAccessException` on deny). The simple case above is direct ownership — read access equals write access. For anything richer, see the next section.

## Advanced lock patterns

`Secured` and `HasAccess` are plain methods with the `CoreContext`, so they can express access derived from other tables and vary by operation. Modeled on `CampaignLock` (a user reads campaigns they're a **member** of, but only a **DungeonMaster** may modify).

**Membership/join-based read — drive the query from the join table.** Filter the join by `userId` and join out to the entity, so the planner uses the join's FK index (e.g. `IX_CampaignMember_UserId`) instead of scanning every row. A unique `(EntityId, UserId)` key keeps the result duplicate-free, so no `Distinct()` is needed:

```csharp
public override IQueryable<Campaign> Secured(long userId)
{
    return from member in context.Set<CampaignMember>()
           join campaign in context.Set<Campaign>() on member.CampaignId equals campaign.Id
           where member.UserId == userId
           select campaign;
}
```

**Operation- and role-dependent write — branch in `HasAccess`.** Authorize `Insert` up front when the row/membership doesn't exist yet (the create handler is responsible for that check — e.g. "caller owns the parent"). Require a role only for mutating ops, appending the filter conditionally for clean SQL over the composite key:

```csharp
public override async Task<bool> HasAccess(Campaign obj, long userId, RepositoryOperationEnum operation, CancellationToken cancellationToken)
{
    if (operation == RepositoryOperationEnum.Insert)
    {
        return true; // membership doesn't exist yet; create handler enforces world ownership
    }

    IQueryable<CampaignMember> membership = context.Set<CampaignMember>()
        .Where(m => m.CampaignId == obj.Id && m.UserId == userId);

    if (operation is RepositoryOperationEnum.Update or RepositoryOperationEnum.Delete)
    {
        membership = membership.Where(m => m.Role == CampaignRole.DungeonMaster);
    }

    return await membership.AnyAsync(cancellationToken);
}
```

When the create handler authorizes the insert (rather than the lock), build the first link in the aggregate's `Create` so EF cascade-inserts it with the root in one save — no `Lock<JoinEntity>` needed, since the secured command repo only authorizes the *root* passed to `InsertAsync` (see `add-entity`).

## Global 403 handler (required when read access ⊋ write access)

If a user can **read** a row they cannot **write** (e.g. a Player loading a campaign they may not edit), the secured command repo's denied write throws `UnauthorizedAccessException`. Without a handler that becomes a **500**. Map it once, app-wide, to a clean **403** — don't re-check the role in the handler.

`Src/Services/WebApi/WebApi.Presentation/Common/ExceptionHandling/UnauthorizedAccessExceptionHandler.cs`:

```csharp
internal sealed class UnauthorizedAccessExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not UnauthorizedAccessException)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails { Status = StatusCodes.Status403Forbidden, Title = "Forbidden" },
        });
    }
}
```

Register it in `ServiceCollectionExtensions` (`services.AddProblemDetails();` + `services.AddExceptionHandler<UnauthorizedAccessExceptionHandler>();`) and add `app.UseExceptionHandler();` as the first middleware in `Program.cs`. This already exists — reuse it; only direct-ownership entities (read == write) never trip it.

## 2. Expose a read queryable on the secured query repo

- Interface — `Src/Services/WebApi/WebApi.Application/Repositories/QueryRepos/SecuredRepos/ISecuredQueryRepo.cs`: add `IQueryable<Transaction> Transactions { get; }`.
- Impl — `Src/Services/WebApi/WebApi.infrastructure/Repositories/QueryRepos/SecuredRepos/SecuredQueryRepo.cs`: add `public IQueryable<Transaction> Transactions => GetQueryable<Transaction>();`.

`GetQueryable<T>()` resolves the lock and applies `Secured(currentUserId)` automatically.

## 3. Lock unit tests (match existing coverage)

The repo tests every lock — mirror `UserLockTests`.

- Test double — `Tests/Services/WebApi/WebApi.infrastructure.UnitTests/TestDoubles/Test<Name>.cs`: build instances and set the DB-generated `Id` via reflection (see `TestCard`).
- Tests — `.../Repositories/Locks/<Name>LockTests.cs`: cover `Secured` (returns only owned rows / empty when none), `HasAccess` true/false across all `RepositoryOperationEnum` values (`[Theory]`/`[InlineData]`), and `IsMatch` for the type and an unrelated type. Uses `MockQueryable.NSubstitute` (`BuildMockDbSet()`), `NSubstitute`, `Shouldly`, `xunit.v3` (`TestContext.Current.CancellationToken`).
- **Advanced locks query a second set** (the join table): stub **both** with `_context.Set<Entity>().Returns(...)` and `_context.Set<JoinEntity>().Returns(...)`, and build the join rows in a test double (set FK ids the relationship would normally fill via reflection — see `TestCampaignMember`). Join on **ids**, not nav properties, in `Secured` so the in-memory mock resolves without wiring navigations. Cover the role matrix: DM passes Update/Delete, Player is denied them but allowed Read, `Insert` is allowed regardless, and a non-member is denied.

## Notes

- **Pre-auth flows only** (e.g. login-time upsert) use the *unsecured* repos (`IUnsecuredQueryRepo`/`IUnsecuredCommandRepo`). Everything user-facing uses the secured ones.
- `dotnet build` (warnings-as-errors) and run `dotnet test` for the infrastructure test project to confirm the new lock tests pass.
