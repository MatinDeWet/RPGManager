using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using CQRS.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using WebApi.Application.Features.SessionFeatures.DeleteSession;
using WebApi.Presentation.Common.Extensions;

namespace WebApi.Presentation.Endpoints.SessionEndpoints;

internal static class DeleteSessionEndpoint
{
    public static RouteGroupBuilder MapDeleteSessionEndpoint(this RouteGroupBuilder group)
    {
        group.MapDelete("/{id:long}", DeleteSession)
            .WithName("DeleteSession")
            .WithSummary("Deletes a session. Requires the Dungeon Master role.")
            .ProducesResult();

        return group;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> DeleteSession(
        [FromRoute] long id,
        [FromServices] ICommandManager<DeleteSessionCommand> handler,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(new DeleteSessionCommand(id), cancellationToken);

        return result.ToMinimalApiResult();
    }
}
