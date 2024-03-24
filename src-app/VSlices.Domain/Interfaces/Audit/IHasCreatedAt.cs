namespace VSlices.Domain.Interfaces.Audit;

/// <summary>
/// Defines an entity that has a creation date
/// </summary>
public interface IHasCreatedAt
{
    /// <summary>
    /// Gets the creation date of the entity
    /// </summary>
    DateTime CreatedAt { get; }

    /// <summary>
    /// Defines an entity that has a creation date and a user that created it
    /// </summary>
    /// <typeparam name="TUserIdKey"></typeparam>
    public interface WithCreatedBy<out TUserIdKey> : IHasCreatedAt
    {
        /// <summary>
        /// Gets the identifier of the user that created the entity
        /// </summary>
        TUserIdKey CreatedById { get; }

    }
}
