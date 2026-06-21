using System.Threading.Channels;
using CQRS.Core.Contracts;
using CQRS.Domain.Contracts;

namespace CQRS.Core.Services;

internal sealed class BackgroundDomainEventQueue : IBackgroundDomainEventQueue
{
    private readonly Channel<IDomainEvent> _channel;

    public BackgroundDomainEventQueue(int capacity = 1000)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _channel = Channel.CreateBounded<IDomainEvent>(options);
    }

    public async ValueTask EnqueueAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent, nameof(domainEvent));
        await _channel.Writer.WriteAsync(domainEvent, cancellationToken);
    }

    public IAsyncEnumerable<IDomainEvent> DequeueAsync(CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
