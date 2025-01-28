using LanguageExt.Common;
using VSlices.Base.Core;
using VSlices.Base.Failures;

namespace VSlices.Base.CrossCutting;

/// <summary>
/// Abstract base class to simplify the implementations of <see cref="IBehaviorInterceptor{TIn, TOut}"/>
/// </summary>
/// <typeparam name="TIn">The intercepted input</typeparam>
/// <typeparam name="TOut">The expected result</typeparam>
public abstract class AbstractBehaviorInterceptor<TIn, TOut> : IBehaviorInterceptor<TIn, TOut>
{
    /// <summary>
    /// Executed before the next action, which might be another <see cref="IBehaviorInterceptor{TIn, TOut}"/>
    /// or the final <see cref="IBehavior{TIn, TOut}"/>
    /// </summary>
    /// <remarks>
    /// <para>
    /// If returns <see cref="Unit" /> it follows the usual flow
    /// </para>
    /// <para>
    /// If returns an instance of <see cref="ExtensibleExpected" /> the pipeline execution
    /// terminates with that response.
    /// </para>
    /// </remarks>
    /// <param name="request">The intercepted request</param>
    /// <returns>
    /// A <see cref="LanguageExt.Eff{TRuntime, TResult}"/> that represents the operation in lazy evaluation, which returns a <see cref="Unit" />
    /// </returns>
    protected internal virtual Eff<VSlicesRuntime, Unit> BeforeHandle(TIn request)
        => unitEff;

    /// <summary>
    /// The next action, which might be another <see cref="IBehaviorInterceptor{TIn, TOut}"/>
    /// or the final <see cref="IBehavior{TIn,TOut}"/>
    /// </summary>
    /// <remarks>
    /// <para>
    /// If returns the expected value, the execution is followed by <see cref="AfterSuccessHandling"/>
    /// </para>
    /// <para>
    /// If not, the execution is followed by <see cref="AfterFailureHandling"/>
    /// </para>
    /// </remarks>
    /// <param name="request">The intercepted request</param>
    /// <param name="next">The next action in the pipeline</param>
    /// <returns>
    /// A <see cref="LanguageExt.Eff{T}"/> that represents the operation in lazy evaluation, which returns a <typeparamref name="TOut" />
    /// </returns>
    protected internal virtual Eff<VSlicesRuntime, TOut> InHandle(TIn request, Eff<VSlicesRuntime, TOut> next) => next;

    /// <summary>
    /// Executed after the next action if returns the expected value
    /// </summary>
    /// <param name="request">The intercepted request</param>
    /// <param name="result">The result of the handler of the request</param>
    /// <returns>
    /// A <see cref="LanguageExt.Eff{T}"/> that represents the operation in lazy evaluation, which returns a <typeparamref name="TOut" />
    /// </returns>
    protected internal virtual Eff<VSlicesRuntime, TOut> AfterSuccessHandling(TIn request, TOut result) => SuccessEff(result);

    /// <summary>
    /// Executed after the next action if not returns the expected value
    /// </summary>
    /// <param name="request">The intercepted request</param>
    /// <param name="result">The result of the handler of the request</param>
    /// <returns>
    /// A <see cref="LanguageExt.Eff{T}"/> that represents the operation in lazy evaluation, which returns a <typeparamref name="TOut" />
    /// </returns>
    protected internal virtual Eff<VSlicesRuntime, TOut> AfterFailureHandling(TIn request, Error result)
        => liftEff<VSlicesRuntime, TOut>(_ => result);

    /// <inheritdoc />
    public Eff<VSlicesRuntime, TOut> Define(TIn request, Eff<VSlicesRuntime, TOut> next) =>
        from handleResult in BeforeHandle(request)
                             .Match(Succ: _ => InHandle(request, next)
                                               .Match(Succ: result => AfterSuccessHandling(request, result),
                                                      Fail: error => AfterFailureHandling(request, error))
                                               .Flatten(),
                                    Fail: FailEff<VSlicesRuntime, TOut>)
                             .Flatten()
        select handleResult;
}

