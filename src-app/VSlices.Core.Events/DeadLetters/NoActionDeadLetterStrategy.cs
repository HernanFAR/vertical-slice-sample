using VSlices.Domain.Interfaces;

namespace VSlices.Core.Events.DeadLetters;

/// <summary>
/// A dead letter strategy that does not do anything
/// </summary>
public sealed class NoActionDeadLetterStrategy : IDeadLetterStrategy
{
    /// <inheritdoc />
    public ValueTask PersistAsync(IEvent @event, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
}
