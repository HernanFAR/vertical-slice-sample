using VSlices.Domain.Interfaces.Audit;

namespace VSlices.Domain.Audited;

public abstract class CUAuditedEntity : CAuditedEntity, IHasUpdatedAt
{
    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; set; }

    protected CUAuditedEntity(DateTime createdAt) : base(createdAt) { }

    protected void Update(DateTime updatedAt)
    {
        UpdatedAt = updatedAt;
    }

}

public abstract class CUAuditedEntity<TKey> : CAuditedEntity<TKey>, IHasUpdatedAt
{
    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; set; }

    protected CUAuditedEntity(DateTime createdAt) : base(createdAt) { }

    protected void Update(DateTime updatedAt)
    {
        UpdatedAt = updatedAt;
    }

}
