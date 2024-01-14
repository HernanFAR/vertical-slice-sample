namespace VSlices.Domain;

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
    /// Performs a identity check between two entities
    /// </summary>
    /// <param name="other">The other entity to </param>
    /// <returns>true if is equal, false if not</returns>
    bool EntityEquals(IEntity? other);
}

/// <summary>
/// Base entity with non specified keys
/// </summary>
/// <remarks>Provides a better <see cref="ToString()"/> implementation, aside of a <see cref="GetKeys()"/> and an <see cref="EntityEquals"/> method</remarks>
public abstract class Entity : IEntity
{
    /// <inheritdoc/>
    public override string ToString() => this.EntityToString();

    /// <inheritdoc/>
    public abstract object[] GetKeys();

    /// <inheritdoc/>
    public bool EntityEquals(IEntity? other) => this.EntityEqualsTo(other);
}

/// <summary>
/// Base entity with a strong typed key
/// </summary>
/// <remarks>Provides a better <see cref="Entity.ToString()"/> implementation, aside of a <see cref="GetKeys()"/> and an <see cref="Entity.EntityEquals"/> method</remarks>
public abstract class Entity<TKey> : Entity
{
    /// <summary>
    /// The key of the entity
    /// </summary>
    public TKey Id { get; private set; }

    /// <summary>
    /// Empty constructor to use in serialization scenarios
    /// </summary>
    /// <remarks>Do not use this constructor in your code, if is not for serialization</remarks>
    protected Entity()
    {
        Id = default!;
    }

    /// <summary>
    /// Creates a new entity with the specified key
    /// </summary>
    /// <param name="id">The key of the entity</param>
    protected Entity(TKey id)
    {
        Id = id;
    }

    /// <inheritdoc/>
    public override object[] GetKeys() => new object[] { Id! };

}