using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repository;
using Shared.Persistence;
using Shared.Persistence.Constants;

namespace Worker.infrastructure;

public static class WorkerInfrastructureDI
{
    /// <summary>
    /// Registers the worker infrastructure: the shared persistence context, the worker's unsecured
    /// repositories, and Hangfire (PostgreSQL storage in its own dedicated schema + the job server).
    /// </summary>
    public static IServiceCollection AddWorkerInfrastructure(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        services.AddPersistence(configuration, isDevelopment);

        // Scan this assembly for the worker's unsecured repo implementations.
        services.AddRepositories(typeof(WorkerInfrastructureDI));

        services.AddHangfire(cfg => cfg
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(
                c => c.UseNpgsqlConnection(configuration.GetConnectionString("CoreDB")),
                new PostgreSqlStorageOptions { SchemaName = SchemaConstants.Hangfire }));

        services.AddHangfireServer();

        return services;
    }
}
