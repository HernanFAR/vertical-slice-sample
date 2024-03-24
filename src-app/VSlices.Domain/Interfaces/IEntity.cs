namespace VSlices.Domain.Interfaces;

/// <summary>
/// Defines an entity with non specified keys
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Gets the keys of the entity
    /// </summary>
    /// <returns>An array with the keys of the entity</returns>
    object[] GetKeys();

    /// <summary>
    /// Performs an identity check between two entities
    /// </summary>
    /// <param name="other">The other entity to </param>
    /// <returns>true if is equal, false if not</returns>
    bool EntityEquals(IEntity? other);
}

public interface IEntity<out TKey> : IEntity
{
    /// <summary>
    /// The key of the entity
    /// </summary>
    TKey Id { get; }
}
