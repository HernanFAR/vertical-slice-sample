﻿using FluentValidation;
using VSlices.Base;
using VSlices.Base.Responses;

namespace VSlices.CrossCutting.Pipeline.FluentValidation;

/// <summary>
/// A validation behavior that uses FluentValidation
/// </summary>
/// <typeparam name="TRequest">The intercepted request to validate</typeparam>
/// <typeparam name="TResult">The expected successful response</typeparam>
public sealed class FluentValidationBehavior<TRequest, TResult> : AbstractPipelineBehavior<TRequest, TResult>
    where TRequest : IFeature<TResult>
{
    private readonly IValidator<TRequest> _requestValidator;

    /// <summary>
    /// Creates a new instance using the validator registered in the container
    /// </summary>
    /// <param name="requestValidator">Validators registered</param>
    public FluentValidationBehavior(IValidator<TRequest> requestValidator)
    {
        _requestValidator = requestValidator;
    }

    /// <inheritdoc />
    protected override async ValueTask<Result<Success>> BeforeHandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _requestValidator.ValidateAsync(request, cancellationToken);

        if (validationResult.IsValid) return Success.Value;

        var errors = validationResult.Errors
            .Select(e => new ValidationError(e.PropertyName, e.ErrorMessage))
            .ToArray();

        return new Failure(FailureKind.ValidationError,
            Errors: errors);
    }
}