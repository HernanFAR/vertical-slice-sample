using VSlices.Domain.Interfaces.Audit;

namespace VSlices.Domain.Audited;

public abstract class CUserAuditedEntity<TCreatedById> : Entity, IHasCreatedAt.WithCreatedBy<TCreatedById>
{
    public DateTime CreatedAt { get; private set; }

    public TCreatedById CreatedById { get; private set; }

    protected CUserAuditedEntity(DateTime createdAt, TCreatedById createdById)
    {
        CreatedAt = createdAt;
        CreatedById = createdById;
    }

}

public abstract class CUserAuditedEntity<TKey, TCreatedById> : Entity<TKey>, IHasCreatedAt.WithCreatedBy<TCreatedById>
{
    public DateTime CreatedAt { get; private set; }

    public TCreatedById CreatedById { get; private set; }

    protected CUserAuditedEntity(DateTime createdAt, TCreatedById createdById)
    {
        CreatedAt = createdAt;
        CreatedById = createdById;
    }

}