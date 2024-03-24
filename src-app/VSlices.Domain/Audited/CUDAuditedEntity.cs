using VSlices.Domain.Interfaces.Audit;

namespace VSlices.Domain.Audited;

public abstract class CUDAuditedEntity : CUAuditedEntity, IHasDeactivatedAt
{
    public DateTime? DeactivatedAt { get; private set; }

    public bool IsDeactivated { get; private set; }

    protected CUDAuditedEntity(DateTime createdAt) : base(createdAt) { }

    protected void Deactivate(DateTime deactivatedAt)
    {
        DeactivatedAt = deactivatedAt;
        IsDeactivated = true;
    }
}

public abstract class CUDAuditedEntity<TKey> : CUAuditedEntity<TKey>, IHasDeactivatedAt
{
    public DateTime? DeactivatedAt { get; private set; }

    public bool IsDeactivated { get; private set; }

    protected CUDAuditedEntity(DateTime createdAt) : base(createdAt) { }

    protected void Deactivate(DateTime deactivatedAt)
    {
        DeactivatedAt = deactivatedAt;
        IsDeactivated = true;
    }

}
