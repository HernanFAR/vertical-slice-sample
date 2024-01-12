using VSlices.Base;
using VSlices.Base.Responses;
using VSlices.Core.DataAccess;

// ReSharper disable PartialTypeWithSinglePart

namespace VSlices.Core.Handlers;

/// <summary>
/// Base class for handlers of <see cref="IFeature{TResult}"/> that update entities.
/// </summary>
/// <typeparam name="TRequest">The command to handle</typeparam>
/// <typeparam name="TResult">The expected result</typeparam>
/// <typeparam name="TEntity">The entity to update</typeparam>
public abstract class UpdateHandler<TRequest, TResult, TEntity> : IHandler<TRequest, TResult>
    where TRequest : IFeature<TResult>
{
    private readonly IUpdateRepository<TEntity> _repository;

    /// <summary>
    /// Creates a new instance using the given <see cref="IUpdateRepository{TEntity}"/>
    /// </summary>
    /// <param name="repository">Repository with update function</param>
    protected UpdateHandler(IUpdateRepository<TEntity> repository)
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

        var dataAccessResult = await _repository.UpdateAsync(entity, cancellationToken);

        if (dataAccessResult.IsFailure)
        {
            return dataAccessResult.Failure;
        }

        await AfterUpdateAsync(entity, request, cancellationToken);

        return await GetResultAsync(entity, request, cancellationToken);
    }

    /// <summary>
    /// Validates the use case rules
    /// </summary>
    /// <param name="request">The request to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// A <see cref="ValueTask{T}"/> holding a <see cref="Result{TRequest}"/> of <see cref="Success"/> that
    /// represents the result of the operation
    /// </returns>
    protected internal abstract ValueTask<Result<Success>> ValidateFeatureRulesAsync(TRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets and process the entity to be updated
    /// </summary>
    /// <param name="request">The request to convert</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="ValueTask{T}"/> with a <typeparamref name="TEntity"/> in</returns>
    protected internal abstract ValueTask<TEntity> GetAndProcessEntityAsync(TRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Represents actions to be executed after the entity is updated
    /// </summary>
    /// <param name="entity">Related Entity</param>
    /// <param name="request">Related Request</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>A <see cref="ValueTask" /> representing the asynchronous operation</returns>
    protected internal virtual ValueTask AfterUpdateAsync(TEntity entity, TRequest request,
        CancellationToken cancellationToken) => ValueTask.CompletedTask;

    /// <summary>
    /// Creates the result to be returned
    /// </summary>
    /// <param name="entity">The updated entity</param>
    /// <param name="request">The handled request</param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> with a <typeparamref name="TResult"/> in representing the asynchronous
    /// operation
    /// </returns>
    protected internal abstract ValueTask<TResult> GetResultAsync(TEntity entity, TRequest request,
        CancellationToken cancellationToken);
}

/// <summary>
/// Base class for handlers of <see cref="IFeature{TResult}"/> that validates entities, before update them
/// </summary>
/// <remarks>Useful to implement always valid entities</remarks>
/// <typeparam name="TRequest">The command to handle</typeparam>
/// <typeparam name="TResult">The expected result</typeparam>
/// <typeparam name="TEntity">The entity to update</typeparam>
public abstract class EntityValidatedUpdateHandler<TRequest, TResult, TEntity> : IHandler<TRequest, TResult>
    where TRequest : IFeature<TResult>
{
    private readonly IUpdateRepository<TEntity> _repository;

    /// <summary>
    /// Creates a new instance using the given <see cref="IUpdateRepository{TEntity}"/>
    /// </summary>
    /// <param name="repository">Repository with update function</param>
    protected EntityValidatedUpdateHandler(IUpdateRepository<TEntity> repository)
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

        var dataAccessResult = await _repository.UpdateAsync(entity, cancellationToken);

        if (dataAccessResult.IsFailure)
        {
            return dataAccessResult.Failure;
        }

        await AfterUpdateAsync(entity, request, cancellationToken);

        return await GetResultAsync(entity, request, cancellationToken);
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
    /// Represents actions to be executed after the entity is updated
    /// </summary>
    /// <param name="entity">Related Entity</param>
    /// <param name="request">Related Request</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>A <see cref="ValueTask" /> representing the asynchronous operation</returns>
    protected internal virtual ValueTask AfterUpdateAsync(TEntity entity, TRequest request,
        CancellationToken cancellationToken) => ValueTask.CompletedTask;

    /// <summary>
    /// Creates the result to be returned
    /// </summary>
    /// <param name="entity">The updated entity</param>
    /// <param name="request">The handled request</param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> with a <typeparamref name="TResult"/> in representing the asynchronous operation
    /// </returns>
    protected internal abstract ValueTask<TResult> GetResultAsync(TEntity entity, TRequest request,
        CancellationToken cancellationToken);
}

/// <summary>
/// Base class for handlers of <see cref="IFeature{TResult}"/> that updates entities.
/// </summary>
/// <remarks>Useful in commands without result</remarks>
/// <typeparam name="TRequest">The command to handle</typeparam>
/// <typeparam name="TEntity">The entity to update</typeparam>
public abstract class UpdateHandler<TRequest, TEntity> : UpdateHandler<TRequest, Success, TEntity>
    where TRequest : IFeature<Success>
{
    /// <inheritdoc />
    protected UpdateHandler(IUpdateRepository<TEntity> repository) : base(repository)
    { }

    /// <inheritdoc />
    protected internal override async ValueTask<Success> GetResultAsync(TEntity _, TRequest r, CancellationToken c)
        => Success.Value;
}

/// <summary>
/// Base class for handlers of <see cref="IFeature{TResult}"/> that validates entities, before update them
/// </summary>
/// <remarks>Useful in commands without result and to implement always-valid entities</remarks>
/// <typeparam name="TRequest">The command to handle</typeparam>
/// <typeparam name="TEntity">The entity to update</typeparam>
public abstract class EntityValidatedUpdateHandler<TRequest, TEntity> : EntityValidatedUpdateHandler<TRequest, Success, TEntity>
    where TRequest : IFeature<Success>
{
    /// <inheritdoc />
    protected EntityValidatedUpdateHandler(IUpdateRepository<TEntity> repository) : base(repository)
    { }

    /// <inheritdoc />
    protected internal override async ValueTask<Success> GetResultAsync(TEntity _, TRequest r, CancellationToken c)
        => Success.Value;
}
