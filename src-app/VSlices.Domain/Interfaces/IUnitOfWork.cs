using VSlices.Base.Responses;

namespace VSlices.Domain.Interfaces;

/// <summary>
/// Defines a unit of work used to interact with the database
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Starts a new transaction
    /// </summary>
    /// <param name="cancellationToken">
    /// Cancellation token
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask{T}"/> holding a <see cref="Result{T}"/> of <see cref="IDisposable"/> that
    /// </returns>
    ValueTask<IDisposable> StartTransactionAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Rolls back the changes to the database
    /// </summary>
    /// <param name="cancellationToken">
    /// Cancellation token
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask"/> representing the operation
    /// </returns>
    ValueTask RollbackAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Commits the changes to the database
    /// </summary>
    /// <param name="cancellationToken">
    /// Cancellation token
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask"/> representing the operation
    /// </returns>
    ValueTask CommitAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Saves the changes to the database
    /// </summary>
    /// <param name="cancellationToken">
    /// Cancellation token
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask"/> representing the operation
    /// </returns>
    ValueTask SaveChangesAsync(CancellationToken cancellationToken);

}
