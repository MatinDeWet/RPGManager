using System.Text.Json.Serialization;
using BlobStorage;
using Caching;
using WebApi.Application;
using WebApi.infrastructure;
using WebApi.Presentation.Common.ExceptionHandling;

namespace WebApi.Presentation.Common.DIExtensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        bool isDevelopmentOrStaging = environment.IsDevelopment() || environment.IsStaging();

        services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddProblemDetails(options => options.CustomizeProblemDetails = ProblemDetailsEnricher.Enrich);
        services.AddExceptionHandler<UnauthorizedAccessExceptionHandler>(); // 403, specific
        services.AddExceptionHandler<ValidationExceptionHandler>();          // 400, specific
        services.AddExceptionHandler<GlobalExceptionHandler>();              // 500, catch-all (last)

        services.AddApiDocumentation();
        services.AddJwtAuthentication(configuration);

        services.AddApplication();
        services.AddInfrastructure(configuration, isDevelopmentOrStaging);
        services.AddCachingSupport(configuration);
        services.AddBlobStorage(configuration);

        return services;
    }
}
