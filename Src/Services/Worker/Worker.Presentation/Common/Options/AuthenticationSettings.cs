namespace Worker.Presentation.Common.Options;

/// <summary>
/// Binds the <c>Authentication</c> configuration section for the worker's Hangfire dashboard.
/// The dashboard is a browser UI, so it uses interactive OIDC login (Authorization Code) against
/// the configured Keycloak client rather than bearer tokens.
/// </summary>
public sealed class AuthenticationSettings
{
    public const string SectionName = "Authentication";

    /// <summary>OIDC authority (Keycloak realm URL) used to discover endpoints and validate tokens.</summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>Confidential client id registered in Keycloak for the dashboard.</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>Client secret for the confidential dashboard client.</summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>Set to <c>false</c> for local HTTP providers; keep <c>true</c> everywhere else.</summary>
    public bool RequireHttpsMetadata { get; set; } = true;
}
