using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSlices.Domain.Repository;

namespace Crud.Domain.DataAccess;

public interface IAppUnitOfWork : IUnitOfWork
{
    IQuestionRepository  Questions { get; } 
}
