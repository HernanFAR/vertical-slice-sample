using VSlices.Base;
using VSlices.Base.Responses;

namespace VSlices.CrossCutting.Pipeline;

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
/// A middleware behavior for a <see cref="IFeature{TResult}"/>
/// </summary>
/// <typeparam name="TRequest">The request to intercept</typeparam>
/// <typeparam name="TResult">The expected result</typeparam>
public interface IPipelineBehavior<in TRequest, TResult>
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
    /// <see cref="Result{TRequest}"/> of <see cref="Success"/> that represents the result of the operation
    /// </returns>
    ValueTask<Result<TResult>> HandleAsync(TRequest request, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken);
}
 