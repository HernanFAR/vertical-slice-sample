using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crud.CrossCutting;
using Crud.Domain.DataAccess;
using VSlices.Base;
using VSlices.Infrastructure.Domain.EntityFrameworkCore;

namespace Crud.Infrastructure;

internal sealed class AppUnitOfWork(IQuestionRepository questions) 
    : EfCoreUnitOfWork<AppDbContext>, 
      IAppUnitOfWork
{
    public IQuestionRepository Questions => questions;
}
