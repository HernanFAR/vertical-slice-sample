// ReSharper disable once CheckNamespace
namespace Crud.Domain;

public static class Question
{
    public static QuestionType Create(NonEmptyString text) => new(QuestionId.Random(), text);

    public static QuestionType Create(QuestionId id, NonEmptyString text) => new(id, text);

    public static Unit UpdateState(this QuestionType question, NonEmptyString text)
    {
        question.Text = text;

        return unit;
    }
}
