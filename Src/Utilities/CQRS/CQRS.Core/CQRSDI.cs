using CQRS.Core.Contracts;
using CQRS.Core.Implementation;
using CQRS.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CQRS.Core;

public static class CQRSDI
{
    public static IServiceCollection AddCQRSSupport(this IServiceCollection services, Type assemblyPointer)
    {
        return services.AddCQRSSupport(assemblyPointer, null);
    }

    public static IServiceCollection AddCQRSSupport(this IServiceCollection services, Type assemblyPointer, Action<IServiceCollection>? configureDecorators)
    {
        services.Scan(scan => scan.FromAssembliesOf(assemblyPointer)
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryManager<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandManager<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandManager<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        // Apply additional decorators if provided
        configureDecorators?.Invoke(services);

        services.Scan(scan => scan.FromAssembliesOf(assemblyPointer)
            .AddClasses(classes => classes.AssignableTo(typeof(IDomainEventManager<>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.AddSingleton<IBackgroundDomainEventQueue, BackgroundDomainEventQueue>();
        services.AddHostedService<BackgroundDomainEventProcessor>();
        services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();

        return services;
    }

    /// <summary>
    /// Safely decorates a service type only if implementations are registered for that type.
    /// This prevents DI container errors when no implementations exist.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="serviceType">The service type to decorate (generic type definition)</param>
    /// <param name="decoratorType">The decorator type (generic type definition)</param>
    public static void TryDecorateIfImplementationsExist(this IServiceCollection services, Type serviceType, Type decoratorType)
    {
        // Check if there are any registered implementations for the service type
        bool hasImplementations = services.Any(descriptor =>
            descriptor.ServiceType.IsGenericType &&
            descriptor.ServiceType.GetGenericTypeDefinition() == serviceType);

        if (hasImplementations)
        {
            services.Decorate(serviceType, decoratorType);
        }
    }
}
