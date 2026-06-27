using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using CQRS.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using WebApi.Application.Features.SessionFeatures.UpdateSession;
using WebApi.Presentation.Common.Extensions;

namespace WebApi.Presentation.Endpoints.SessionEndpoints;

internal static class UpdateSessionEndpoint
{
    public static RouteGroupBuilder MapUpdateSessionEndpoint(this RouteGroupBuilder group)
    {
        group.MapPut("/{id:long}", UpdateSession)
            .WithName("UpdateSession")
            .WithSummary("Updates the title, scheduled time and summary of a session. Requires the Dungeon Master role.")
            .ProducesResult();

        return group;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> UpdateSession(
        [FromRoute] long id,
        [FromBody] UpdateSessionRequest request,
        [FromServices] ICommandManager<UpdateSessionCommand> handler,
        CancellationToken cancellationToken)
    {
        UpdateSessionCommand command = new(id, request.Title, request.ScheduledAt, request.Summary);

        Result result = await handler.Handle(command, cancellationToken);

        return result.ToMinimalApiResult();
    }

    private sealed record UpdateSessionRequest(string Title, DateTimeOffset ScheduledAt, string? Summary);
}
