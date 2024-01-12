using VSlices.Base;
using VSlices.Base.Responses;
using VSlices.Core.DataAccess;

// ReSharper disable PartialTypeWithSinglePart

namespace VSlices.Core.Handlers;

/// <summary>
/// Base class for handlers of <see cref="IFeature{TResult}"/> that removes entities.
/// </summary>
/// <typeparam name="TRequest">The command to handle</typeparam>
/// <typeparam name="TResult">The expected result</typeparam>
/// <typeparam name="TEntity">The entity to remove</typeparam>
public abstract class DeleteHandler<TRequest, TResult, TEntity> : IHandler<TRequest, TResult>
    where TRequest : IFeature<TResult>
{
    private readonly IRemoveRepository<TEntity> _repository;

    /// <summary>
    /// Creates a new instance using the given <see cref="IRemoveRepository{TEntity}"/>
    /// </summary>
    /// <param name="repository">Repository with remove function</param>
    protected DeleteHandler(IRemoveRepository<TEntity> repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public virtual async ValueTask<Result<TResult>> HandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        var featureValidationResult = await ValidateFeatureRulesAsync(request, cancellationToken);

        if (featureValidationResult.IsFailure)
        {
            return featureValidationResult.Failure;
        }

        var entity = await GetAndProcessEntityAsync(request, cancellationToken);

        var dataAccessResult = await _repository.RemoveAsync(entity, cancellationToken);

        if (dataAccessResult.IsFailure)
        {
            return dataAccessResult.Failure;
        }

        await AfterRemoveAsync(entity, request, cancellationToken);

        return await GetResponseAsync(entity, request, cancellationToken);
    }

    /// <summary>
    /// Validates the use case rules
    /// </summary>
    /// <param name="request">The request to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="ValueTask{T}"/> holding a <see cref="Result{TRequest}"/> of <see cref="Success"/> that represents the result of the operation </returns>
    protected internal abstract ValueTask<Result<Success>> ValidateFeatureRulesAsync(TRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets and process the entity to be removed
    /// </summary>
    /// <param name="request">The request to convert</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="ValueTask{T}"/> with a <typeparamref name="TEntity"/> in</returns>
    protected internal abstract ValueTask<TEntity> GetAndProcessEntityAsync(TRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Represents actions to be executed after the entity is removed
    /// </summary>
    /// <param name="entity">Related Entity</param>
    /// <param name="request">Related Request</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>A <see cref="ValueTask" /> representing the asynchronous operation</returns>
    protected internal virtual ValueTask AfterRemoveAsync(TEntity entity, TRequest request,
        CancellationToken cancellationToken) => ValueTask.CompletedTask;

    /// <summary>
    /// Creates the result to be returned
    /// </summary>
    /// <param name="entity">The removed entity</param>
    /// <param name="request">The handled request</param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// A <see cref="ValueTask{TResponse}"/> with a <typeparamref name="TResult"/> in representing the asynchronous operation
    /// </returns>
    protected internal abstract ValueTask<TResult> GetResponseAsync(TEntity entity, TRequest request,
        CancellationToken cancellationToken);
}

/// <summary>
/// Base class for handlers of <see cref="IFeature{TResponse}"/> that validate entities, before remove them
/// </summary>
/// <remarks>Useful to implement always valid entities</remarks>
/// <typeparam name="TRequest">The command to handle</typeparam>
/// <typeparam name="TResult">The expected result</typeparam>
/// <typeparam name="TEntity">The entity to create</typeparam>
public abstract class EntityValidatedRemoveHandler<TRequest, TResult, TEntity> : IHandler<TRequest, TResult>
    where TRequest : IFeature<TResult>
{
    private readonly IRemoveRepository<TEntity> _repository;

    /// <summary>
    /// Creates a new instance using the given <see cref="IRemoveRepository{TEntity}"/>
    /// </summary>
    /// <param name="repository">Repository with remove function</param>
    protected EntityValidatedRemoveHandler(IRemoveRepository<TEntity> repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public virtual async ValueTask<Result<TResult>> HandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        var featureValidationResult = await ValidateFeatureRulesAsync(request, cancellationToken);

        if (featureValidationResult.IsFailure)
        {
            return featureValidationResult.Failure;
        }

        var entity = await GetAndProcessEntityAsync(request, cancellationToken);

        var entityValidationResult = await ValidateEntityAsync(entity, cancellationToken);

        if (entityValidationResult.IsFailure)
        {
            return entityValidationResult.Failure;
        }

        var dataAccessResult = await _repository.RemoveAsync(entity, cancellationToken);

        if (dataAccessResult.IsFailure)
        {
            return dataAccessResult.Failure;
        }

        await AfterRemoveAsync(entity, request, cancellationToken);

        return await GetResponseAsync(entity, request, cancellationToken);
    }

    /// <summary>
    /// Validates the use case rules
    /// </summary>
    /// <param name="request">The request to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// A <see cref="ValueTask{T}"/> holding a <see cref="Result{TRequest}"/> of <see cref="Success"/> that represents
    /// the result of the operation
    /// </returns>
    protected internal abstract ValueTask<Result<Success>> ValidateFeatureRulesAsync(TRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets and process the entity to be persisted
    /// </summary>
    /// <param name="request">The request to convert</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="ValueTask{T}"/> with a <typeparamref name="TEntity"/> in</returns>
    protected internal abstract ValueTask<TEntity> GetAndProcessEntityAsync(TRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Validates the before processed entity
    /// </summary>
    /// <param name="entity">The created entity</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>A <see cref="ValueTask{T}"/> holding a <see cref="Result{TRequest}"/> of <see cref="Success"/> that represents the result of the operation </returns>
    protected internal abstract ValueTask<Result<Success>> ValidateEntityAsync(TEntity entity,
        CancellationToken cancellationToken);

    /// <summary>
    /// Represents actions to be executed after the entity is removed
    /// </summary>
    /// <param name="entity">Related Entity</param>
    /// <param name="request">Related Request</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>A <see cref="ValueTask" /> representing the asynchronous operation</returns>
    protected internal virtual ValueTask AfterRemoveAsync(TEntity entity, TRequest request,
        CancellationToken cancellationToken) => ValueTask.CompletedTask;

    /// <summary>
    /// Creates the result to be returned
    /// </summary>
    /// <param name="entity">The removed entity</param>
    /// <param name="request">The handled request</param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// A <see cref="ValueTask{TResponse}"/> with a <typeparamref name="TResult"/> in representing the asynchronous operation
    /// </returns>
    protected internal abstract ValueTask<TResult> GetResponseAsync(TEntity entity, TRequest request,
        CancellationToken cancellationToken);
}

/// <summary>
/// Base class for handlers of <see cref="IFeature{TResult}"/> that removes entities.
/// </summary>
/// <remarks>Useful in commands without result</remarks>
/// <typeparam name="TRequest">The command to handle</typeparam>
/// <typeparam name="TEntity">The entity to remove</typeparam>
public abstract class DeleteHandler<TRequest, TEntity> : DeleteHandler<TRequest, Success, TEntity>
    where TRequest : IFeature<Success>
{
    /// <inheritdoc />
    protected DeleteHandler(IRemoveRepository<TEntity> repository) : base(repository)
    { }

    /// <inheritdoc />
    protected internal override async ValueTask<Success> GetResponseAsync(TEntity _, TRequest r, CancellationToken c)
        => Success.Value;
}

/// <summary>
/// Base class for handlers of <see cref="IFeature{TResult}"/> that validates entities, before remove them
/// </summary>
/// <remarks>Useful in commands without result and to implement always-valid entities</remarks>
/// <typeparam name="TRequest">The command to handle</typeparam>
/// <typeparam name="TEntity">The entity to remove</typeparam>
public abstract class EntityValidatedRemoveHandler<TRequest, TEntity> : EntityValidatedRemoveHandler<TRequest, Success, TEntity>
    where TRequest : IFeature<Success>
{
    /// <inheritdoc />
    protected EntityValidatedRemoveHandler(IRemoveRepository<TEntity> repository) : base(repository)
    { }

    /// <inheritdoc />
    protected internal override async ValueTask<Success> GetResponseAsync(TEntity _, TRequest r, CancellationToken c)
        => Success.Value;

}
