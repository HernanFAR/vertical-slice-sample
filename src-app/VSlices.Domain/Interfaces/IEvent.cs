using LanguageExt;
using VSlices.Base.Core;

namespace VSlices.Domain.Interfaces;

/// <summary>
/// Represents the start point of a side effect of domain rule
/// </summary>
public interface IEvent : IFeature<Unit>
{
    /// <summary>
    /// The unique identifier of this event
    /// </summary>
    Guid EventId { get; }
}
