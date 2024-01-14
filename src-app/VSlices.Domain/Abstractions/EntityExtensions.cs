// ReSharper disable once CheckNamespace
namespace VSlices.Domain;

/// <summary>
/// A static class that contains abstractions for classes that can't implement <see cref="Entity"/>.
/// </summary>
public static class EntityExtensions
{
    /// <summary>
    /// An abstraction that returns a string representation of a <see cref="IEntity"/> instance, used in <see cref="Entity.ToString()"/>"/>
    /// </summary>
    /// <param name="this">Entity</param>
    /// <returns>String representation of the instance</returns>
    public static string EntityToString(this IEntity @this) => $"[{@this.GetType().Name} | {string.Join(", ", @this.GetKeys())}]";

    /// <summary>
    /// An abstraction that performs a key-value comparision between two <see cref="IEntity"/> instances, used in <see cref="Entity.EntityEquals"/>"/>
    /// </summary>
    /// <param name="this">Entity</param>
    /// <param name="other">Entity</param>
    /// <returns>true if the entities are the sames, false if not</returns>
    public static bool EntityEqualsTo(this IEntity? @this, IEntity? other)
    {
        if (other is null || @this is null)
        {
            return false;
        }

        //Same instances must be considered as equal
        if (ReferenceEquals(@this, other))
        {
            return true;
        }

        //Must have a IS-A relation of types or must be same type
        var typeOfEntity1 = @this.GetType();
        var typeOfEntity2 = other.GetType();

        if (!typeOfEntity1.IsAssignableFrom(typeOfEntity2) && !typeOfEntity2.IsAssignableFrom(typeOfEntity1))
        {
            return false;
        }

        var entity1Keys = @this.GetKeys();
        var entity2Keys = other.GetKeys();

        for (var i = 0; i < entity1Keys.Length; i++)
        {
            var entity1Key = entity1Keys[i];
            var entity2Key = entity2Keys[i];

            if (!entity1Key.Equals(entity2Key))
            {
                return false;
            }
        }

        return true;
    }
}
