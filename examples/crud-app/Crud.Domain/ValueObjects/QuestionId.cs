using System.Runtime.Serialization;

namespace Crud.Domain.ValueObjects;

public sealed class QuestionId : NewType<QuestionId, Guid>
{
    public QuestionId(Guid value) 
        : base(value) { }

    public QuestionId(SerializationInfo info, StreamingContext context) 
        : base(info, context) { }
}
