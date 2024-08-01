using Crud.Domain.ValueObjects;
using VSlices.Domain;

namespace Crud.Domain;

public sealed class QuestionType : AggregateRoot<QuestionId>
{
    internal QuestionType(QuestionId id, NonEmptyString text) : base(id)
    {
        Text = text;
    }
    
    public NonEmptyString Text { get; internal set; }

}
