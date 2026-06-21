namespace WebApi.Presentation.Common.Options;

/// <summary>
/// Binds the <c>Authentication</c> configuration section. Provider-agnostic: it describes an
/// OIDC/OAuth2 identity provider by its authority and endpoints, so switching providers is a
/// configuration change rather than a code change.
/// </summary>
public sealed class AuthenticationSettings
{
    public const string SectionName = "Authentication";

    /// <summary>OIDC authority used to discover signing keys and validate tokens (the <c>.well-known</c> metadata is fetched from here).</summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>Expected <c>aud</c> claim. The provider must be configured to emit it (e.g. via an audience mapper).</summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>Set to <c>false</c> for local HTTP providers; keep <c>true</c> everywhere else.</summary>
    public bool RequireHttpsMetadata { get; set; } = true;

    /// <summary>Disable only when the provider is not configured to emit an audience claim.</summary>
    public bool ValidateAudience { get; set; } = true;

    public SwaggerAuthenticationSettings Swagger { get; set; } = new();
}

/// <summary>Public-client settings used by the Swagger UI to perform an Authorization Code + PKCE login.</summary>
public sealed class SwaggerAuthenticationSettings
{
    public string ClientId { get; set; } = string.Empty;

    /// <summary>Provider authorization endpoint (e.g. <c>.../authorize</c> or <c>.../protocol/openid-connect/auth</c>).</summary>
    public Uri? AuthorizationUrl { get; set; }

    /// <summary>Provider token endpoint.</summary>
    public Uri? TokenUrl { get; set; }

    /// <summary>Scopes offered on the Swagger "Authorize" dialog, keyed by scope name with a display description.</summary>
    public IDictionary<string, string> Scopes { get; } = new Dictionary<string, string>(StringComparer.Ordinal);
}
