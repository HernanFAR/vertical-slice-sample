using Crud.Domain.ValueObjects;
using VSlices.Domain.Repository;

namespace Crud.Domain.DataAccess;

public interface IQuestionRepository : IRepository<Question, QuestionId>
{
    Eff<VSlicesRuntime, bool> Exists(QuestionId id, NonEmptyString name);

    Eff<VSlicesRuntime, bool> Exists(NonEmptyString text);

}
