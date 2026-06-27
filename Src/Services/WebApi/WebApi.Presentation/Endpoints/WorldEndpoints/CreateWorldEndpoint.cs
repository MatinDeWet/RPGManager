using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using CQRS.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using WebApi.Application.Features.WorldFeatures.CreateWorld;
using WebApi.Presentation.Common.Extensions;

namespace WebApi.Presentation.Endpoints.WorldEndpoints;

internal static class CreateWorldEndpoint
{
    public static RouteGroupBuilder MapCreateWorldEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost("/", CreateWorld)
            .WithName("CreateWorld")
            .WithSummary("Creates a new world owned by the current user.")
            .ProducesResult<long>();

        return group;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> CreateWorld(
        [FromBody] CreateWorldCommand command,
        [FromServices] ICommandManager<CreateWorldCommand, long> handler,
        CancellationToken cancellationToken)
    {
        Result<long> result = await handler.Handle(command, cancellationToken);

        return result.ToMinimalApiResult();
    }
}
