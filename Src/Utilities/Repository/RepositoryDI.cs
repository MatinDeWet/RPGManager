using Microsoft.Extensions.DependencyInjection;
using Repository.Contracts;

namespace Repository;

/// <summary>
/// Provides dependency injection configuration for repository services.
/// </summary>
public static class RepositoryDI
{
    /// <summary>
    /// Registers basic repository services (queries and commands) without security constraints.
    /// Scans the specified assembly for implementations of <see cref="IQueryRepo"/> and <see cref="ICommandRepo"/>.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="assemblyPointer">A type from the assembly to scan for repository implementations.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddRepositories(this IServiceCollection services, Type assemblyPointer)
    {
        services.Scan(scan => scan
            .FromAssemblies(assemblyPointer.Assembly)
            .AddClasses(classes => classes.AssignableToAny(typeof(IQueryRepo), typeof(ICommandRepo)), publicOnly: false)
            .AsSelfWithInterfaces()
            .WithScopedLifetime());

        return services;
    }

    /// <summary>
    /// Registers secured repository services along with the entity protection implementations
    /// (<see cref="IProtected"/>) discovered in the same assembly.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="assemblyPointer">A type from the assembly to scan for repository implementations.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddSecuredRepositories(this IServiceCollection services, Type assemblyPointer)
    {
        services.Scan(scan => scan
            .FromAssemblies(assemblyPointer.Assembly)
            .AddClasses(classes => classes.AssignableToAny(typeof(ISecureQueryRepo), typeof(ISecureCommandRepo)), publicOnly: false)
            .AsSelfWithInterfaces()
            .WithScopedLifetime());

        services.Scan(scan => scan
            .FromAssemblies(assemblyPointer.Assembly)
            .AddClasses(classes => classes.AssignableToAny(typeof(IProtected)), publicOnly: false)
            .AsSelfWithInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
