using FluentValidation;
using FluentValidation.Results;
using LanguageExt;
using VSlices.Base;
using VSlices.Base.CrossCutting;
using static LanguageExt.Prelude;
using static VSlices.VSlicesPrelude;

namespace VSlices.CrossCutting.Interceptor.FluentValidation;

/// <summary>
/// A validation behavior that uses FluentValidation
/// </summary>
/// <typeparam name="TIn">The intercepted input</typeparam>
/// <typeparam name="TOut">The expected result</typeparam>
public sealed class FluentValidationInterceptor<TIn, TOut> : AbstractBehaviorInterceptor<TIn, TOut>
{
    /// <inheritdoc />
    protected internal override Eff<VSlicesRuntime, Unit> BeforeHandle(TIn request) =>
        from validator in provide<IValidator<TIn>>()
        from token in cancelToken
        from result in liftEff(async () => await validator.ValidateAsync(request, token))
        from _ in guard(result.IsValid, result.ToUnprocessable())
        select unit;

}