using VSlices.Domain.Repository;

namespace Crud.Domain.Rules.DataAccess;

public interface IAppUnitOfWork : IUnitOfWork
{
    IQuestionRepository Questions { get; }
}
