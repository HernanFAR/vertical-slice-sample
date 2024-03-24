namespace VSlices.Domain.Interfaces.Audit;

/// <summary>
/// Defines an entity that has a creation date
/// </summary>
public interface IHasDeactivatedAt
{
    /// <summary>
    /// Gets the creation date of the entity
    /// </summary>
    DateTime? DeactivatedAt { get; }

    /// <summary>
    /// Gets a value indicating whether the entity is deactivated
    /// </summary>
    bool IsDeactivated { get; }

    /// <summary>
    /// Defines an entity that has a creation date and a user that created it
    /// </summary>
    /// <typeparam name="TUserIdKey"></typeparam>
    public interface WithDeactivatedBy<out TUserIdKey> : IHasDeactivatedAt
    {
        /// <summary>
        /// Gets the identifier of the user that created the entity
        /// </summary>
        TUserIdKey? DeactivatedById { get; }

    }
}
