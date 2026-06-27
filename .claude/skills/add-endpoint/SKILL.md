---
name: add-endpoint
description: Scaffold one minimal-API endpoint in the RPGManager WebApi Presentation layer and wire it up. Use when exposing a CQRS feature over HTTP (e.g. "add a GET /transactions/{id} endpoint"). Creates the RouteGroupBuilder extension with explicit binding attributes and Ardalis.Result mapping, registers it in the entity's endpoint group, and ensures the group is mapped under the versioned route prefix in MapApiEndpoints.
---

# Add a minimal-API endpoint

Minimal APIs grouped per entity. One file per endpoint, all collected in a `<Entity>Endpoints.cs` group, registered once in `MapApiEndpoints` (`Endpoints/EndpointRouteBuilderExtensions.cs`) under the URL-version prefix, which `Program.cs` invokes via `app.MapApiEndpoints();`. Auth is on by default (a global `FallbackPolicy` requires an authenticated user), so endpoints need no extra attribute unless they're public (`[AllowAnonymous]`).

## 1. Endpoint file — `Src/Services/WebApi/WebApi.Presentation/Endpoints/<Entity>Endpoints/<Name>Endpoint.cs`

- `internal static class` with a `Map<Name>Endpoint(this RouteGroupBuilder group)` extension; `.WithName(...)` + `.WithSummary(...)` + `.ProducesResult<TResponse>()` (or `.ProducesResult()` for no-content — see **Response metadata** below).
- Handler method is `private static async Task<Microsoft.AspNetCore.Http.IResult>`; call `handler.Handle(...)` then `return result.ToMinimalApiResult();` (`Ardalis.Result.AspNetCore`).
- **Be explicit about every input's binding source** (`using Microsoft.AspNetCore.Mvc;`): `[FromRoute]`, `[FromBody]`, `[FromQuery]`/`[AsParameters]`, and `[FromServices]` for injected handlers.

Route param (GET by id / DELETE):

```csharp
private static async Task<Microsoft.AspNetCore.Http.IResult> GetTransactionById(
    [FromRoute] long id,
    [FromServices] IQueryManager<GetTransactionByIdQuery, GetTransactionByIdResponse> handler,
    CancellationToken cancellationToken)
{
    Result<GetTransactionByIdResponse> result = await handler.Handle(new GetTransactionByIdQuery(id), cancellationToken);
    return result.ToMinimalApiResult();
}
// route: group.MapGet("/{id:long}", GetTransactionById) … .ProducesResult<GetTransactionByIdResponse>();
```

Body (POST):

```csharp
private static async Task<Microsoft.AspNetCore.Http.IResult> CreateTransaction(
    [FromBody] CreateTransactionCommand command,
    [FromServices] ICommandManager<CreateTransactionCommand, CreateTransactionResponse> handler,
    CancellationToken cancellationToken) { ... }
// route: group.MapPost("/", CreateTransaction) … .ProducesResult<CreateTransactionResponse>();
```

Route + body (PUT) — id from route, fields from a body record; compose the command:

```csharp
private static async Task<Microsoft.AspNetCore.Http.IResult> UpdateTransaction(
    [FromRoute] long id,
    [FromBody] UpdateTransactionRequest request,
    [FromServices] ICommandManager<UpdateTransactionCommand> handler,
    CancellationToken cancellationToken)
{
    UpdateTransactionCommand command = new(id, request.Description, request.Amount);
    Result result = await handler.Handle(command, cancellationToken);
    return result.ToMinimalApiResult();
}

private sealed record UpdateTransactionRequest(string Description, decimal Amount);
```

Paginated query string — bind a **record struct** with `[AsParameters]` and constructor defaults (so params are optional and enums render as a Swagger dropdown):

```csharp
private static async Task<Microsoft.AspNetCore.Http.IResult> SearchTransactions(
    [AsParameters] SearchTransactionsRequest request,
    [FromServices] IQueryManager<SearchTransactionsQuery, PageableResponse<SearchTransactionsResponse>> handler,
    CancellationToken cancellationToken)
{
    SearchTransactionsQuery query = new()
    {
        PageNumber = request.PageNumber,
        PageSize = request.PageSize,
        OrderBy = request.OrderBy,
        OrderDirection = request.OrderDirection,
    };
    Result<PageableResponse<SearchTransactionsResponse>> result = await handler.Handle(query, cancellationToken);
    return result.ToMinimalApiResult();
}

private readonly record struct SearchTransactionsRequest(
    [property: FromQuery] int PageNumber = 1,
    [property: FromQuery] int PageSize = 10,
    [property: FromQuery] string? OrderBy = null,
    [property: FromQuery] OrderDirectionEnum OrderDirection = OrderDirectionEnum.Ascending);
```

> Do NOT put `[AsParameters]` on a *class* with field-initializer defaults — OpenAPI marks its non-nullable value-type members **required** and Swagger blocks the call. A record struct with constructor defaults keeps them optional.

### Response metadata — always chain `.ProducesResult<T>()`

The handler returns a bare `IResult`, so the OpenAPI generator can't infer the response type or error shapes on its own — without this the document is untyped and client codegen (NSwag/Kiota/openapi-generator) emits `object`/`void`. Chain the shared helper from `WebApi.Presentation.Common.Extensions` (`using WebApi.Presentation.Common.Extensions;`) on the `Map<Name>Endpoint` call. It declares a typed **200** plus the standard `400/401/403/404` `application/problem+json` responses in one call:

