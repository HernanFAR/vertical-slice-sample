using VSlices.Domain.Interfaces.Audit;

namespace VSlices.Domain.Audited;

public abstract class CUUserAuditedEntity<TCreatedById> : CUserAuditedEntity<TCreatedById>, IHasUpdatedAt.WithUpdatedBy<TCreatedById>
{
    public DateTime? UpdatedAt { get; private set; }

    public TCreatedById? UpdatedById { get; private set; }

    protected CUUserAuditedEntity(DateTime createdAt, TCreatedById createdById) 
        : base(createdAt, createdById) { }

    protected void Update(DateTime updatedAt, TCreatedById updatedById)
    {
        UpdatedAt = updatedAt;
        UpdatedById = updatedById;
    }
}

public abstract class CUUserAuditedEntity<TKey, TCreatedById> : CUserAuditedEntity<TKey, TCreatedById>, IHasUpdatedAt.WithUpdatedBy<TCreatedById>
{
    public DateTime? UpdatedAt { get; private set; }

    public TCreatedById? UpdatedById { get; private set; }

    protected CUUserAuditedEntity(DateTime createdAt, TCreatedById createdById)
        : base(createdAt, createdById) { }

    protected void Update(DateTime updatedAt, TCreatedById updatedById)
    {
        UpdatedAt = updatedAt;
        UpdatedById = updatedById;
    }
}