using System.Reflection;
using CQRS.Domain.Contracts;
using CQRS.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CQRS.Core.Services;

internal sealed class BackgroundDomainEventProcessor(
    IBackgroundDomainEventQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<BackgroundDomainEventProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Background Domain Event Processor started");

        await foreach (IDomainEvent domainEvent in queue.DequeueAsync(stoppingToken))
        {
            try
            {
                await ProcessDomainEventAsync(domainEvent, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing domain event {EventType}", domainEvent.GetType().Name);
            }
        }

        logger.LogInformation("Background Domain Event Processor stopped");
    }

    private async Task ProcessDomainEventAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        using IServiceScope scope = scopeFactory.CreateScope();

        Type domainEventType = domainEvent.GetType();
        Type handlerType = typeof(IDomainEventManager<>).MakeGenericType(domainEventType);

        IEnumerable<object?> handlers = scope.ServiceProvider.GetServices(handlerType);

        foreach (object? handler in handlers)
        {
            if (handler is null)
            {
                continue;
            }

            try
            {
                await InvokeHandlerAsync(handlerType, handler, domainEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Handler {HandlerType} failed for event {EventType}",
                    handler.GetType().Name, domainEventType.Name);
            }
        }
    }

    // Invoke through the public IDomainEventManager<TEvent>.Handle method so that internal
    // handler implementations (the scan registers them with publicOnly: false) remain reachable.
    private static async Task InvokeHandlerAsync(
        Type handlerType,
        object handler,
        IDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        MethodInfo handle = handlerType.GetMethod(nameof(IDomainEventManager<IDomainEvent>.Handle))!;
        await (Task)handle.Invoke(handler, [domainEvent, cancellationToken])!;
    }
}
