using VSlices.Domain.Interfaces;

namespace VSlices.Domain;

/// <summary>
/// A static class that contains abstractions for classes that can't implement <see cref="Entity{TKey}"/>.
/// </summary>
public static class EntityExtensions
{
    /// <summary>
    /// An abstraction that returns a string representation of a <see cref="IEntity{TKey}"/> 
    /// instance, used in <seealso cref="Entity{TKey}.ToString"/> 
    /// </summary>
    /// <param name="this">Entity</param>
    /// <returns>String representation of the instance</returns>
    public static string EntityToString<TKey>(this IEntity<TKey> @this)
        where TKey : struct, IEquatable<TKey>
    {
        return $"[{@this.GetType().Name} | {string.Join(", ", @this.Id.ToString())}]";
    }

    public static bool EntityEquals<TKey>(this IEntity<TKey>? @this, IEntity<TKey>? other)
        where TKey : struct, IEquatable<TKey>
    {
        if (other is null || @this is null)
        {
            return false;
        }

        // Same instances must be considered as equal
        if (ReferenceEquals(@this, other))
        {
            return true;
        }

        // Must have a IS-A relation of types or must be same type
        var typeOfEntity1 = @this.GetType();
        var typeOfEntity2 = other.GetType();

        if (!typeOfEntity1.IsAssignableFrom(typeOfEntity2) && !typeOfEntity2.IsAssignableFrom(typeOfEntity1))
        {
            return false;
        }

        return @this.Id.Equals(other.Id);
    }

}
