using Crud.Domain.ValueObjects;
using LanguageExt.Effects.Traits;
using LanguageExt.SysX.Live;

namespace Crud.Domain.Repositories;

public interface IQuestionRepository
{
    Aff<Runtime, Unit> Create(Question question);

    Aff<Runtime, Question> Read(QuestionId requestId);

    Aff<Runtime, Unit> Update(Question question);

    Aff<Runtime, bool> Exists(QuestionId id);

    Aff<Runtime, bool> Exists(NonEmptyString text);

    Aff<Runtime, Unit> Delete(Question question);
}
