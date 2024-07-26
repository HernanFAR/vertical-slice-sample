using FluentValidation;
using FluentValidation.Results;
using LanguageExt;
using LanguageExt.Common;
using VSlices.Core;
using VSlices.Core.Stream;
using static LanguageExt.Prelude;
using static VSlices.CorePrelude;

namespace VSlices.CrossCutting.StreamPipeline.FluentValidation;

/// <summary>
/// A validation behavior that uses FluentValidation
/// </summary>
/// <typeparam name="TRequest">The intercepted request to validate</typeparam>
/// <typeparam name="TResult">The expected successful response</typeparam>
public sealed class FluentValidationStreamBehavior<TRequest, TResult> : AbstractStreamPipelineBehavior<TRequest, TResult>
    where TRequest : IStream<TResult>
{
    /// <inheritdoc />
    protected override Eff<HandlerRuntime, Unit> BeforeHandle(TRequest request) =>
        from validator in provide<IValidator<TRequest>>()
        from token in cancelToken
        from result in liftEff(async () => await validator.ValidateAsync(request, token))
        from _ in guard(result.IsValid, result.ToUnprocessable() as Error)
        select unit;
}