using VSlices.Domain.Interfaces.Audit;

namespace VSlices.Domain.Audited;

public abstract class CAuditedEntity : Entity, IHasCreatedAt
{
    public DateTime CreatedAt { get; private set; }

    protected CAuditedEntity(DateTime createdAt)
    {
        CreatedAt = createdAt;
    }

}

public abstract class CAuditedEntity<TKey> : Entity<TKey>, IHasCreatedAt
{
    public DateTime CreatedAt { get; private set; }

    protected CAuditedEntity(DateTime createdAt)
    {
        CreatedAt = createdAt;
    }

}
