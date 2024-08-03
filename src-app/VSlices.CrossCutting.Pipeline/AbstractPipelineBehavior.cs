using LanguageExt;
using static LanguageExt.Prelude;
using LanguageExt.Common;
using VSlices.Base;
using VSlices.Base.Failures;
using VSlices.Core;

namespace VSlices.CrossCutting.Pipeline;

/// <summary>
/// Abstract base class to simplify the implementations of <see cref="IPipelineBehavior{TRequest, TResult}"/>
/// </summary>
/// <typeparam name="TRequest">The request to intercept</typeparam>
/// <typeparam name="TResult">The expected result</typeparam>
public abstract class AbstractPipelineBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, TResult> 
    where TRequest : IFeature<TResult>
{
    /// <summary>
    /// Executed before the next action, which might be another <see cref="IPipelineBehavior{TRequest, TResult}"/>
    /// or the final <see cref="IHandler{TRequest,TResult}"/>
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
    protected internal virtual Eff<VSlicesRuntime, Unit> BeforeHandle(TRequest request) 
        => unitEff;

    /// <summary>
    /// The next action, which might be another <see cref="IPipelineBehavior{TRequest, TResult}"/>
    /// or the final <see cref="IHandler{TRequest,TResult}"/>
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
    /// A <see cref="LanguageExt.Eff{T}"/> that represents the operation in lazy evaluation, which returns a <typeparamref name="TResult" />
    /// </returns>
    protected internal virtual Eff<VSlicesRuntime, TResult> InHandle(TRequest request, Eff<VSlicesRuntime, TResult> next) => next;

    /// <summary>
    /// Executed after the next action if returns the expected value
    /// </summary>
    /// <param name="request">The intercepted request</param>
    /// <param name="result">The result of the handler of the request</param>
    /// <returns>
    /// A <see cref="LanguageExt.Eff{T}"/> that represents the operation in lazy evaluation, which returns a <typeparamref name="TResult" />
    /// </returns>
    protected internal virtual Eff<VSlicesRuntime, TResult> AfterSuccessHandling(TRequest request, TResult result) => SuccessEff(result);

    /// <summary>
    /// Executed after the next action if not returns the expected value
    /// </summary>
    /// <param name="request">The intercepted request</param>
    /// <param name="result">The result of the handler of the request</param>
    /// <returns>
    /// A <see cref="LanguageExt.Eff{T}"/> that represents the operation in lazy evaluation, which returns a <typeparamref name="TResult" />
    /// </returns>
    protected internal virtual Eff<VSlicesRuntime, TResult> AfterFailureHandling(TRequest request, Error result) 
        => liftEff<VSlicesRuntime, TResult>(_ => result); 

    /// <inheritdoc />
    public Eff<VSlicesRuntime, TResult> Define(TRequest request, Eff<VSlicesRuntime, TResult> next) =>
        from handleResult in BeforeHandle(request)
                             .Match(Succ: _ => InHandle(request, next)
                                               .Match(Succ: result => AfterSuccessHandling(request, result),
                                                      Fail: error  => AfterFailureHandling(request, error))
                                               .Flatten(), 
                                    Fail: FailEff<VSlicesRuntime, TResult>)
                             .Flatten()
        select handleResult;
}

