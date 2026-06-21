using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repository;
using Shared.Persistence;

namespace WebApi.infrastructure;

public static class InfrastructureDI
{
    /// <summary>
    /// Registers the infrastructure layer: the shared persistence context and the secured/unsecured
    /// repositories (and their entity protections) discovered in this assembly.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, bool IsDevelopment)
    {
        services.AddPersistence(configuration, IsDevelopment);

        // Scan this assembly (where the repo facades + locks live) — not the persistence assembly.
        services.AddRepositories(typeof(InfrastructureDI));
        services.AddSecuredRepositories(typeof(InfrastructureDI));

        return services;
    }
}
