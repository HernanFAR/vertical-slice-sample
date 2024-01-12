using VSlices.Base;
using VSlices.Base.Responses;

namespace VSlices.Core.Events;

/// <summary>
/// Represents the start point of a side effect of a use case
/// </summary>
public interface IEvent : IFeature<Success>
{
    /// <summary>
    /// The unique identifier of this event
    /// </summary>
    Guid Id { get; }
}

/// <summary>
/// Abstract base class for all events
/// </summary>
public abstract record EventBase : IEvent
{
    /// <inheritdoc />
    public virtual Guid Id { get; } = Guid.NewGuid();
}
