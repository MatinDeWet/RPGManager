namespace WebApi.Presentation.Common.CurrentUser;

internal static class UserCacheKeys
{
    public static string ByExternalId(string externalId) => $"user:external:{externalId}";
}
