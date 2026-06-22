---
name: add-endpoint
description: Scaffold one minimal-API endpoint in the RPGManager WebApi Presentation layer and wire it up. Use when exposing a CQRS feature over HTTP (e.g. "add a GET /transactions/{id} endpoint"). Creates the RouteGroupBuilder extension with explicit binding attributes and Ardalis.Result mapping, registers it in the entity's endpoint group, and ensures the group is mapped in Program.cs.
---

# Add a minimal-API endpoint

Minimal APIs grouped per entity. One file per endpoint, all collected in a `<Entity>Endpoints.cs` group, mapped once in `Program.cs`. Auth is on by default (a global `FallbackPolicy` requires an authenticated user), so endpoints need no extra attribute unless they're public (`[AllowAnonymous]`).

## 1. Endpoint file — `Src/Services/WebApi/WebApi.Presentation/Endpoints/<Entity>Endpoints/<Name>Endpoint.cs`

- `internal static class` with a `Map<Name>Endpoint(this RouteGroupBuilder group)` extension; `.WithName(...)` + `.WithSummary(...)`.
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
// route: group.MapGet("/{id:long}", GetTransactionById)
```

Body (POST):

```csharp
private static async Task<Microsoft.AspNetCore.Http.IResult> CreateTransaction(
    [FromBody] CreateTransactionCommand command,
    [FromServices] ICommandManager<CreateTransactionCommand, CreateTransactionResponse> handler,
    CancellationToken cancellationToken) { ... }
// route: group.MapPost("/", CreateTransaction)
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

Routes are unchanged (each endpoint still maps its full pattern); the extra group builders only carry the tag. A literal segment (`/members/me`) and a constrained param (`/members/{userId:long}`) on the same path don't conflict. A flow keyed by something other than the parent id (e.g. an invite token) belongs in its **own** top-level group + file (`InvitationEndpoints` on `/invitations`, mapped separately in `Program.cs`).

## 3. Map the group in `Program.cs`

Add `app.Map<Entity>Endpoints();` alongside `app.MapUserEndpoints();` / `app.MapCardEndpoints();` (and the `using` for the namespace).

## Notes

- `ToMinimalApiResult()` maps the `Ardalis.Result` status to HTTP (200/201, 400, 404, 403, …) and carries the handler's descriptive `Result.NotFound(...)`/`Result.Forbidden(...)` messages into the response — so set good messages in the handler (see `add-feature`), not at the endpoint. A non-`Result` failure like an `UnauthorizedAccessException` thrown by a secured write is turned into a 403 by the global exception handler (see `add-secured-repo`).
- Enums serialize as text app-wide via `JsonStringEnumConverter` (registered in `ServiceCollectionExtensions`); new enums get string values + Swagger dropdowns for free. Restart + hard-refresh Swagger to pick up schema changes (it caches `/openapi/v1.json`).
- `dotnet build` — warnings are errors.
