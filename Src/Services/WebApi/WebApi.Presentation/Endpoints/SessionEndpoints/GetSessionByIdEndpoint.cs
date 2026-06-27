using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using CQRS.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using WebApi.Application.Features.SessionFeatures.GetSessionById;
using WebApi.Presentation.Common.Extensions;

namespace WebApi.Presentation.Endpoints.SessionEndpoints;

internal static class GetSessionByIdEndpoint
{
    public static RouteGroupBuilder MapGetSessionByIdEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:long}", GetSessionById)
            .WithName("GetSessionById")
            .WithSummary("Returns a single session in a campaign the current user is a member of.")
            .ProducesResult<GetSessionByIdResponse>();

        return group;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> GetSessionById(
        [FromRoute] long id,
        [FromServices] IQueryManager<GetSessionByIdQuery, GetSessionByIdResponse> handler,
        CancellationToken cancellationToken)
    {
        Result<GetSessionByIdResponse> result = await handler.Handle(new GetSessionByIdQuery(id), cancellationToken);

        return result.ToMinimalApiResult();
    }
}
