using LanguageExt;

namespace VSlices.Core.Stream;

/// <summary>
/// Represents an asynchronous runner for <see cref="IStream{TResponse}"/>
/// </summary>
public interface IStreamRunner
{
    /// <summary>
    /// Asynchronously runs the <see cref="IHandler{TRequest,TResult}"/> effect associated to <see cref="IStream{TResponse}" />
    /// </summary>
    /// <typeparam name="TResult">Expected response type</typeparam>
    /// <param name="request">Request to be handled</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>
    /// A <see cref="Aff{T}"/> that represents an asynchronous lazy operation which returns a
    /// <typeparamref name="TResult"/>.
    /// </returns>
    ValueTask<Fin<IAsyncEnumerable<TResult>>> RunAsync<TResult>(IStream<TResult> request, CancellationToken cancellationToken = default);

}
