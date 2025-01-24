using VSlices.Base.Core;

namespace VSlices.Base.CrossCutting;

/// <summary>
/// Not intended to use in development, use <see cref="IBehaviorInterceptor{TRequest,TResult}" />
/// or <see cref="AbstractBehaviorInterceptor{TRequest,TResult}"/>
/// </summary>
public interface IBehaviorInterceptor;

/// <summary>
/// A behavior that intercepts <see cref="IBehavior{TIn,TOut}"/> implementations
/// </summary>
/// <typeparam name="TIn">The intercepted input</typeparam>
/// <typeparam name="TOut">The expected result</typeparam>
public interface IBehaviorInterceptor<in TIn, TOut> : IBehaviorInterceptor
{
    /// <summary>
    /// Defines the behavior of the interceptor
    /// </summary>
    /// <param name="request">The input to intercept</param>
    /// <param name="next">The next action in the chain</param>
    /// <returns>
    /// A <see cref="LanguageExt.Eff{T, T}"/> that represents the operation in lazy evaluation, which returns a <typeparamref name="TOut"/>
    /// </returns>
    Eff<VSlicesRuntime, TOut> Define(TIn request, Eff<VSlicesRuntime, TOut> next);
}
