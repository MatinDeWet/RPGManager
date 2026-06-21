using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using WebApi.Presentation.Common.Options;

namespace WebApi.Presentation.Common.OpenApi;

/// <summary>
/// Adds an OAuth2 (Authorization Code) security scheme to the generated OpenAPI document and
/// applies it to every operation, so the Swagger UI shows an "Authorize" button wired to the
/// configured identity provider.
/// </summary>
internal sealed class OAuthSecuritySchemeTransformer(IOptions<AuthenticationSettings> options) : IOpenApiDocumentTransformer
{
    public const string SchemeName = "OAuth2";

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        SwaggerAuthenticationSettings swagger = options.Value.Swagger;

        var scheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = swagger.AuthorizationUrl,
                    TokenUrl = swagger.TokenUrl,
                    Scopes = new Dictionary<string, string>(swagger.Scopes, StringComparer.Ordinal),
                },
            },
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>(StringComparer.Ordinal);
        document.Components.SecuritySchemes[SchemeName] = scheme;

        var requirement = new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(SchemeName, document)] = [],
        };

        foreach (IOpenApiPathItem path in document.Paths.Values)
        {
            if (path.Operations is null)
            {
                continue;
            }

            foreach (OpenApiOperation operation in path.Operations.Values)
            {
                (operation.Security ??= []).Add(requirement);
            }
        }

        return Task.CompletedTask;
    }
}
