using VSlices.Base;
using VSlices.Base.Responses;

namespace VSlices.Core;

/// <summary>
/// Defines a handler for a <see cref="IFeature{TResult}"/>
/// </summary>
/// <remarks>If idempotency is necessary, the handler itself must ensure it</remarks>
/// <typeparam name="TRequest">The request to be handled</typeparam>
/// <typeparam name="TResult">The expected response of the handler</typeparam>
public interface IHandler<in TRequest, TResult>
    where TRequest : IFeature<TResult>
{
    /// <summary>
    /// Handles the request
    /// </summary>
    /// <param name="request">The request to be handled</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// A <see cref="ValueTask{T}"/> that represents an asynchronous operation which returns a <see cref="Result{T}"/>
    /// of <see cref="Success"/> that represents the result of the operation
    /// </returns>
    ValueTask<Result<TResult>> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a handler for a <see cref="IFeature{TResult}"/>
/// </summary>
/// <typeparam name="TRequest">The request to be handled</typeparam>
public interface IHandler<in TRequest> : IHandler<TRequest, Success>
    where TRequest : IFeature<Success>
{ }
