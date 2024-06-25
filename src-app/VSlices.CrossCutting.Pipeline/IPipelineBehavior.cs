﻿using LanguageExt;
using LanguageExt.SysX.Live;
using VSlices.Base;
using VSlices.Core;

namespace VSlices.CrossCutting.Pipeline;

/// <summary>
/// Not intended to be used in development, use <see cref="IPipelineBehavior{TRequest,TResult}" />
/// or <see cref="AbstractPipelineBehavior{TRequest,TResult}"/>
/// </summary>
public interface IPipelineBehavior;

/// <summary>
/// A middleware behavior for a <see cref="IHandler{TRequest}"/> <see cref="IFeature{TResult}" />
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
    /// A <see cref="Aff{T}"/> that represents the operation in lazy evaluation, which returns a <typeparamref name="TResult"/>
    /// </returns>
    Aff<Runtime, TResult> Define(TRequest request, Aff<Runtime, TResult> next);
}
