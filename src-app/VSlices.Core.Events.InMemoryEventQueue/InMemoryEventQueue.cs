using System.Threading.Channels;
using VSlices.Domain.Interfaces;

namespace VSlices.Core.Events;

/// <summary>
/// Allows publishing, peek and dequeue events through an in memory channel.
/// </summary>
public sealed class InMemoryEventQueue : IEventQueue
{
    internal readonly Channel<IEvent> _channel;

    /// <summary>
    /// Creates a new instance of <see cref="InMemoryEventQueue"/>
    /// </summary>
    /// <param name="configuration">Configuration</param>
    public InMemoryEventQueue(InMemoryEventQueueConfiguration configuration)
    {
        var options = new BoundedChannelOptions(configuration.Capacity)
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
        => await _channel.Writer.WriteAsync(@event, cancellationToken);
    
}
