using VSlices.Domain.Interfaces.Audit;

namespace VSlices.Domain.Audited;

public abstract class CUDUserAuditedEntity<TCreatedById> : CUUserAuditedEntity<TCreatedById>, IHasDeactivatedAt.WithDeactivatedBy<TCreatedById>
{
    public DateTime? DeactivatedAt { get; private set; }

    public TCreatedById? DeactivatedById { get; private set; }

    public bool IsDeactivated { get; private set; }

    protected CUDUserAuditedEntity(DateTime createdAt, TCreatedById createdById) 
        : base(createdAt, createdById) { }

    protected void Deactivate(DateTime deactivatedAt, TCreatedById? deactivatedById)
    {
        DeactivatedAt = deactivatedAt;
        DeactivatedById = deactivatedById;
        IsDeactivated = true;
    }

}

public abstract class CUDUserAuditedEntity<TKey, TCreatedById> : CUUserAuditedEntity<TKey, TCreatedById>, IHasDeactivatedAt.WithDeactivatedBy<TCreatedById>
{
    public DateTime? DeactivatedAt { get; private set; }

    public TCreatedById? DeactivatedById { get; private set; }

    public bool IsDeactivated { get; private set; }

    protected CUDUserAuditedEntity(DateTime createdAt, TCreatedById createdById)
        : base(createdAt, createdById) { }

    protected void Deactivate(DateTime deactivatedAt, TCreatedById? deactivatedById)
    {
        DeactivatedAt = deactivatedAt;
        DeactivatedById = deactivatedById;
        IsDeactivated = true;
    }

}