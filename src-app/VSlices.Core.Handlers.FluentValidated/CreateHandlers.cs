using FluentValidation;
using VSlices.Base;
using VSlices.Base.Responses;
using VSlices.Core.DataAccess;

namespace VSlices.Core.Handlers;

/// <summary>
/// Base class for handlers of <see cref="IFeature{TResult}"/> that validate entities with Fluent Validation, before create them
/// </summary>
/// <remarks>Useful to implement always valid entities</remarks>
/// <typeparam name="TRequest">The request to handle</typeparam>
/// <typeparam name="TResult">The expected result</typeparam>
/// <typeparam name="TEntity">The entity to create</typeparam>
public abstract class EntityFluentValidatedCreateHandler<TRequest, TResult, TEntity>
    : EntityValidatedCreateHandler<TRequest, TResult, TEntity>
    where TRequest : IFeature<TResult>
{
    private readonly IValidator<TEntity> _validator;

    /// <summary>
    /// Creates a new instance using the given <see cref="validator"/>
    /// </summary>
    /// <param name="validator">Validator of the entity</param>
    /// <param name="repository">Repository with create function</param>
    protected EntityFluentValidatedCreateHandler(IValidator<TEntity> validator,
        ICreateRepository<TEntity> repository) : base(repository)
    {
        _validator = validator;
    }

    /// <inheritdoc />
    protected override async ValueTask<Result<Success>> ValidateEntityAsync(TEntity domain, CancellationToken cancellationToken)
    {
        var domainValidationResult = await _validator.ValidateAsync(domain, cancellationToken);

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
/// Base class for handlers of <see cref="IFeature{TResult}"/> that validates entities with Fluent Validation, before create them
/// </summary>
/// <remarks>Useful in commands without result and to implement always-valid entities</remarks>
/// <typeparam name="TRequest">The request to handle</typeparam>
/// <typeparam name="TEntity">The entity to create</typeparam>
public abstract class EntityFluentValidatedCreateHandler<TRequest, TEntity> : EntityValidatedCreateHandler<TRequest, TEntity>
    where TRequest : IFeature<Success>
{
    private readonly IValidator<TEntity> _entityValidator;

    /// <summary>
    /// Creates a new instance using the given <see cref="ICreateRepository{TEntity}"/>
    /// </summary>
    /// <param name="entityValidator">Validator of the entity</param>
    /// <param name="repository">Repository with create function</param>
    protected EntityFluentValidatedCreateHandler(IValidator<TEntity> entityValidator, ICreateRepository<TEntity> repository) : base(repository)
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
