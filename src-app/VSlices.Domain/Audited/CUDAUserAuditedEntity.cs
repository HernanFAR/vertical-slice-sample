using VSlices.Domain.Interfaces.Audit;

namespace VSlices.Domain.Audited;

public abstract class CUDAUserAuditedEntity<TCreatedById> : CUUserAuditedEntity<TCreatedById>, IHasActivatedAt.WithActivatedBy<TCreatedById>
{
    public DateTime? DeactivatedAt { get; private set; }

    public TCreatedById? DeactivatedById { get; private set; }

    public bool IsDeactivated { get; private set; }

    public DateTime? ActivatedAt { get; private set; }

    public TCreatedById? ActivatedById { get; private set; }

    protected CUDAUserAuditedEntity(DateTime createdAt, TCreatedById createdById) 
        : base(createdAt, createdById) { }

    protected void Deactivate(DateTime deactivatedAt, TCreatedById? deactivatedById)
    {
        DeactivatedAt = deactivatedAt;
        DeactivatedById = deactivatedById;
        IsDeactivated = true;
    }

    protected void Activate(DateTime activatedAt, TCreatedById? activatedById)
    {
        ActivatedAt = activatedAt;
        ActivatedById = activatedById;
        IsDeactivated = false;
    }

}

public abstract class CUDAUserAuditedEntity<TKey, TCreatedById> : CUUserAuditedEntity<TKey, TCreatedById>, IHasActivatedAt.WithActivatedBy<TCreatedById>
{
    public DateTime? DeactivatedAt { get; private set; }

    public TCreatedById? DeactivatedById { get; private set; }

    public bool IsDeactivated { get; private set; }

    public DateTime? ActivatedAt { get; private set; }

    public TCreatedById? ActivatedById { get; private set; }

    protected CUDAUserAuditedEntity(DateTime createdAt, TCreatedById createdById)
        : base(createdAt, createdById) { }

    protected void Deactivate(DateTime deactivatedAt, TCreatedById? deactivatedById)
    {
        DeactivatedAt = deactivatedAt;
        DeactivatedById = deactivatedById;
        IsDeactivated = true;
    }

    protected void Activate(DateTime activatedAt, TCreatedById? activatedById)
    {
        ActivatedAt = activatedAt;
        ActivatedById = activatedById;
        IsDeactivated = false;
    }

}