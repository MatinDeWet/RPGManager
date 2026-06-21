namespace WebApi.Presentation.Common.CurrentUser;

/// <summary>
/// Cached representation of the authenticated caller after they have been resolved against the
/// database. Stored per external identity by <c>CurrentUserMiddleware</c>.
/// </summary>
public sealed record UserCacheModel(long Id, string IdentityId);
