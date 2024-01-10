using VSlices.Base;
using VSlices.Base.Responses;

namespace VSlices.CrossCutting;

/// <summary>
/// A delegate that represents the next action in the pipeline
/// </summary>
/// <typeparam name="T">The response of the next action</typeparam>
/// <returns>
/// A <see cref="ValueTask{TResult}"/> that represents an asynchronous operation which returns a
/// <see cref="Result{TRequest}"/> of <see cref="Success"/> that represents the result of the next action
/// </returns>
public delegate ValueTask<Result<T>> RequestHandlerDelegate<T>();

/// <summary>
/// A middleware behavior for a <see cref="IBaseRequest{TResult}"/>
/// </summary>
/// <typeparam name="TRequest">The request to intercept</typeparam>
/// <typeparam name="TResult">The expected result</typeparam>
public interface IPipelineBehavior<in TRequest, TResult>
    where TRequest : IBaseRequest<TResult>
{
    /// <summary>
    /// A method that intercepts the pipeline
    /// </summary>
    /// <param name="request">The intercepted request</param>
    /// <param name="next">The next action in the pipeline</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// A <see cref="ValueTask{T}"/> that represents an asynchronous operation which returns a
    /// <see cref="Result{TRequest}"/> of <see cref="Success"/> that represents the result of the operation
    /// </returns>
    ValueTask<Result<TResult>> HandleAsync(TRequest request, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken);
}

/// <summary>
/// An abstract base class to simplify the implementations of <see cref="IPipelineBehavior{TRequest, TResult}"/>
/// </summary>
/// <typeparam name="TRequest">The request to intercept</typeparam>
/// <typeparam name="TResult">The expected result</typeparam>
public abstract class AbstractPipelineBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, TResult> 
    where TRequest : IBaseRequest<TResult>
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
    public async ValueTask<Result<TResult>> HandleAsync(TRequest request, RequestHandlerDelegate<TResult> next, 
        CancellationToken cancellationToken)
    {
        var beforeResult = await BeforeHandleAsync(request, cancellationToken);

        if (beforeResult.IsFailure) return beforeResult.Failure;

        var result = await InHandleAsync(request, next, cancellationToken);

        var afterResult = await AfterHandleAsync(request, result, cancellationToken);

        return afterResult.IsSuccess 
            ? result
            : beforeResult.Failure;
    }

    /// <summary>
    /// A method that executes before the execution of the next action in the pipeline
    /// </summary>
    /// <param name="request">The intercepted request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// A <see cref="ValueTask{T}"/> that represents an asynchronous operation which returns a
    /// <see cref="Result{TRequest}"/> of <see cref="Success"/> that represents the result of the operation
    /// </returns>
    protected virtual async ValueTask<Result<Success>> BeforeHandleAsync(TRequest request, CancellationToken cancellationToken) 
        => Success.Value;

    /// <summary>
    /// A method that executes the next action in the pipeline
    /// </summary>
    /// <param name="request">The intercepted request</param>
    /// <param name="next">The next action in the pipeline</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// A <see cref="ValueTask{T}"/> that represents an asynchronous operation which returns a
    /// <see cref="Result{TRequest}"/> of <typeparamref name="TResult"/> that represents the result of the operation
    /// </returns>
    protected virtual async ValueTask<Result<TResult>> InHandleAsync(TRequest request, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken)
    {
        return await next();
    }

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
    protected virtual async ValueTask<Result<Success>> AfterHandleAsync(TRequest request, Result<TResult> result, CancellationToken cancellationToken) 
        => Success.Value;

}