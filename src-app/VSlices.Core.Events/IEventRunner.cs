using LanguageExt;
using VSlices.Domain.Interfaces;

namespace VSlices.Core.Events;

/// <summary>
/// Publishes an event to be handled by many handlers
/// </summary>
public interface IEventRunner
{
    /// <summary>
    /// Asynchronously publishes an event to an event handler
    /// </summary>
    /// <param name="event">Event</param>
    /// <param name="cancellationToken">CancellationToken</param> 
    /// <returns><see cref="Task"/> representing the action</returns>
    ValueTask<Fin<Unit>> PublishAsync(IEvent @event, CancellationToken cancellationToken);

}
