using CQRS.Core;
using Identification;
using Microsoft.Extensions.DependencyInjection;

namespace WebApi.Application;

public static class ApplicationDI
{
    /// <summary>
    /// Registers the application layer: CQRS handlers (queries/commands/domain events) discovered in
    /// this assembly and request identity resolution.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddCQRSSupport(typeof(ApplicationDI));
        services.AddIdentificationSupport();

        return services;
    }
}
