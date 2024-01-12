using System.Threading.Channels;

namespace VSlices.Core.Events;

/// <summary>
/// Allows to publish, peek and dequeue events through an in memory channel
/// </summary>
public sealed class InMemoryEventQueue : IEventQueue
{
    readonly Channel<IEvent> _channel;

    /// <summary>
    /// Creates a new instance of <see cref="InMemoryEventQueue"/>
    /// </summary>
    /// <param name="inMemoryEventQueueConfiguration">Configuration</param>
    public InMemoryEventQueue(InMemoryEventQueueConfiguration inMemoryEventQueueConfiguration)
    {
        var options = new BoundedChannelOptions(inMemoryEventQueueConfiguration.Capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        
        _channel = Channel.CreateBounded<IEvent>(options);
    }

    /// <inheritdoc />
    public async ValueTask<IEvent> DequeueAsync(CancellationToken cancellationToken) 
        => await _channel.Reader.ReadAsync(cancellationToken);

    /// <inheritdoc />
    public async ValueTask EnqueueAsync(IEvent @event, CancellationToken cancellationToken)
    {
        await _channel.Writer.WriteAsync(@event, cancellationToken);
    }
}
