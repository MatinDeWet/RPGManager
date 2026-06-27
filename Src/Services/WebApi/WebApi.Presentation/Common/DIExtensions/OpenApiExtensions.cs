using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;
using WebApi.Presentation.Common.OpenApi;
using WebApi.Presentation.Common.Options;

namespace WebApi.Presentation.Common.DIExtensions;

public static class OpenApiExtensions
{
    /// <summary>Registers the built-in OpenAPI document generator with the OAuth2 security scheme.</summary>
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<OpenApiInfoTransformer>();
            options.AddDocumentTransformer<OAuthSecuritySchemeTransformer>();
        });

        return services;
    }

    /// <summary>
    /// Exposes the OpenAPI document and the Swagger UI, pre-wiring the UI's OAuth2 (PKCE) login
    /// against the configured identity provider. The document endpoint is anonymous so the
    /// secure-by-default policy does not lock the UI out.
    /// </summary>
    public static WebApplication UseApiDocumentation(this WebApplication app)
    {
        app.MapOpenApi().AllowAnonymous();

        SwaggerAuthenticationSettings swagger = app.Services
            .GetRequiredService<IOptions<AuthenticationSettings>>().Value.Swagger;

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "RPGManager API v1");
            options.OAuthClientId(swagger.ClientId);
            options.OAuthUsePkce();
            options.OAuthScopeSeparator(" ");
            options.OAuthScopes([.. swagger.Scopes.Keys]);
        });

        return app;
    }
}
