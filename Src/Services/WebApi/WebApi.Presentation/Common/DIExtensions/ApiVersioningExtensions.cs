using Asp.Versioning;
using WebApi.Presentation.Common;

namespace WebApi.Presentation.Common.DIExtensions;

public static class ApiVersioningExtensions
{
    /// <summary>
    /// Registers URL-segment API versioning (e.g. <c>/v1/worlds</c>) and the ApiExplorer metadata that
    /// lets the OpenAPI generator split endpoints into one document per version. Runtime routing
    /// behaviour, so this applies in every environment (unlike the Development-only Swagger UI).
    /// </summary>
    public static IServiceCollection AddApiVersioningSupport(this IServiceCollection services)
    {
        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = ApiVersions.All[0];
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = ApiVersions.GroupNameFormat; // 1.0 -> "v1"
                options.SubstituteApiVersionInUrl = true;              // paths show /v1/... not /v{version:apiVersion}/...
            });

        return services;
    }
}
