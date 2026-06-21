using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using CQRS.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using WebApi.Application.Features.WorldFeatures.GetWorldById;

namespace WebApi.Presentation.Endpoints.WorldEndpoints;

internal static class GetWorldByIdEndpoint
{
    public static RouteGroupBuilder MapGetWorldByIdEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:long}", GetWorldById)
            .WithName("GetWorldById")
            .WithSummary("Returns a single world owned by the current user.");

        return group;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> GetWorldById(
        [FromRoute] long id,
        IQueryManager<GetWorldByIdQuery, GetWorldByIdResponse> handler,
        CancellationToken cancellationToken)
    {
        Result<GetWorldByIdResponse> result = await handler.Handle(new GetWorldByIdQuery(id), cancellationToken);

        return result.ToMinimalApiResult();
    }
}
