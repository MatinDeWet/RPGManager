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

/// <summary>
/// Resolves the authenticated caller to a persisted user on every request: reads the external id
/// from the token, resolves the user from the cache (only dispatching the upsert command — which
/// auto-provisions on first login — on a cache miss), then populates the request identity once with
/// the token claims plus the resolved internal user id. The result is exposed as a
/// <see cref="UserCacheModel"/>. Anonymous requests pass straight through.
/// </summary>
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
            UserCacheModel user = await cache.GetOrCreateAsync(
                UserCacheKeys.ByExternalId(externalId),
                async ct =>
                {
                    Result<UpsertUserResponse> result = await upsertUser.Handle(new UpsertUserCommand(externalId), ct);

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
