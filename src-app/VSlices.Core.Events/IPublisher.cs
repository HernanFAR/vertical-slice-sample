namespace VSlices.Core.Events;

/// <summary>
/// Publishes an event to be handled by many handlers
/// </summary>
public interface IPublisher
{
    /// <summary>
    /// Asynchronously publishes an event to an event handler
    /// </summary>
    /// <param name="event">Event</param>
    /// <param name="cancellationToken">CancellationToken</param> 
    /// <returns><see cref="ValueTask"/> representing the action</returns>
    ValueTask PublishAsync(IEvent @event, CancellationToken cancellationToken);

}
