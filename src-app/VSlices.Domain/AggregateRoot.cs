using VSlices.Domain.Interfaces;

namespace VSlices.Domain;

/// <inheritdoc cref="IEntity{TKey}" />
public abstract class AggregateRoot<TKey> : Entity<TKey>, IAggregateRoot<TKey>
    where TKey : class, IEquatable<TKey>
{
    /// <summary>
    /// Empty constructor to use in serialization scenarios
    /// </summary>
    /// <remarks>Do not use this constructor in your code, if is not for serialization</remarks>
    protected AggregateRoot() {}

    /// <summary>
    /// Creates a new entity with the specified key
    /// </summary>
    /// <param name="id">The key of the entity</param>
    protected AggregateRoot(TKey id) : base(id) { }

}
