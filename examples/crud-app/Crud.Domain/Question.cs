namespace Crud.Domain;

public readonly record struct QuestionId(Guid Value)
{
    public static QuestionId New() => new(Guid.NewGuid());
}

public sealed class Question
{
    internal Question(QuestionId id, string text)
    {
        Id = id;
        Text = text;
    }

    public QuestionId Id { get; }
    
    public string Text { get; private set; }

    public Unit UpdateState(string text)
    {
        Text = text;

        return unit;
    }

    internal static Question Create(string text) 
    {
        return new Question(QuestionId.New(), text);
    }
}
