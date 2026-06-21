---
name: add-search
description: Add PostgreSQL ILIKE or full-text filtering to a RPGManager query feature using MatinDeWet.Searchable.PostgreSQL. Use when a feature should filter by a free-text term (e.g. "search worlds by name"). Covers implementing ISearchableRequest, ILikeSearch/FullTextSearch, match modes, and the backing trigram/tsvector index. Composes with add-pagination and add-feature.
---

# Add text search to a query feature

Built on `MatinDeWet.Searchable.PostgreSQL` (already referenced by `WebApi.Application`). It builds PostgreSQL search predicates over `IQueryable<T>` from a request that carries the raw search term.

## 1. The query carries the term — implement `ISearchableRequest`

`Searchable.PostgreSQL.Contracts`. **`SearchTerm` is `string?`** — declaring it non-nullable is a build error (CS8767, nullability mismatch):

```csharp
using Searchable.PostgreSQL.Contracts;

public sealed class SearchWorldsQuery
    : PageableRequest, IQuery<PageableResponse<SearchWorldsResponse>>, ISearchableRequest
{
    public string? SearchTerm { get; init; }
}
```

(Searchable exposes only the `ISearchableRequest` interface — there is no concrete request type to instantiate; the query *is* the request.)

## 2. Apply the filter in the handler

Guard on a non-blank term, then filter **before** the projection so the predicate translates server-side:

```csharp
using Searchable.PostgreSQL;          // ILikeSearch / FullTextSearch
using Searchable.PostgreSQL.Enums;    // ILikeMatchModeEnum

IQueryable<World> worlds = queryRepo.Worlds;

if (!string.IsNullOrWhiteSpace(request.SearchTerm))
{
    worlds = worlds.ILikeSearch(request, x => x.Name, ILikeMatchModeEnum.Contains);
}
```

Match modes (`ILikeMatchModeEnum`): `Contains` (`%term%`), `StartsWith` (`term%`), `EndsWith` (`%term`), `Exact`. Across multiple columns:

```csharp
worlds = worlds.ILikeSearch(request, [x => x.Name, x => x.Summary], ILikeMatchModeEnum.Contains, useOrLogic: true);
```

Full-text over a mapped `NpgsqlTsVector` column instead of ILIKE:

```csharp
worlds = worlds.FullTextSearch(request, x => x.SearchVector, language: "english");
```

## 3. Back it with an index (see `add-entity-config`)

ILIKE substring search is only index-backed with a **GIN + pg_trgm** index on the column (the `pg_trgm` extension is enabled in `CoreContext.OnModelCreating`). In the entity config:

```csharp
entity.HasIndex(x => x.Name)
    .HasMethod("gin")
    .HasOperators("gin_trgm_ops");
```

Full-text instead wants a mapped tsvector column (`HasGeneratedTsVectorColumn`) with its own GIN index. Add the index in the entity config and create a migration (see `add-entity-config`).

## 4. Endpoint — expose the term

Add `SearchTerm` to the search endpoint's `[AsParameters]` record struct and map it onto the query:

```csharp
private readonly record struct SearchWorldsRequest(
    // ...paging params...
    [property: FromQuery] string? SearchTerm = null);

// in the handler method: SearchTerm = request.SearchTerm
```

## Compose

- Pair with **`add-pagination`** — the usual case is a paged, filtered list (both interfaces on one query).
- See **`add-feature`** for the base query/handler.
- `dotnet build` — warnings are errors.
