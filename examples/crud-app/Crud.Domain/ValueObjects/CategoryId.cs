using System.Runtime.Serialization;

namespace Crud.Domain.ValueObjects;

public sealed class CategoryId : NewType<CategoryId, Guid>
{
    public CategoryId(Guid value) : base(value) { }

    public CategoryId(SerializationInfo info, StreamingContext context) : base(info, context) { }

}