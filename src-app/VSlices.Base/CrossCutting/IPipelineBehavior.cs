﻿using VSlices.Base.Core;

namespace VSlices.Base.CrossCutting;

/// <summary>
/// Not intended to use in development, use <see cref="IPipelineBehavior{TRequest,TResult}" />
/// or <see cref="AbstractPipelineBehavior{TRequest,TResult}"/>
/// </summary>
public interface IPipelineBehavior;

/// <summary>
/// A middleware behavior for a <see cref="IHandler{TRequest, TResult}"/> <see cref="IFeature{TResult}" />
/// </summary>
/// <typeparam name="TRequest">The request to intercept</typeparam>
/// <typeparam name="TResult">The expected result</typeparam>
public interface IPipelineBehavior<in TRequest, TResult> : IPipelineBehavior
    where TRequest : IFeature<TResult>
{
    /// <summary>
    /// A method that intercepts the pipeline
    /// </summary>
    /// <param name="request">The intercepted request</param>
    /// <param name="next">The next action in the pipeline</param>
    /// <returns>
    /// A <see cref="LanguageExt.Eff{T, T}"/> that represents the operation in lazy evaluation, which returns a <typeparamref name="TResult"/>
    /// </returns>
    Eff<VSlicesRuntime, TResult> Define(TRequest request, Eff<VSlicesRuntime, TResult> next);
}