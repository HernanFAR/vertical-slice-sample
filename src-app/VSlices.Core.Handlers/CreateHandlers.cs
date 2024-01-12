using VSlices.Base;
using VSlices.Base.Responses;
using VSlices.Core.DataAccess;

namespace VSlices.Core.Handlers;

/// <summary>
/// Base class for handlers of <see cref="IFeature{TResult}"/> that creates entities.
/// </summary>
/// <typeparam name="TRequest">The command to handle</typeparam>
/// <typeparam name="TResult">The expected result</typeparam>
/// <typeparam name="TEntity">The entity to create</typeparam>
public abstract class CreateHandler<TRequest, TResult, TEntity> : IHandler<TRequest, TResult>
    where TRequest : IFeature<TResult>
{
    private readonly ICreateRepository<TEntity> _repository;

    /// <summary>
    /// Creates a new instance using the given <see cref="ICreateRepository{TEntity}"/>
    /// </summary>
    /// <param name="repository">Repository with create function</param>
    protected CreateHandler(ICreateRepository<TEntity> repository)
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

        var entity = await CreateEntityAsync(request, cancellationToken);

        var dataAccessResult = await _repository.CreateAsync(entity, cancellationToken);

        if (dataAccessResult.IsFailure)
        {
            return dataAccessResult.Failure;
        }

        await AfterCreationAsync(entity, request, cancellationToken);

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
    /// Creates the entity to be persisted
    /// </summary>
    /// <param name="request">The request to convert</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="ValueTask{T}"/> with a <typeparamref name="TEntity"/> in</returns>
    protected internal abstract ValueTask<TEntity> CreateEntityAsync(TRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Represents actions to be executed after the entity is created
    /// </summary>
    /// <param name="entity">Related Entity</param>
    /// <param name="request">Related Request</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>A <see cref="ValueTask" /> representing the asynchronous operation</returns>
    protected internal virtual ValueTask AfterCreationAsync(TEntity entity, TRequest request,
        CancellationToken cancellationToken) => ValueTask.CompletedTask;

    /// <summary>
    /// Creates the result to be returned
    /// </summary>
    /// <param name="entity">The created entity</param>
    /// <param name="request">The handled request</param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// A <see cref="ValueTask{TResponse}"/> with a <typeparamref name="TResult"/> in representing the asynchronous operation
    /// </returns>
    protected internal abstract ValueTask<TResult> GetResponseAsync(TEntity entity, TRequest request,
        CancellationToken cancellationToken);
}

/// <summary>
/// Base class for handlers of <see cref="IFeature{TResult}"/> that validate entities, before create them
/// </summary>
/// <remarks>Useful to implement always valid entities</remarks>
/// <typeparam name="TRequest">The command to handle</typeparam>
/// <typeparam name="TResult">The expected result</typeparam>
/// <typeparam name="TEntity">The entity to create</typeparam>
public abstract class EntityValidatedCreateHandler<TRequest, TResult, TEntity> : IHandler<TRequest, TResult>
    where TRequest : IFeature<TResult>
{
    private readonly ICreateRepository<TEntity> _repository;

    /// <summary>
    /// Creates a new instance using the given <see cref="ICreateRepository{TEntity}"/>
    /// </summary>
    /// <param name="repository">Repository with create function</param>
    protected EntityValidatedCreateHandler(ICreateRepository<TEntity> repository)
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

        var entity = await CreateEntityAsync(request, cancellationToken);

        var entityValidationResult = await ValidateEntityAsync(entity, cancellationToken);

        if (entityValidationResult.IsFailure)
        {
            return entityValidationResult.Failure;
        }

        var dataAccessResult = await _repository.CreateAsync(entity, cancellationToken);

        if (dataAccessResult.IsFailure)
        {
            return dataAccessResult.Failure;
        }

        await AfterCreationAsync(entity, request, cancellationToken);

        return await GetResponseAsync(entity, request, cancellationToken);
    }

    /// <summary>
    /// Validates the use case rules
    /// </summary>
    /// <param name="request">The request to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="ValueTask{T}"/> holding a <see cref="Result{T}"/> of <see cref="Success"/> that represents the result of the operation </returns>
    protected internal abstract ValueTask<Result<Success>> ValidateFeatureRulesAsync(TRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Creates the entity to be persisted
    /// </summary>
    /// <param name="request">The request to convert</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="ValueTask{T}"/> with a <typeparamref name="TEntity"/> in</returns>
    protected internal abstract ValueTask<TEntity> CreateEntityAsync(TRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Validates the before created entity
    /// </summary>
    /// <param name="entity">The created entity</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>A <see cref="ValueTask{T}"/> holding a <see cref="Result{TResponse}" /> of <see cref="Success"/> that represents the result of the process</returns>
    protected internal abstract ValueTask<Result<Success>> ValidateEntityAsync(TEntity entity,
        CancellationToken cancellationToken);

    /// <summary>
    /// Represents actions to be executed after the entity is created
    /// </summary>
    /// <param name="entity">Related Entity</param>
    /// <param name="request">Related Request</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>A <see cref="ValueTask" /> representing the asynchronous operation</returns>
    protected internal virtual ValueTask AfterCreationAsync(TEntity entity, TRequest request,
        CancellationToken cancellationToken) => ValueTask.CompletedTask;

    /// <summary>
    /// Creates the result to be returned
    /// </summary>
    /// <param name="entity">The created entity</param>
    /// <param name="request">The handled request</param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// A <see cref="ValueTask{TResponse}"/> with a <typeparamref name="TResult"/> in representing the asynchronous operation
    /// </returns>
    protected internal abstract ValueTask<TResult> GetResponseAsync(TEntity entity, TRequest request,
        CancellationToken cancellationToken);
}

/// <summary>
/// Base class for handlers of <see cref="IFeature{TResult}"/> that creates entities.
/// </summary>
/// <remarks>Useful in commands without specific result</remarks>
/// <typeparam name="TRequest">The command to handle</typeparam>
/// <typeparam name="TEntity">The entity to create</typeparam>
public abstract class CreateHandler<TRequest, TEntity> : CreateHandler<TRequest, Success, TEntity>
    where TRequest : IFeature<Success>
{
    /// <inheritdoc />
    protected CreateHandler(ICreateRepository<TEntity> repository) : base(repository)
    { }

    /// <inheritdoc />
    protected internal override async ValueTask<Success> GetResponseAsync(TEntity _, TRequest r, CancellationToken c)
        => Success.Value;
}

/// <summary>
/// Base class for handlers of <see cref="IFeature{TResult}"/> that validates entities, before create them
/// </summary>
/// <remarks>Useful in commands without specific result and to implement always-valid entities</remarks>
/// <typeparam name="TRequest">The command to handle</typeparam>
/// <typeparam name="TEntity">The entity to create</typeparam>
public abstract class EntityValidatedCreateHandler<TRequest, TEntity> : EntityValidatedCreateHandler<TRequest, Success, TEntity>
    where TRequest : IFeature<Success>
{
    /// <inheritdoc />
    protected EntityValidatedCreateHandler(ICreateRepository<TEntity> repository) : base(repository)
    { }

    /// <inheritdoc />
    protected internal override async ValueTask<Success> GetResponseAsync(TEntity _, TRequest r, CancellationToken c)
        => Success.Value;
}
