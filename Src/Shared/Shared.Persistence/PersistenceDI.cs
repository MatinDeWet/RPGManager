using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Persistence.Constants;
using Shared.Persistence.Data.Contexts;

namespace Shared.Persistence;

public static class PersistenceDI
{
    /// <summary>
    /// Registers the shared persistence layer: the <see cref="CoreContext"/> wired to PostgreSQL.
    /// Both the WebApi and Worker services compose their data access on top of this.
    /// </summary>
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        services.AddDbContext<CoreContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());

            options.UseNpgsql(
                configuration.GetConnectionString("CoreDB"),
                opt =>
                {
                    opt.MigrationsAssembly(typeof(CoreContext).GetTypeInfo().Assembly.GetName().Name);
                    opt.MigrationsHistoryTable(HistoryRepository.DefaultTableName, SchemaConstants.Migrations);
                });

            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            if (isDevelopment)
            {
                options.EnableSensitiveDataLogging();
            }
        });

        return services;
    }
}
