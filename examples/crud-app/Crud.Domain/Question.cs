namespace Crud.Domain;

public readonly record struct QuestionId(Guid Value)
{
    public static QuestionId New() => new(Guid.NewGuid());
}

public sealed class Question(QuestionId id, string text)
{
    public QuestionId Id { get; } = id;
    
    public string Text { get; private set; } = text;

    public Unit UpdateState(string text)
    {
        Text = text;

        return unit;
    }

    public static Question Create(string text) 
    {
        return new Question(QuestionId.New(), text);
    }
}
