using LanguageExt;
using Microsoft.EntityFrameworkCore;
using VSlices.Base;
using VSlices.Domain.Repository;
using static VSlices.VSlicesPrelude;
using static LanguageExt.Prelude;

namespace VSlices.Infrastructure.Domain.EntityFrameworkCore;

/// <summary>
/// EF Core implementation of the <see cref="IUnitOfWork"/>
/// </summary>
public abstract class EfCoreUnitOfWork<TDbContext> : IUnitOfWork
    where TDbContext : DbContext
{
    /// <inheritdoc />
    public Eff<VSlicesRuntime, Unit> SaveChanges() => 
        from context in provide<TDbContext>()
        from token in cancelToken
        from _ in liftEff(() => context.SaveChangesAsync(token))
        select unit;
}
