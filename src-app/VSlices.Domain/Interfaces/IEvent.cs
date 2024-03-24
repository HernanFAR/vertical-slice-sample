using VSlices.Base;
using VSlices.Base.Responses;

namespace VSlices.Domain.Interfaces;

/// <summary>
/// Represents the start point of a side effect of domain rule
/// </summary>
public interface IEvent : IFeature<Success>
{
    /// <summary>
    /// The unique identifier of this event
    /// </summary>
    Guid Id { get; }
}