```csharp
// body endpoint (Result<TResponse>) — 200 returns TResponse
group.MapGet("/{id:long}", GetTransactionById)
    .WithName("GetTransactionById")
    .WithSummary("Returns a single transaction owned by the current user.")
    .ProducesResult<GetTransactionByIdResponse>();

// no-content endpoint (plain Result — delete/update/etc.) — 200 with no body
group.MapDelete("/{id:long}", DeleteTransaction)
    .WithName("DeleteTransaction")
    .WithSummary("Deletes a transaction owned by the current user.")
    .ProducesResult();
```

Use `.ProducesResult<TResponse>()` whenever the handler is `IQueryManager<…, TResponse>` / `ICommandManager<…, TResponse>` (the `TResponse` is the success body; for a paged endpoint it's `PageableResponse<…>`), and the parameterless `.ProducesResult()` when the handler is a non-generic `ICommandManager<…>`. Helper source: `Src/Services/WebApi/WebApi.Presentation/Common/Extensions/EndpointMetadataExtensions.cs` (extend it there if the standard status set ever needs to change — don't add per-endpoint `.Produces*` noise).

## 2. Register in the group — `Endpoints/<Entity>Endpoints/<Entity>Endpoints.cs`

`public static class` with `Map<Entity>Endpoints(this IEndpointRouteBuilder app)`: `app.MapGroup("/<entities>").WithTags("<Entities>")`, then call each `group.Map<Name>Endpoint();`. Create this file the first time you add the entity's first endpoint.

**Sub-resources get their own tag via a dedicated group builder — not a per-endpoint `.WithTags`.** When an entity's routes split into sub-resources (e.g. `/campaigns/{id}/members`, `/campaigns/{id}/invitations`), declare a **sibling group on the same prefix** with its own tag so OpenAPI groups them separately. Set the tag once on the group builder; don't repeat `.WithTags(...)` on each endpoint:

```csharp
public static IEndpointRouteBuilder MapCampaignEndpoints(this IEndpointRouteBuilder app)
{
    RouteGroupBuilder group = app.MapGroup("/campaigns").WithTags("Campaigns");
    group.MapGetCampaignByIdEndpoint(); // …campaign CRUD…

    RouteGroupBuilder members = app.MapGroup("/campaigns").WithTags("Campaign Members");
    members.MapGetCampaignMembersEndpoint();   // route: "/{id:long}/members"
    members.MapKickCampaignMemberEndpoint();    // route: "/{id:long}/members/{userId:long}"

    RouteGroupBuilder invitations = app.MapGroup("/campaigns").WithTags("Campaign Invitations");
    invitations.MapCreateInvitationEndpoint();  // route: "/{id:long}/invitations"

    return app;
}
```

Routes are unchanged (each endpoint still maps its full pattern); the extra group builders only carry the tag. A literal segment (`/members/me`) and a constrained param (`/members/{userId:long}`) on the same path don't conflict. A flow keyed by something other than the parent id (e.g. an invite token) belongs in its **own** top-level group + file (`InvitationEndpoints` on `/invitations`, mapped separately in `MapApiEndpoints` — see §3).

## 3. Map the group in `MapApiEndpoints` — `Endpoints/EndpointRouteBuilderExtensions.cs`

Endpoint groups are **not** mapped in `Program.cs` (which only calls `app.MapApiEndpoints();`). Register a new entity group inside `MapApiEndpoints`, on the **`versioned`** builder — never on `app` directly, or it skips versioning:

```csharp
public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
{
    // … builds a shared ApiVersionSet from ApiVersions.All …
    IEndpointRouteBuilder versioned = app.MapGroup("/v{version:apiVersion}").WithApiVersionSet(apiVersionSet);

    versioned.MapUserEndpoints();
    versioned.MapWorldEndpoints();
    versioned.Map<Entity>Endpoints();   // <-- add here (+ the `using` for the namespace)
    return app;
}
```

The `/v{version:apiVersion}` prefix + version set **propagate into your group**, so the group file (§2) keeps its plain `MapGroup("/<entities>")` and the endpoint is served under `/v1/<entities>` automatically — don't add a version segment or `.WithApiVersionSet`/`.HasApiVersion` yourself. Publishing a new API version is a one-line addition to `ApiVersions.All` (`Common/ApiVersions.cs`), not a per-endpoint change.

## Notes

- `ToMinimalApiResult()` maps the `Ardalis.Result` status to HTTP (200/201, 400, 404, 403, …) and carries the handler's descriptive `Result.NotFound(...)`/`Result.Forbidden(...)` messages into the response — so set good messages in the handler (see `add-feature`), not at the endpoint. A non-`Result` failure like an `UnauthorizedAccessException` thrown by a secured write is turned into a 403 by the global exception handler (see `add-secured-repo`).
- Enums serialize as text app-wide via `JsonStringEnumConverter` (registered in `ServiceCollectionExtensions`); new enums get string values + Swagger dropdowns for free. Restart + hard-refresh Swagger to pick up schema changes (it caches `/openapi/v1.json`).
- `dotnet build` — warnings are errors.
