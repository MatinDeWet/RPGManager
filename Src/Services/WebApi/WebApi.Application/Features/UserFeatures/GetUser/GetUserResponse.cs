namespace WebApi.Application.Features.UserFeatures.GetUser;

public sealed record GetUserResponse(long Id, string IdentityId, DateTimeOffset DateCreated);
