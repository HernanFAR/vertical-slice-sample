using VSlices.Domain.Interfaces;

namespace VSlices.Domain;

/// <summary>
/// Abstract base class for all events
/// </summary>
public abstract class Event : IEvent
{
    /// <inheritdoc />
    public Guid EventId { get; } = Guid.NewGuid();

}
