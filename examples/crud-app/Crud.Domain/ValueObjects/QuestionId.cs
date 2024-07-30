using System.Runtime.Serialization;
using LanguageExt.Traits;

namespace Crud.Domain.ValueObjects;

public sealed class QuestionId : NewType<QuestionId, Guid, QuestionId>, 
                                 Pred<Guid>
{
    public QuestionId(Guid value) 
        : base(value) { }

    public QuestionId(SerializationInfo info, StreamingContext context) 
        : base(info, context) { }

    public static bool True(Guid value) => value != Guid.Empty;

    public static QuestionId Random() => new(Guid.NewGuid());

    public static QuestionId From(Guid value) => new(value);
}
