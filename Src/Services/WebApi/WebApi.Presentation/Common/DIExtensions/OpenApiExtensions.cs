using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;
using WebApi.Presentation.Common;
using WebApi.Presentation.Common.OpenApi;
using WebApi.Presentation.Common.Options;

namespace WebApi.Presentation.Common.DIExtensions;

public static class OpenApiExtensions
{
    /// <summary>Registers one built-in OpenAPI document per API version, each with the OAuth2 security scheme.</summary>
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        foreach (string documentName in ApiVersions.AllGroupNames)
        {
            services.AddOpenApi(documentName, options =>
            {
                options.AddDocumentTransformer<OpenApiInfoTransformer>();
                options.AddDocumentTransformer<OAuthSecuritySchemeTransformer>();
            });
        }

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

        IApiVersionDescriptionProvider provider = app.Services
            .GetRequiredService<IApiVersionDescriptionProvider>();

        app.UseSwaggerUI(options =>
        {
            foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint(
                    $"/openapi/{description.GroupName}.json",
                    $"RPGManager API {description.GroupName.ToUpperInvariant()}");
            }

            options.OAuthClientId(swagger.ClientId);
            options.OAuthUsePkce();
            options.OAuthScopeSeparator(" ");
            options.OAuthScopes([.. swagger.Scopes.Keys]);
        });

        return app;
    }
}
