using Crud.Domain.ValueObjects;
using LanguageExt.Effects.Traits;

namespace Crud.Domain.Repositories;

public interface IQuestionRepository
{
    Aff<TRuntime, Unit> Create<TRuntime>(Question question)
        where TRuntime : struct, HasCancel<TRuntime>;

    Aff<TRuntime, Question> Read<TRuntime>(QuestionId requestId)
        where TRuntime : struct, HasCancel<TRuntime>;

    Aff<TRuntime, Unit> Update<TRuntime>(Question question)
        where TRuntime : struct, HasCancel<TRuntime>;

    Aff<TRuntime, bool> Exists<TRuntime>(QuestionId id)
        where TRuntime : struct, HasCancel<TRuntime>;

    Aff<TRuntime, bool> Exists<TRuntime>(NonEmptyString text)
        where TRuntime : struct, HasCancel<TRuntime>;

    Aff<TRuntime, Unit> Delete<TRuntime>(Question question)
        where TRuntime : struct, HasCancel<TRuntime>;
}
