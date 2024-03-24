namespace VSlices.Domain.Interfaces.Audit;

/// <summary>
/// Defines an entity that has a creation date
/// </summary>
public interface IHasActivatedAt : IHasDeactivatedAt
{
    /// <summary>
    /// Gets the creation date of the entity
    /// </summary>
    DateTime? ActivatedAt { get; }

    /// <summary>
    /// Defines an entity that has a creation date and a user that created it
    /// </summary>
    /// <typeparam name="TUserIdKey"></typeparam>
    public interface WithActivatedBy<out TUserIdKey> : IHasActivatedAt, WithDeactivatedBy<TUserIdKey>
    {
        /// <summary>
        /// Gets the identifier of the user that created the entity
        /// </summary>
        TUserIdKey? ActivatedById { get; }

    }
}
