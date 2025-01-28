using Crud.CrossCutting;
using Crud.Domain.Rules.DataAccess;
using VSlices.Infrastructure.Domain.EntityFrameworkCore;

namespace Crud.Infrastructure;

internal sealed class AppUnitOfWork(IQuestionRepository questions)
    : EfCoreUnitOfWork<AppDbContext>,
      IAppUnitOfWork
{
    public IQuestionRepository Questions => questions;
}
