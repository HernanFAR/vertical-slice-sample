namespace VSlices.Core.Events;

/// <summary>
/// Represents a queue writer of events
/// </summary>
public interface IEventQueueWriter
{
    /// <summary>
    /// Asynchronously enqueue a event to the queue
    /// </summary>
    /// <param name="event">Event</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous execution</returns>
    ValueTask EnqueueAsync(IEvent @event, CancellationToken cancellationToken = default);

}

/// <summary>
/// Represents a queue reader of events
/// </summary>
public interface IEventQueueReader
{
    /// <summary>
    /// Asynchronously dequeue the next event from the queue
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous execution which returns a <see cref="IEvent"/></returns>
    ValueTask<IEvent> DequeueAsync(CancellationToken cancellationToken = default);

}

/// <summary>
/// Represents a queue of events, with write and read capabilities
/// </summary>
public interface IEventQueue : IEventQueueReader, IEventQueueWriter
{
}
