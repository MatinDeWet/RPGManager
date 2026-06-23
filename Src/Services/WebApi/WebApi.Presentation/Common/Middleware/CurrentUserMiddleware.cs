using System.Globalization;
using System.Security.Claims;
using Ardalis.Result;
using Caching.Contracts;
using CQRS.Core.Contracts;
using Identification.Constants;
using Identification.Contracts;
using WebApi.Application.Features.UserFeatures.UpsertUser;
using WebApi.Presentation.Common.CurrentUser;

namespace WebApi.Presentation.Common.Middleware;

internal sealed class CurrentUserMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        IInfoSetter infoSetter,
        ICacheService cache,
        ICommandManager<UpsertUserCommand, UpsertUserResponse> upsertUser)
    {
        string? externalId = context.User.Identity?.IsAuthenticated == true
            ? context.User.FindFirstValue(ClaimConstants.ExternalUserId)
            : null;

        if (!string.IsNullOrWhiteSpace(externalId))
        {
            string email = context.User.FindFirstValue(ClaimConstants.Email) ?? string.Empty;

            UserCacheModel user = await cache.GetOrCreateAsync(
                UserCacheKeys.ByExternalId(externalId),
                async ct =>
                {
                    Result<UpsertUserResponse> result = await upsertUser.Handle(new UpsertUserCommand(externalId, email), ct);

                    if (!result.IsSuccess)
                    {
                        throw new InvalidOperationException("Unable to resolve the current user.");
                    }

                    return new UserCacheModel(result.Value.Id, result.Value.IdentityId);
                },
                context.RequestAborted);

            List<Claim> claims =
            [
                .. context.User.Claims,
                new Claim(ClaimConstants.InternalUserId, user.Id.ToString(CultureInfo.InvariantCulture)),
            ];

            infoSetter.SetUser(claims);
        }

        await next(context);
    }
}
