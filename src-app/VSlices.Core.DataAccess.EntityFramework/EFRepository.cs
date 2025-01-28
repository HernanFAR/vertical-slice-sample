using Microsoft.EntityFrameworkCore;
using VSlices.Base.Responses;
using VSlices.Core.Abstracts.Responses;
using VSlices.Domain.Interfaces;

namespace VSlices.Core.DataAccess.EntityFramework;

/// <summary>
/// A reposito
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TDbContext"></typeparam>
public abstract class EFRepository<TEntity, TDbContext> : IRepository<TEntity>
    where TEntity : class
    where TDbContext : DbContext
{
    protected readonly TDbContext _dbContext;

    protected EFRepository(TDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<Result<TEntity>> CreateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await _dbContext.AddAsync(entity, cancellationToken);

        return entity;

    }

    public async ValueTask<Result<TEntity>> ReadAsync(object[] key, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.FindAsync<TEntity>(key);

        if (entity == null) return BusinessFailure.Of.NotFoundResource("Not ");

        return entity;
    }

    public ValueTask<Result<TEntity>> UpdateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public ValueTask<Result<TEntity>> DeleteAsync(TEntity entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
