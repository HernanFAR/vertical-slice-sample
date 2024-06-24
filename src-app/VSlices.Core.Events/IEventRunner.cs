using LanguageExt;
using LanguageExt.SysX.Live;
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
    /// <param name="runtime">Execution runtime</param> 
    /// <returns><see cref="Task"/> representing the action</returns>
    ValueTask<Fin<Unit>> PublishAsync(IEvent @event, Runtime runtime);

}
