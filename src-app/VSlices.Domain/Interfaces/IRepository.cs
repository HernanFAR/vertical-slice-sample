using VSlices.Base.Responses;

namespace VSlices.Domain.Interfaces;

/// <summary>
/// Defines a repository for the specified <typeparamref name="TRoot" />
/// </summary>
/// <typeparam name="TRoot"><see cref="IAggregateRoot{TKey}" /> that this repository manages</typeparam>
/// <typeparam name="TKey">Key of the <see cref="IAggregateRoot{TKey}" /></typeparam>
public interface IRepository<TRoot, TKey>
    where TRoot : IAggregateRoot<TKey>
    where TKey : struct, IEquatable<TKey>
{
    /// <summary>
    /// Creates a new <typeparamref name="TRoot"/>
    /// </summary>
    /// <param name="entity">The values to create the entity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// A <see cref="ValueTask{T}"/> holding a <see cref="Result{TRequest}"/> of <see cref="Success"/> that
    /// represents the result of the operation
    /// </returns>
    ValueTask<Result<TRoot>> AddAsync(TRoot entity, CancellationToken cancellationToken);

    /// <summary>
    /// Queries the entity using the primary keys
    /// </summary>
    /// <param name="key">The primary keys values of the <typeparamref name="TRoot"/></param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// A <see cref="ValueTask{T}"/> holding a <see cref="Result{TRequest}"/> of <typeparamref name="TRoot"/>
    /// that represents the result of the operation
    /// </returns>
    ValueTask<Result<TRoot>> ReadAsync(TKey key, CancellationToken cancellationToken);

    /// <summary>
    /// Updates a <typeparamref name="TRoot"/>
    /// </summary>
    /// <param name="entity">The values to update the entity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// A <see cref="ValueTask{T}"/> holding a <see cref="Result{TRequest}"/> of <see cref="Success"/> that
    /// represents the result of the operation
    /// </returns>
    ValueTask<Result<TRoot>> UpdateAsync(TRoot entity, CancellationToken cancellationToken);


    /// <summary>
    /// Deletes a <typeparamref name="TRoot"/>
    /// </summary>
    /// <param name="entity">The <typeparamref name="TRoot"/> to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// A <see cref="ValueTask{T}"/> holding a <see cref="Result{TRequest}"/> of <see cref="Success"/> that
    /// represents the result of the operation
    /// </returns>
    ValueTask<Result<TRoot>> DeleteAsync(TRoot entity, CancellationToken cancellationToken);

    /// <summary>
    /// Verifies the existense of the entity using the primary keys
    /// </summary>
    /// <param name="key">The primary keys values of the <typeparamref name="TRoot"/></param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// A <see cref="ValueTask{T}"/> holding a <see cref="Result{TRequest}"/> of a <see cref="bool"/> that
    /// that represents the result of the operation
    /// </returns>
    ValueTask<Result<bool>> AnyAsync(TKey key, CancellationToken cancellationToken);

}
