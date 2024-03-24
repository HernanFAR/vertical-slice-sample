using VSlices.Domain.Interfaces.Audit;

namespace VSlices.Domain.Audited;

public abstract class CUDAAuditedEntity : CUDAuditedEntity, IHasActivatedAt
{
    public DateTime? ActivatedAt { get; private set; }

    protected CUDAAuditedEntity(DateTime createdAt) : base(createdAt) { }

    protected void Activate(DateTime activatedAt)
    {
        ActivatedAt = activatedAt;
    }
}

public abstract class CUDAAuditedEntity<TKey> : CUDAuditedEntity<TKey>, IHasActivatedAt
{
    public DateTime? ActivatedAt { get; private set; }

    protected CUDAAuditedEntity(DateTime createdAt) : base(createdAt) { }

    protected void Activate(DateTime activatedAt)
    {
        ActivatedAt = activatedAt;
    }

}
