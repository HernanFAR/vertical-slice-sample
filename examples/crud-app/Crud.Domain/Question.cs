using Crud.Domain.ValueObjects;
using VSlices.Domain;

namespace Crud.Domain;

public sealed class Question : AggregateRoot<QuestionId>
{
    private Question(QuestionId id, NonEmptyString text) : base(id)
    {
        Text = text;
    }
    
    public NonEmptyString Text { get; private set; }

    public Unit UpdateState(NonEmptyString text)
    {
        Text = text;

        return unit;
    }

    public static Question Create(NonEmptyString text) => Create(QuestionId.Random(), text);

    public static Question Create(QuestionId id, NonEmptyString text) => new(id, text);
}
