using VSlices.Base.Responses;

namespace VSlices.Core.UseCases;

/// <summary>
/// Sends a request to be handled by a single handler
/// </summary>
public interface ISender
{
    /// <summary>
    /// Asynchronously sends a request to a handler
    /// </summary>
    /// <typeparam name="TResult">Expected response type</typeparam>
    /// <param name="request">Request to be handled</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>A <see cref="ValueTask{T}"/> that represents an asynchronous operations which returns a <see cref="Result{T}"/> of <see cref="Success"/> that represents the result of the operation </returns>
    ValueTask<Result<TResult>> SendAsync<TResult>(IRequest<TResult> request, CancellationToken cancellationToken);

}
