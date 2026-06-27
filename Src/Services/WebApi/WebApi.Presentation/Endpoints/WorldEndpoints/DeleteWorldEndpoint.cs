using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using CQRS.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using WebApi.Application.Features.WorldFeatures.DeleteWorld;
using WebApi.Presentation.Common.Extensions;

namespace WebApi.Presentation.Endpoints.WorldEndpoints;

internal static class DeleteWorldEndpoint
{
    public static RouteGroupBuilder MapDeleteWorldEndpoint(this RouteGroupBuilder group)
    {
        group.MapDelete("/{id:long}", DeleteWorld)
            .WithName("DeleteWorld")
            .WithSummary("Deletes a world owned by the current user.")
            .ProducesResult();

        return group;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> DeleteWorld(
        [FromRoute] long id,
        [FromServices] ICommandManager<DeleteWorldCommand> handler,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(new DeleteWorldCommand(id), cancellationToken);

        return result.ToMinimalApiResult();
    }
}
