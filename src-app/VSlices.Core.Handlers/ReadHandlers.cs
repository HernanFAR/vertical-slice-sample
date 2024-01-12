using VSlices.Base;
using VSlices.Base.Responses;
using VSlices.Core.DataAccess;

namespace VSlices.Core.Handlers;

/// <summary>
/// Base class for handlers of <see cref="IFeature{TResult}"/> that reads data
/// </summary>
/// <typeparam name="TRequest">The query to handle</typeparam>
/// <typeparam name="TSearchOptions">The options to complete the read process</typeparam>
/// <typeparam name="TResult">The expected result</typeparam>
public abstract class ReadHandler<TRequest, TSearchOptions, TResult> : IHandler<TRequest, TResult>
    where TRequest : IFeature<TResult>
{
    private readonly IReadRepository<TResult, TSearchOptions> _repository;

    /// <summary>
    /// Creates a new instance using the given <see cref="IReadRepository{TResult, TSearchOptions}"/>
    /// </summary>
    /// <param name="repository">Repository with read function</param>
    protected ReadHandler(IReadRepository<TResult, TSearchOptions> repository)
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

        var options = await RequestToSearchOptionsAsync(request, cancellationToken);

        return await _repository.ReadAsync(options, cancellationToken);
    }

    /// <summary>
    /// Validates the use case rules
    /// </summary>
    /// <param name="request">The request to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="ValueTask{T}"/> holding a <see cref="Result{TRequest}"/> of <see cref="Success"/> that represents the result of the operation </returns>
    protected internal abstract ValueTask<Result<Success>> ValidateFeatureRulesAsync(TRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Converts the <typeparamref name="TRequest"/> to <typeparamref name="TSearchOptions"/>
    /// </summary>
    /// <param name="request">The request to convert</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <typeparamref name="TSearchOptions"/></returns>
    protected internal abstract ValueTask<TSearchOptions> RequestToSearchOptionsAsync(TRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Base class for handlers of <see cref="IFeature{TResult}"/> that reads data.
/// </summary>
/// <typeparam name="TRequest">The query to handle</typeparam>
/// <typeparam name="TResult">The expected result</typeparam>

public abstract class ReadHandler<TRequest, TResult> : IHandler<TRequest, TResult>
    where TRequest : IFeature<TResult>
{
    private readonly IReadRepository<TResult, TRequest> _repository;

    /// <summary>
    /// Creates a new instance using the given <see cref="IReadRepository{TResult, TSearchOptions}"/>
    /// </summary>
    /// <param name="repository">Repository with read function</param>
    protected ReadHandler(IReadRepository<TResult, TRequest> repository)
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

        return await _repository.ReadAsync(request, cancellationToken);
    }

    /// <summary>
    /// Validates the use case rules
    /// </summary>
    /// <param name="request">The request to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="ValueTask{T}"/> holding a <see cref="Result{TRequest}"/> of <see cref="Success"/> that represents the result of the operation </returns>>
    protected internal abstract ValueTask<Result<Success>> ValidateFeatureRulesAsync(TRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Base class for handlers of <see cref="IFeature{TResult}"/> that reads basic data.
/// </summary>
/// <typeparam name="TRequest">The query to handle</typeparam>
/// <typeparam name="TResult">The expected result</typeparam>
public abstract class BasicReadHandler<TRequest, TResult> : IHandler<TRequest, TResult>
    where TRequest : IFeature<TResult>
{
    private readonly IReadRepository<TResult> _repository;

    /// <summary>
    /// Creates a new instance using the given <see cref="IReadRepository{TResult}"/>
    /// </summary>
    /// <param name="repository">Repository with read function</param>
    protected BasicReadHandler(IReadRepository<TResult> repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public virtual async ValueTask<Result<TResult>> HandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        var useCaseValidationResult = await ValidateFeatureRulesAsync(request, cancellationToken);

        if (useCaseValidationResult.IsFailure)
        {
            return useCaseValidationResult.Failure;
        }

        return await _repository.ReadAsync(cancellationToken);
    }

    /// <summary>
    /// Validates the use case rules
    /// </summary>
    /// <param name="request">The request to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="ValueTask{T}"/> holding a <see cref="Result{TRequest}"/> of <see cref="Success"/> that represents the result of the operation </returns>
    protected internal abstract ValueTask<Result<Success>> ValidateFeatureRulesAsync(TRequest request, CancellationToken cancellationToken);
}
