using Crud.Domain.ValueObjects;
using VSlices.Domain;

namespace Crud.Domain;

public sealed class QuestionType : AggregateRoot<QuestionId>
{
    internal QuestionType(QuestionId id, CategoryType category, NonEmptyString text) : base(id)
    {
        Category = category;
        Text       = text;
    }

    public CategoryType Category { get; internal set; }

    public NonEmptyString Text { get; internal set; }

}
