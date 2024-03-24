using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using VSlices.Base.Responses;
using VSlices.Domain.Interfaces;

namespace VSlices.Core.DataAccess.EntityFramework;

/// <summary>
/// Represents a unit of work implementation using Entity Framework
/// </summary>
/// <typeparam name="TDbContext">
/// The type of the database context
/// </typeparam>
public abstract class EntityFrameworkUnitOfWork<TDbContext> : IUnitOfWork
    where TDbContext : DbContext
{
    readonly TDbContext _dbContext;
    IDbContextTransaction? _transaction;

    /// <summary>
    /// Initializes a new instance of <see cref="EntityFrameworkUnitOfWork{TDbContext}"/>
    /// </summary>
    /// <param name="dbContext">
    /// The database context
    /// </param>
    protected EntityFrameworkUnitOfWork(TDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async ValueTask<IDisposable> StartTransactionAsync(CancellationToken cancellationToken)
    {
        _transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        return _transaction;
    }

    /// <inheritdoc />
    public async ValueTask RollbackAsync(CancellationToken cancellationToken)
    {
        _ = _transaction ?? throw new InvalidOperationException("Transaction was null");
        await _transaction.RollbackAsync(cancellationToken);

    }

    /// <inheritdoc />
    public async ValueTask CommitAsync(CancellationToken cancellationToken)
    {
        _ = _transaction ?? throw new InvalidOperationException("Transaction was null");
        await _transaction.CommitAsync(cancellationToken);

    }

    /// <inheritdoc />
    public async ValueTask SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);

    }
}
