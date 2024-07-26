using LanguageExt;

namespace VSlices.Core.Stream;

/// <summary>
/// Represents an asynchronous runner for <see cref="IStream{TResponse}"/>
/// </summary>
public interface IStreamRunner
{
    /// <summary>
    /// Asynchronously runs the <see cref="IHandler{TRequest,TResult}"/> effect associated to
    /// <see cref="IStream{TResponse}" />, using a specified runtime.
    /// </summary>
    /// <typeparam name="TResult">Expected response type</typeparam>
    /// <param name="request">Request to handle</param>
    /// <param name="runtime">Execution runtime</param>
    /// <returns>
    /// A <see cref="Fin{T}"/> that represents an asynchronous lazy operation, which returns a
    /// <typeparamref name="TResult"/>.
    /// </returns>
    Fin<IAsyncEnumerable<TResult>> Run<TResult>(IStream<TResult> request, 
                                                HandlerRuntime runtime);

    /// <summary>
    /// Asynchronously runs the <see cref="IHandler{TRequest,TResult}"/> effect associated to
    /// <see cref="IStream{T}" />, using generated runtime with the specified cancellation token
    /// </summary>
    /// <typeparam name="TResult">Expected response type</typeparam>
    /// <param name="request">Request to be handled</param>
    /// <param name="cancellationToken">Cancellation token to create a runtime of the handler</param>
    /// <returns>
    /// A <see cref="Fin{T}"/> that represents an asynchronous lazy operation, which returns a
    /// <typeparamref name="TResult"/>.
    /// </returns>
    Fin<IAsyncEnumerable<TResult>> Run<TResult>(IStream<TResult> request, 
                                                CancellationToken cancellationToken = default);

}
