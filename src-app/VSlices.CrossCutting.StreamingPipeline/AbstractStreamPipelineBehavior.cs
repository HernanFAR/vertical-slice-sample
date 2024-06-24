using LanguageExt;
using LanguageExt.Common;
using LanguageExt.SysX.Live;
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
    /// <see cref="InHandle" />
    /// </para>
    /// <para>
    /// If this methods returns an instance of <see cref="ExtensibleExpectedError" /> the pipeline execution is
    /// terminated with that response
    /// </para>
    /// </remarks>
    /// <param name="request">The intercepted request</param>
    /// <returns>
    /// A <see cref="LanguageExt.Aff{T}"/> that represents the operation in lazy evaluation, which returns a <see cref="Unit" />
    /// </returns>
    protected internal virtual Aff<Runtime, Unit> BeforeHandle(TRequest request) => unitAff;

    /// <summary>
    /// A method that executes the next action in the pipeline
    /// </summary>
    /// <remarks>
    /// If success, the next step is execute <see cref="AfterSuccessHandling" />, if failure, <see cref="AfterFailureHandling" />
    /// </remarks>
    /// <param name="request">The intercepted request</param>
    /// <param name="next">The next action in the pipeline</param>
    /// <returns>
    /// A <see cref="LanguageExt.Aff{T}"/> that represents the operation in lazy evaluation, which returns a <typeparamref name="TResult" />
    /// </returns>
    protected internal virtual Aff<Runtime, IAsyncEnumerable<TResult>> InHandle(
        TRequest request, 
        Aff<Runtime, IAsyncEnumerable<TResult>> next) => next;

    /// <summary>
    /// A method that executes after a success execution of the decorated <see cref="IHandler{TRequest, TResult}"/>
    /// </summary>
    /// <param name="request">The intercepted request</param>
    /// <param name="result">The result of the handler of the request</param>
    /// <returns>
    /// A <see cref="LanguageExt.Aff{T}"/> that represents the operation in lazy evaluation, which returns a <typeparamref name="TResult" />
    /// </returns>
    protected internal virtual Aff<Runtime, IAsyncEnumerable<TResult>> AfterSuccessHandling(
        TRequest request, 
        IAsyncEnumerable<TResult> result) => SuccessAff(result);

    /// <summary>
    /// A method that executes after a fail execution of the decorated <see cref="IHandler{TRequest, TResult}"/>
    /// </summary>
    /// <param name="request">The intercepted request</param>
    /// <param name="result">The result of the handler of the request</param>
    /// <returns>
    /// A <see cref="LanguageExt.Aff{T}"/> that represents the operation in lazy evaluation, which returns a <typeparamref name="TResult" />
    /// </returns>
    protected internal virtual Aff<Runtime, IAsyncEnumerable<TResult>> AfterFailureHandling(
        TRequest request, 
        Error result) 
        => FailAff<IAsyncEnumerable<TResult>>(result); 

    /// <inheritdoc />
    public Aff<Runtime, IAsyncEnumerable<TResult>> Define(TRequest request, Aff<Runtime, IAsyncEnumerable<TResult>> next) =>
        from cancelToken in cancelToken<Runtime>()
        from handleResult in BeforeHandle(request)
            .BiBind(
                Succ: _ => InHandle(request, next)
                    .BiBind(
                        Succ: result => AfterSuccessHandling(request, result),
                        Fail: error  => AfterFailureHandling(request, error)),
                Fail: FailAff<Runtime, IAsyncEnumerable<TResult>>)
        select handleResult;
}