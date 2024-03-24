using VSlices.Base.Responses;

namespace VSlices.Domain.Interfaces;

/// <summary>
/// Defines a repository used to interact with the <typeparam name="TEntity" /> entity
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IRepository<TEntity>
{
    /// <summary>
    /// Creates a new <typeparamref name="TEntity"/> entity
    /// </summary>
    /// <param name="entity">The values to create the entity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// A <see cref="ValueTask{T}"/> holding a <see cref="Result{TRequest}"/> of <see cref="Success"/> that
    /// represents the result of the operation
    /// </returns>
    ValueTask<Result<TEntity>> CreateAsync(TEntity entity, CancellationToken cancellationToken);

    /// <summary>
    /// Queries the entity using the primary keys
    /// </summary>
    /// <param name="key">The primary keys of the object</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// A <see cref="ValueTask{T}"/> holding a <see cref="Result{TRequest}"/> of <typeparamref cref="TEntity"/>
    /// that represents the result of the operation
    /// </returns>
    ValueTask<Result<TEntity>> ReadAsync(object[] key, CancellationToken cancellationToken);

    /// <summary>
    /// Updates a new <typeparamref name="TEntity"/> entity
    /// </summary>
    /// <param name="entity">The values to update the entity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// A <see cref="ValueTask{T}"/> holding a <see cref="Result{TRequest}"/> of <see cref="Success"/> that
    /// represents the result of the operation
    /// </returns>
    ValueTask<Result<TEntity>> UpdateAsync(TEntity entity, CancellationToken cancellationToken);


    /// <summary>
    /// Deletes a new <typeparamref name="TEntity"/> entity
    /// </summary>
    /// <param name="entity">The entity to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// A <see cref="ValueTask{T}"/> holding a <see cref="Result{TRequest}"/> of <see cref="Success"/> that
    /// represents the result of the operation
    /// </returns>
    ValueTask<Result<TEntity>> DeleteAsync(TEntity entity, CancellationToken cancellationToken);

}
