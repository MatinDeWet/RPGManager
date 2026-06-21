using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using CQRS.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using WebApi.Application.Features.WorldFeatures.UpdateWorld;

namespace WebApi.Presentation.Endpoints.WorldEndpoints;

internal static class UpdateWorldEndpoint
{
    public static RouteGroupBuilder MapUpdateWorldEndpoint(this RouteGroupBuilder group)
    {
        group.MapPut("/{id:long}", UpdateWorld)
            .WithName("UpdateWorld")
            .WithSummary("Updates the name and description of a world owned by the current user.");

        return group;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> UpdateWorld(
        [FromRoute] long id,
        [FromBody] UpdateWorldRequest request,
        ICommandManager<UpdateWorldCommand> handler,
        CancellationToken cancellationToken)
    {
        UpdateWorldCommand command = new(id, request.Name, request.Description);

        Result result = await handler.Handle(command, cancellationToken);

        return result.ToMinimalApiResult();
    }

    private sealed record UpdateWorldRequest(string Name, string? Description);
}
