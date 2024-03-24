using VSlices.Domain.Interfaces;

namespace VSlices.Domain;

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
public abstract class Entity<TKey> : Entity, IEntity<TKey>
{
    /// <inheritdoc />
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