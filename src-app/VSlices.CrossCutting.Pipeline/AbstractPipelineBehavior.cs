using VSlices.Base;
using VSlices.Base.Responses;

namespace VSlices.CrossCutting.Pipeline;

/// <summary>
/// An abstract base class to simplify the implementations of <see cref="IPipelineBehavior{TRequest, TResult}"/>
/// </summary>
/// <typeparam name="TRequest">The request to intercept</typeparam>
/// <typeparam name="TResult">The expected result</typeparam>
public abstract class AbstractPipelineBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, TResult> 
    where TRequest : IFeature<TResult>
{
    /// <summary>
    /// A method that intercepts the pipeline
    /// </summary>
    /// <param name="request">The intercepted request</param>
    /// <param name="next">The next action in the pipeline</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// A <see cref="ValueTask{T}"/> that represents an asynchronous operation which returns a
    /// <see cref="Result{TRequest}"/> of <typeparamref name="TResult"/> that represents the result of the operation
    /// </returns>
    public virtual async ValueTask<Result<TResult>> HandleAsync(TRequest request, RequestHandlerDelegate<TResult> next, 
        CancellationToken cancellationToken)
    {
        var beforeResult = await BeforeHandleAsync(request, cancellationToken);

        if (beforeResult.IsFailure) return beforeResult.Failure;

        var inHandlerResult = await InHandleAsync(request, next, cancellationToken);

        await AfterHandleAsync(request, inHandlerResult, cancellationToken);

        return inHandlerResult;
    }

    /// <summary>
    /// A method that executes before the execution of the next action in the pipeline
    /// </summary>
    /// <remarks>
    /// <para>
    /// If this methods returns an instance of <see cref="Success" /> the next step is execute 
    /// <see cref="InHandleAsync(TRequest, RequestHandlerDelegate{TResult}, CancellationToken)" />
    /// </para>
    /// <para>
    /// If this methods returns an instance of <see cref="Failure" /> the pipeline execution is
    /// terminated with that response
    /// </para>
    /// </remarks>
    /// <param name="request">The intercepted request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// A <see cref="ValueTask{T}"/> that represents an asynchronous operation which returns a
    /// <see cref="Result{TRequest}"/> of <see cref="Success"/> that represents the result of the operation
    /// </returns>
    protected internal virtual ValueTask<Result<Success>> BeforeHandleAsync(TRequest request,
        CancellationToken cancellationToken) => new(Success.Value);

    /// <summary>
    /// A method that executes the next action in the pipeline
    /// </summary>
    /// <remarks>
    /// Regardless of the response, the method <see cref="AfterHandleAsync(TRequest, Result{TResult}, CancellationToken)" />
    /// </remarks>
    /// <param name="request">The intercepted request</param>
    /// <param name="next">The next action in the pipeline</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// A <see cref="ValueTask{T}"/> that represents an asynchronous operation which returns a
    /// <see cref="Result{TRequest}"/> of <typeparamref name="TResult"/> that represents the result of the operation
    /// </returns>
    protected internal virtual async ValueTask<Result<TResult>> InHandleAsync(TRequest request, RequestHandlerDelegate<TResult> next,
        CancellationToken cancellationToken) => await next();

    /// <summary>
    /// A method that executes after the execution of the next action in the pipeline
    /// </summary>
    /// <param name="request">The intercepted request</param>
    /// <param name="result">The result of the handler of the request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// A <see cref="ValueTask{T}"/> that represents an asynchronous operation which returns a
    /// <see cref="Result{TRequest}"/> of <see cref="Success"/> that represents the result of the operation
    /// </returns>
    protected internal virtual ValueTask AfterHandleAsync(TRequest request, Result<TResult> result,
        CancellationToken cancellationToken) => ValueTask.CompletedTask;

}