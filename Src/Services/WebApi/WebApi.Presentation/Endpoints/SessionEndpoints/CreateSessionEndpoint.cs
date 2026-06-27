using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using CQRS.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using WebApi.Application.Features.SessionFeatures.CreateSession;
using WebApi.Presentation.Common.Extensions;

namespace WebApi.Presentation.Endpoints.SessionEndpoints;

internal static class CreateSessionEndpoint
{
    public static RouteGroupBuilder MapCreateSessionEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost("/", CreateSession)
            .WithName("CreateSession")
            .WithSummary("Creates a new session in a campaign. Requires the Dungeon Master role.")
            .ProducesResult<long>();

        return group;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> CreateSession(
        [FromBody] CreateSessionCommand command,
        [FromServices] ICommandManager<CreateSessionCommand, long> handler,
        CancellationToken cancellationToken)
    {
        Result<long> result = await handler.Handle(command, cancellationToken);

        return result.ToMinimalApiResult();
    }
}
