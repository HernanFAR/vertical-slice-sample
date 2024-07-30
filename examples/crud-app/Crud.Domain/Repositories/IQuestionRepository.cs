using Crud.Domain.ValueObjects;
using VSlices.Core;

namespace Crud.Domain.Repositories;

public interface IQuestionRepository
{
    Eff<HandlerRuntime, Unit> Create(Question question);

    Eff<HandlerRuntime, Question> Read(QuestionId requestId);

    Eff<HandlerRuntime, Unit> Update(Question question);

    Eff<HandlerRuntime, bool> Exists(QuestionId id);

    Eff<HandlerRuntime, bool> Exists(QuestionId id, NonEmptyString name);

    Eff<HandlerRuntime, bool> Exists(NonEmptyString text);

    Eff<HandlerRuntime, Unit> Delete(Question question);
}
