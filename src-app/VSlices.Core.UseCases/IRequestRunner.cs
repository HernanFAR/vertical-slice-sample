using LanguageExt;

namespace VSlices.Core.UseCases;

/// <summary>
/// Represents an asynchronous runner for <see cref="IRequest{T}"/>
/// </summary>
public interface IRequestRunner
{
    /// <summary>
    /// Asynchronously runs the <see cref="IHandler{TRequest,TResult}"/> effect associated to <see cref="IRequest{T}" />
    /// </summary>
    /// <typeparam name="TResult">Expected response type</typeparam>
    /// <param name="request">Request to be handled</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>
    /// A <see cref="Aff{T}"/> that represents an asynchronous lazy operation which returns a
    /// <typeparamref name="TResult"/>.
    /// </returns>
    ValueTask<Fin<TResult>> RunAsync<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default);

}
