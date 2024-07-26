﻿using LanguageExt;
using static LanguageExt.Prelude;
using VSlices.Base;
using VSlices.Core;

namespace VSlices.CrossCutting.Pipeline.ExceptionHandling;

/// <summary>
/// Base exception handling behavior
/// </summary>
/// <typeparam name="TRequest">The intercepted request to handle</typeparam>
/// <typeparam name="TResult">The expected successful result</typeparam>
public abstract class AbstractExceptionHandlingBehavior<TRequest, TResult> : AbstractPipelineBehavior<TRequest, TResult>
    where TRequest : IFeature<TResult>
{
    /// <inheritdoc />
    protected override Eff<HandlerRuntime, TResult> InHandle(TRequest request, Eff<HandlerRuntime, TResult> next) =>
        from result in next | catchM(e => e.IsExceptional, 
                                     e => Process(e.ToException(), request))
        select result;

    /// <summary>
    /// Processes the exception
    /// </summary>
    /// <remarks>You can add more specific logging, email sending, etc. here</remarks>
    /// <param name="ex">The throw exception</param>
    /// <param name="request">The related request information</param>
    /// <returns>A <see cref="ValueTask"/> representing the processing of the exception</returns>
    protected internal abstract Eff<HandlerRuntime, TResult> Process(Exception ex, TRequest request);

}
