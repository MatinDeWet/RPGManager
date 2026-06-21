---
name: add-feature
description: Scaffold one CQRS feature (a query or a command) in the RPGManager WebApi Application layer. Use when adding a single use case (e.g. "add a GetTransactionById query" or "add a CreateTransaction command"). Creates the request record, the internal sealed handler returning Ardalis.Result, and the response record — auto-registered by Scrutor, no manual DI.
---

# Add a CQRS feature

Custom CQRS via `CQRS.Core` (not MediatR). One folder per feature under `Src/Services/WebApi/WebApi.Application/Features/<Entity>Features/<FeatureName>/`, containing the request, handler, and (if any) response. Handlers are auto-registered by `AddCQRSSupport` (Scrutor) — never wire DI by hand.

## Contracts (in `CQRS.Core.Contracts`)

| Request | Handler | Returns |
|---|---|---|
| `IQuery<TResponse>` | `IQueryManager<TQuery, TResponse>` | `Task<Result<TResponse>>` |
| `ICommand<TResponse>` | `ICommandManager<TCommand, TResponse>` | `Task<Result<TResponse>>` |
| `ICommand` (no payload) | `ICommandManager<TCommand>` | `Task<Result>` |

Requests are `public sealed record`. Handlers are `internal sealed class`. Use `Ardalis.Result` — `Result.NotFound()`, `Result.Success()`, or return the value directly (implicit conversion to `Result<T>`).

**Give failure results a descriptive message** (the convention across the codebase): pass a sentence to `Result.NotFound(...)` / `Result.Forbidden(...)` naming the resource and why it failed — e.g. `Result.NotFound($"Campaign '{request.Id}' was not found or the current user is not a member of it.")`. The message surfaces to the client through `ToMinimalApiResult` (see `add-endpoint`). Reserve bare `Result.NotFound()` for cases with nothing useful to add.

## Repositories to inject

- Reads: `ISecuredQueryRepo` (row-level filtered to the current user) — `WebApi.Application.Repositories.QueryRepos.SecuredRepos`. Its queryables (e.g. `queryRepo.Transactions`) are already user-scoped.
- Writes: `ISecuredCommandRepo` (`WebApi.Application.Repositories.CommandRepos.SecuredRepos`) — `InsertAsync/UpdateAsync/DeleteAsync` (each with a `persistImmediately` overload) + `SaveAsync`. It runs the entity's `Lock.HasAccess` before staging.
- Current user id: inject `IIdentityInfo` (`Identification.Contracts`) → `GetInternalUserId()`. Needed on Create to set the owner FK.
- Pre-auth only: the `IUnsecured*` repos.

## Templates

**Query** (model on `UserFeatures/GetUser`):

```csharp
// GetTransactionByIdQuery.cs
using CQRS.Core.Contracts;
namespace WebApi.Application.Features.TransactionFeatures.GetTransactionById;
public sealed record GetTransactionByIdQuery(long Id) : IQuery<GetTransactionByIdResponse>;

// GetTransactionByIdResponse.cs
namespace WebApi.Application.Features.TransactionFeatures.GetTransactionById;
public sealed record GetTransactionByIdResponse(long Id, string Description, decimal Amount, DateTimeOffset DateCreated);

// GetTransactionByIdQueryHandler.cs
using Ardalis.Result;
using CQRS.Core.Contracts;
using Microsoft.EntityFrameworkCore;
using WebApi.Application.Repositories.QueryRepos.SecuredRepos;
namespace WebApi.Application.Features.TransactionFeatures.GetTransactionById;

internal sealed class GetTransactionByIdQueryHandler(ISecuredQueryRepo queryRepo)
    : IQueryManager<GetTransactionByIdQuery, GetTransactionByIdResponse>
{
    public async Task<Result<GetTransactionByIdResponse>> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        GetTransactionByIdResponse? item = await queryRepo.Transactions
            .Where(x => x.Id == request.Id)
            .Select(x => new GetTransactionByIdResponse(x.Id, x.Description, x.Amount, x.DateCreated))
            .FirstOrDefaultAsync(cancellationToken);

        return item is null
            ? Result.NotFound($"Transaction '{request.Id}' was not found or is not owned by the current user.")
            : item;
    }
}
```

**Create command** (sets owner from identity; `var` because the type is apparent — IDE0007):

```csharp
internal sealed class CreateTransactionCommandHandler(
    ISecuredCommandRepo commandRepo,
    IIdentityInfo identityInfo) : ICommandManager<CreateTransactionCommand, CreateTransactionResponse>
{
    public async Task<Result<CreateTransactionResponse>> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var item = Transaction.Create(identityInfo.GetInternalUserId(), request.Description, request.Amount);
        await commandRepo.InsertAsync(item, persistImmediately: true, cancellationToken);
        return new CreateTransactionResponse(item.Id);
    }
}
```

**Update/Delete** (`ICommand` → `Result`): load via `queryRepo.<X>.FirstOrDefaultAsync(...)` → descriptive `Result.NotFound(...)` if null → mutate via the entity's `Update(...)` then `UpdateAsync(...)` (or `DeleteAsync(...)`), `persistImmediately: true` → `Result.Success()`. When write access is narrower than read access (e.g. role-gated edits), let the entity's `Lock.HasAccess` reject the write — it becomes a 403 via the global handler (see `add-secured-repo`) — rather than re-checking in the handler.

## List, paginated, and searchable features

This skill covers the base query/command shape. For list features, compose the dedicated skills:

- **`add-pagination`** — return a page of results (`PageableRequest` query, member-init DTO, `ToPageableListAsync`, `[AsParameters]` endpoint binding).
- **`add-search`** — add free-text filtering (`ISearchableRequest` on the query, `ILikeSearch`/`FullTextSearch`, backing trigram/tsvector index).

The two compose: a paged, name-filtered list uses both (one query implements `PageableRequest` *and* `ISearchableRequest`).

## After scaffolding

`dotnet build` (warnings-as-errors). Then expose it over HTTP with `add-endpoint`.
