using VSlices.Domain.Repository;

namespace Crud.Domain.Rules.DataAccess;

public interface IQuestionRepository : IRepository<QuestionType, QuestionId>
{
    Eff<VSlicesRuntime, bool> Exists(QuestionId id, NonEmptyString name);

    Eff<VSlicesRuntime, bool> Exists(NonEmptyString text);

}
