using Crud.Domain.ValueObjects;

namespace Crud.Domain;

public sealed class Question
{
    internal Question(QuestionId id, NonEmptyString text)
    {
        Id = id;
        Text = text;
    }

    public QuestionId Id { get; }
    
    public NonEmptyString Text { get; private set; }

    public Unit UpdateState(NonEmptyString text)
    {
        Text = text;

        return unit;
    }

    internal static Question Create(NonEmptyString text) 
    {
        return new Question(QuestionId.New(Guid.NewGuid()), text);
    }
}
