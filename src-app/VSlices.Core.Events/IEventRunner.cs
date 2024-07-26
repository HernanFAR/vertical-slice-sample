using LanguageExt;
using VSlices.Domain.Interfaces;

namespace VSlices.Core.Events;

/// <summary>
/// Publishes an event to handle by many handlers
/// </summary>
public interface IEventRunner
{
    /// <summary>
    /// Asynchronously publishes an event to an event handler
    /// </summary>
    /// <param name="event">Event</param>
    /// <param name="runtime">Execution runtime</param> 
    /// <returns><see cref="Task"/> representing the action</returns>
    Fin<Unit> Publish(IEvent @event, HandlerRuntime runtime);

}
