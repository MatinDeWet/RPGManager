using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using CQRS.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using WebApi.Application.Features.WorldFeatures.CreateWorld;

namespace WebApi.Presentation.Endpoints.WorldEndpoints;

internal static class CreateWorldEndpoint
{
    public static RouteGroupBuilder MapCreateWorldEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost("/", CreateWorld)
            .WithName("CreateWorld")
            .WithSummary("Creates a new world owned by the current user.");

        return group;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> CreateWorld(
        [FromBody] CreateWorldCommand command,
        [FromServices] ICommandManager<CreateWorldCommand, CreateWorldResponse> handler,
        CancellationToken cancellationToken)
    {
        Result<CreateWorldResponse> result = await handler.Handle(command, cancellationToken);

        return result.ToMinimalApiResult();
    }
}
