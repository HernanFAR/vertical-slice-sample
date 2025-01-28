// ReSharper disable once CheckNamespace
namespace Crud.Domain;

public static class Question
{
    public static QuestionType Create(CategoryId categoryId, NonEmptyString text) => 
        new(QuestionId.Random(), 
            CategoryType.Find(categoryId),
            text);

    public static QuestionType Create(QuestionId id, CategoryId categoryId, NonEmptyString text) => 
        new(id, 
            CategoryType.Find(categoryId),
            text);

    public static Unit UpdateState(this QuestionType question, CategoryId categoryId, NonEmptyString text)
    {
        question.Category = CategoryType.Find(categoryId);
        question.Text = text;

        return unit;
    }
}
