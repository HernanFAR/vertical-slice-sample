using FluentValidation;
using VSlices.Base;
using VSlices.Base.Responses;
using VSlices.Core.DataAccess;

namespace VSlices.Core.Handlers;

/// <summary>
/// Base class for handlers of <see cref="IFeature{TResult}"/> that validates entities with Fluent Validation, before update them
/// </summary>
/// <remarks>Useful to implement always valid entities</remarks>
/// <typeparam name="TRequest">The command to handle</typeparam>
/// <typeparam name="TResult">The expected result</typeparam>
/// <typeparam name="TEntity">The entity to update</typeparam>
public abstract class EntityFluentValidatedUpdateHandler<TRequest, TResult, TEntity> : EntityValidatedUpdateHandler<TRequest, TResult, TEntity>
    where TRequest : IFeature<TResult>
{
    private readonly IValidator<TEntity> _entityValidator;

    /// <summary>
    /// Creates a new instance using the given <see cref="IUpdateRepository{TEntity}"/>
    /// </summary>
    /// <param name="entityValidator">Validator of the entity</param>
    /// <param name="repository">Repository with update function</param>
    protected EntityFluentValidatedUpdateHandler(IValidator<TEntity> entityValidator, IUpdateRepository<TEntity> repository) : base(repository)
    {
        _entityValidator = entityValidator;
    }

    /// <inheritdoc />
    protected override async ValueTask<Result<Success>> ValidateEntityAsync(TEntity domain, CancellationToken cancellationToken)
    {
        var domainValidationResult = await _entityValidator.ValidateAsync(domain, cancellationToken);

        if (domainValidationResult.IsValid) return Success.Value;

        var errors = domainValidationResult
            .Errors
            .Select(e => new ValidationError(e.PropertyName, e.ErrorMessage))
            .ToArray();

        return new Failure(FailureKind.ValidationError,
            Errors: errors);
    }
}

/// <summary>
/// Base class for handlers of <see cref="IFeature{Success}"/> that validates entities with Fluent Validation, before update them
/// </summary>
/// <remarks>Useful in commands without result and to implement always-valid entities</remarks>
/// <typeparam name="TRequest">The command to handle</typeparam>
/// <typeparam name="TEntity">The entity to update</typeparam>
public abstract class EntityFluentValidatedUpdateHandler<TRequest, TEntity> : EntityValidatedUpdateHandler<TRequest, TEntity>
    where TRequest : IFeature<Success>
{
    private readonly IValidator<TEntity> _entityValidator;

    /// <summary>
    /// Creates a new instance using the given <see cref="IUpdateRepository{TEntity}"/>
    /// </summary>
    /// <param name="entityValidator">Validator of the entity</param>
    /// <param name="repository">Repository with update function</param>
    protected EntityFluentValidatedUpdateHandler(IValidator<TEntity> entityValidator, IUpdateRepository<TEntity> repository) : base(repository)
    {
        _entityValidator = entityValidator;
    }

    /// <inheritdoc />
    protected override async ValueTask<Result<Success>> ValidateEntityAsync(TEntity domain, CancellationToken cancellationToken)
    {
        var domainValidationResult = await _entityValidator.ValidateAsync(domain, cancellationToken);

        if (domainValidationResult.IsValid) return Success.Value;

        var errors = domainValidationResult
            .Errors
            .Select(e => new ValidationError(e.PropertyName, e.ErrorMessage))
            .ToArray();

        return new Failure(FailureKind.ValidationError,
            Errors: errors);
    }
}
