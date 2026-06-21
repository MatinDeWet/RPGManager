---
name: add-pagination
description: Make a RPGManager query feature return a page of results using MatinDeWet.Pagination. Use when a feature should return a paged/sorted list (e.g. "list worlds", "paginate the search results"). Covers the PageableRequest query, the member-init response DTO, the ToPageableListAsync projection, and the [AsParameters] endpoint binding. Composes with add-feature (base query/handler) and add-search (text filtering).
---

# Paginate a query feature

Built on `MatinDeWet.Pagination` (already referenced by `WebApi.Application`). A paginated feature is a normal CQRS query (see `add-feature`) whose request derives from `PageableRequest` and whose response is wrapped in `PageableResponse<T>`.

## 1. Query — derive from `PageableRequest`

Use a **class** (the base carries settable paging properties). `Pagination.Models.Requests` / `.Responses`:

```csharp
using CQRS.Core.Contracts;
using Pagination.Models.Requests;
using Pagination.Models.Responses;

namespace WebApi.Application.Features.WorldFeatures.SearchWorlds;

public sealed class SearchWorldsQuery : PageableRequest, IQuery<PageableResponse<SearchWorldsResponse>>;
```

`PageableRequest` gives `PageNumber`, `PageSize`, `OrderBy` (`string?`), and `OrderDirection` (`OrderDirectionEnum`, in `Pagination.Enums`).

## 2. Response — member-init record (NOT positional)

```csharp
public sealed record SearchWorldsResponse
{
    public long Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public DateTimeOffset DateCreated { get; init; }
}
```

> Critical: EF Core cannot translate the ordering the pagination helper applies **after a constructor projection**. `ToPageableListAsync` always orders, so paged DTOs must use member-init. (Single-item queries with no post-projection ordering may use positional records.)

## 3. Handler — project, then `ToPageableListAsync`

```csharp
using Pagination;                    // ToPageableListAsync
using Pagination.Models.Responses;

PageableResponse<SearchWorldsResponse> result = await queryRepo.Worlds
    .Select(x => new SearchWorldsResponse
    {
        Id = x.Id,
        Name = x.Name,
        DateCreated = x.DateCreated,
    })
    .ToPageableListAsync(x => x.Id, request, cancellationToken);

return result;
```

`ToPageableListAsync(keySelector, request, ct)` returns `PageableResponse<T>`. The key selector is the fallback ordering used when `OrderBy` is null.

## 4. Endpoint — bind a `[AsParameters]` record struct

Bind a **record struct** with constructor defaults (params stay optional; enums render as a Swagger dropdown), then build the query. Inject the handler with `[FromServices]` (the project convention — see `add-endpoint`):

```csharp
private static async Task<Microsoft.AspNetCore.Http.IResult> SearchWorlds(
    [AsParameters] SearchWorldsRequest request,
    [FromServices] IQueryManager<SearchWorldsQuery, PageableResponse<SearchWorldsResponse>> handler,
    CancellationToken cancellationToken)
{
    SearchWorldsQuery query = new()
    {
        PageNumber = request.PageNumber,
        PageSize = request.PageSize,
        OrderBy = request.OrderBy,
        OrderDirection = request.OrderDirection,
    };
    Result<PageableResponse<SearchWorldsResponse>> result = await handler.Handle(query, cancellationToken);
    return result.ToMinimalApiResult();
}

private readonly record struct SearchWorldsRequest(
    [property: FromQuery] int PageNumber = 1,
    [property: FromQuery] int PageSize = 10,
    [property: FromQuery] string? OrderBy = null,
    [property: FromQuery] OrderDirectionEnum OrderDirection = OrderDirectionEnum.Ascending);
```

> Do NOT put `[AsParameters]` on a *class* with field-initializer defaults — OpenAPI marks its non-nullable value-type members **required** and Swagger blocks the call. A record struct with constructor defaults keeps them optional.

## Compose

- Add a free-text filter with **`add-search`** (the query also implements `ISearchableRequest`; add a `SearchTerm` query param).
- See **`add-feature`** for the base query/handler/registration and **`add-endpoint`** for general endpoint wiring.
- `dotnet build` — warnings are errors.
