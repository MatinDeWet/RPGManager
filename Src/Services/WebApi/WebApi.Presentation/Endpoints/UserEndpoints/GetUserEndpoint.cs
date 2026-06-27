using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using CQRS.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using WebApi.Application.Features.UserFeatures.GetUser;
using WebApi.Presentation.Common.Extensions;

namespace WebApi.Presentation.Endpoints.UserEndpoints;

internal static class GetUserEndpoint
{
    public static RouteGroupBuilder MapGetUserEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("/me", GetCurrentUser)
            .WithName("GetCurrentUser")
            .WithSummary("Returns the currently authenticated user.")
            .ProducesResult<GetUserResponse>();

        return group;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> GetCurrentUser(
        [FromServices] IQueryManager<GetUserQuery, GetUserResponse> handler,
        CancellationToken cancellationToken)
    {
        Result<GetUserResponse> result = await handler.Handle(new GetUserQuery(), cancellationToken);

        return result.ToMinimalApiResult();
    }
}
