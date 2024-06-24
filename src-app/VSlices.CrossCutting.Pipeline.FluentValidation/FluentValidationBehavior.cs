using FluentValidation;
using FluentValidation.Results;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.SysX.Live;
using static LanguageExt.Prelude;
using VSlices.Base;
using VSlices.Base.Failures;

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
    protected override Aff<Runtime, Unit> BeforeHandle(TRequest request) =>
        from cancelToken in cancelToken<Runtime>()
        from validationResult in Aff(async () => await _requestValidator.ValidateAsync(request, cancelToken))
        from f in guard(validationResult.IsValid, validationResult.ToUnprocessable() as Error)
        select unit;

}