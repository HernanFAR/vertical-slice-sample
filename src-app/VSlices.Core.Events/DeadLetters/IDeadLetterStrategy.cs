using VSlices.Domain.Interfaces;

namespace VSlices.Core.Events.DeadLetters;

/// <summary>
/// Defines a strategy to handle events that does not complete in time, due to errors in the system.
/// </summary>
public interface IDeadLetterStrategy
{
    /// <summary>
    /// Persists the <paramref name="event"/>
    /// </summary>
    ValueTask PersistAsync(IEvent @event, CancellationToken cancellationToken = default);
}
