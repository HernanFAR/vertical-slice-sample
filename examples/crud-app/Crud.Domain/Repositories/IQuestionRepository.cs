using Crud.Domain.ValueObjects;
using VSlices.Core;

namespace Crud.Domain.Repositories;

public interface IQuestionRepository
{
    Eff<VSlicesRuntime, Unit> Create(Question question);

    Eff<VSlicesRuntime, Question> Read(QuestionId requestId);

    Eff<VSlicesRuntime, Unit> Update(Question question);

    Eff<VSlicesRuntime, bool> Exists(QuestionId id);

    Eff<VSlicesRuntime, bool> Exists(QuestionId id, NonEmptyString name);

    Eff<VSlicesRuntime, bool> Exists(NonEmptyString text);

    Eff<VSlicesRuntime, Unit> Delete(Question question);
}
