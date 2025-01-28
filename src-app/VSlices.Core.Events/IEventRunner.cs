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
    Fin<Unit> Publish(IEvent @event, CancellationToken cancellationToken = default);

}
