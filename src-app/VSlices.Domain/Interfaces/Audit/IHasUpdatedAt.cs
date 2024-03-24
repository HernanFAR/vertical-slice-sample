namespace VSlices.Domain.Interfaces.Audit;

/// <summary>
/// Defines an entity that has an update date
/// </summary>
public interface IHasUpdatedAt
{
    /// <summary>
    /// Gets the update date of the entity
    /// </summary>
    DateTime? UpdatedAt { get; }

    /// <summary>
    /// Defines an entity that has an update date and a user that updated it
    /// </summary>
    /// <typeparam name="TUserIdKey"></typeparam>
    public interface WithUpdatedBy<out TUserIdKey> : IHasUpdatedAt
    {
        /// <summary>
        /// Gets the identifier of the user that updated the entity
        /// </summary>
        TUserIdKey? UpdatedById { get; }

    }
}
