﻿using LanguageExt;
using LanguageExt.Common;
using VSlices.Base;
using VSlices.Base.Failures;
using VSlices.Core;
using VSlices.Core.Stream;
using static LanguageExt.Prelude;

namespace VSlices.CrossCutting.StreamPipeline;

/// <summary>
/// An abstract base class to simplify the implementations of <see cref="IStreamPipelineBehavior{TRequest, TResult}"/>
/// </summary>
/// <typeparam name="TRequest">The request to intercept</typeparam>
/// <typeparam name="TResult">The expected result</typeparam>
public abstract class AbstractStreamPipelineBehavior<TRequest, TResult> : IStreamPipelineBehavior<TRequest, TResult> 
    where TRequest : IStream<TResult>
{
    /// <summary>
    /// A method that executes before the execution of the next action in the pipeline
    /// </summary>
    /// <remarks>
    /// <para>
    /// If this methods returns <see cref="Unit" /> the next step is execute 
    /// <see cref="InHandleAsync" />
    /// </para>
    /// <para>
    /// If this methods returns an instance of <see cref="ExtensibleExpectedError" /> the pipeline execution is
    /// terminated with that response
    /// </para>
    /// </remarks>
    /// <param name="request">The intercepted request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// A <see cref="LanguageExt.Aff{T}"/> that represents the operation in lazy evaluation, which returns a <see cref="Unit" />
    /// </returns>
    protected internal virtual Aff<Unit> BeforeHandleAsync(TRequest request, CancellationToken cancellationToken) => unitAff;

    /// <summary>
    /// A method that executes the next action in the pipeline
    /// </summary>
    /// <remarks>
    /// If success, the next step is execute <see cref="AfterSuccessHandlingAsync" />, if failure, <see cref="AfterFailureHandlingAsync" />
    /// </remarks>
    /// <param name="request">The intercepted request</param>
    /// <param name="next">The next action in the pipeline</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// A <see cref="LanguageExt.Aff{T}"/> that represents the operation in lazy evaluation, which returns a <typeparamref name="TResult" />
    /// </returns>
    protected internal virtual Aff<IAsyncEnumerable<TResult>> InHandleAsync(
        TRequest request, 
        Aff<IAsyncEnumerable<TResult>> next, 
        CancellationToken cancellationToken) => next;

    /// <summary>
    /// A method that executes after a success execution of the decorated <see cref="IHandler{TRequest, TResult}"/>
    /// </summary>
    /// <param name="request">The intercepted request</param>
    /// <param name="result">The result of the handler of the request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// A <see cref="LanguageExt.Aff{T}"/> that represents the operation in lazy evaluation, which returns a <typeparamref name="TResult" />
    /// </returns>
    protected internal virtual Aff<IAsyncEnumerable<TResult>> AfterSuccessHandlingAsync(
        TRequest request, 
        IAsyncEnumerable<TResult> result, 
        CancellationToken cancellationToken) => SuccessAff(result);

    /// <summary>
    /// A method that executes after a fail execution of the decorated <see cref="IHandler{TRequest, TResult}"/>
    /// </summary>
    /// <param name="request">The intercepted request</param>
    /// <param name="result">The result of the handler of the request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// A <see cref="LanguageExt.Aff{T}"/> that represents the operation in lazy evaluation, which returns a <typeparamref name="TResult" />
    /// </returns>
    protected internal virtual Aff<IAsyncEnumerable<TResult>> AfterFailureHandlingAsync(
        TRequest request, 
        Error result, 
        CancellationToken cancellationToken) => FailAff<IAsyncEnumerable<TResult>>(result); 

    /// <inheritdoc />
    public Aff<IAsyncEnumerable<TResult>> Define(TRequest request, Aff<IAsyncEnumerable<TResult>> next, CancellationToken cancellationToken) =>
        from handleResult in BeforeHandleAsync(request, cancellationToken)
            .BiBind(
                Succ: _ => InHandleAsync(request, next, cancellationToken)
                    .BiBind(
                        Succ: result => AfterSuccessHandlingAsync(request, result, cancellationToken),
                        Fail: error  => AfterFailureHandlingAsync(request, error, cancellationToken)),
                Fail: FailAff<IAsyncEnumerable<TResult>>)
        select handleResult;
}