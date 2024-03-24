namespace VSlices.Domain.Interfaces;

/// <summary>
/// Defines an aggregate root
/// </summary>
public interface IAggregateRoot<TKey> : IEntity<TKey>
    where TKey : struct, IEquatable<TKey>
{
}
