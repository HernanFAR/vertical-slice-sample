using VSlices.Base.Responses;

namespace VSlices.Domain.DataAccess;

/// <summary>
/// Defines a unit of work used to interact with the database
/// </summary>
public interface IUnitOfWork
{
    ValueTask<Result<IDisposableTransaction>> StartTransactionAsync(CancellationToken cancellationToken);

    ValueTask<Result<Success>> SaveChangesAsync(CancellationToken cancellationToken);

}

public interface IDisposableTransaction : IDisposable
{
    ValueTask<Result<Success>> RollbackAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Commits the changes to the database
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// A <see cref="ValueTask{T}"/> holding a <see cref="Result{T}"/> of <see cref="Success"/> that
    /// represents the result of the operation
    /// </returns>
    ValueTask<Result<Success>> CommitAsync(CancellationToken cancellationToken);
}
