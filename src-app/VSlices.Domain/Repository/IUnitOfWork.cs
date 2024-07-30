using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LanguageExt;
using VSlices.Base;

namespace VSlices.Domain.Repository;

/// <summary>
/// Defines a contract for a Unit of Work pattern
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves all changes made in this context to the database
    /// </summary>
    Eff<VSlicesRuntime, Unit> SaveChanges();

}
