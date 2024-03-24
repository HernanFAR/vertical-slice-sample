namespace VSlices.Domain.Interfaces;

/// <summary>
/// Defines an entity with a defined identifier
/// </summary>
public interface IEntity<TKey> : IEquatable<IEntity<TKey>>
    where TKey : struct, IEquatable<TKey>
{
    /// <summary>
    /// The key of the entity
    /// </summary>
    TKey Id { get; }

}
